// FighterPort Studio addition, 2026-10-17. Licensed under the repository LGPL-3.0 terms.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BrawlLib.Internal.IO
{
    /// <summary>Process-wide native-output guard. Source roots remain protected across restarts.</summary>
    public static class SourceWriteGuard
    {
        private static readonly object Sync = new object();
        private static readonly HashSet<string> Roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static string _registryFile;
        private static string _mutexName;
        private static Exception _initializationError;

        public static void Initialize(string registryFile)
        {
            lock (Sync)
            {
                string full = NormalizePath(registryFile);
                if (_registryFile != null)
                {
                    if (!string.Equals(_registryFile, full, StringComparison.OrdinalIgnoreCase))
                        throw new IOException("Source protection is already initialized with another registry.");
                    CheckInitialization();
                    return;
                }
                _registryFile = full;
                using (SHA256 hash = SHA256.Create())
                    _mutexName = "Local\\FighterPortStudio.SourceRoots." + BitConverter.ToString(
                        hash.ComputeHash(Encoding.UTF8.GetBytes(full.ToUpperInvariant()))).Replace("-", "");
                try
                {
                    WithRegistryLock(() => { RefreshRoots(); return true; });
                }
                catch (Exception error)
                {
                    _initializationError = error;
                    throw new IOException("Source protection could not load. Native writes are disabled.", error);
                }
            }
        }

        private static void EnsureInitialized()
        {
            if (_registryFile == null)
                Initialize(Path.Combine(FighterPortPaths.DataRoot, "source-roots.txt"));
            CheckInitialization();
        }

        public static void InitializeDefault()
        {
            lock (Sync) EnsureInitialized();
        }

        private static void CheckInitialization()
        {
            if (_initializationError != null)
                throw new IOException("Source protection could not load. Native writes are disabled.", _initializationError);
        }

        // All instances using the same data root merge mounts and see newly mounted roots before writing.
        // There is deliberately no unmount operation that could silently remove another instance's protection.
        private static void RefreshRoots()
        {
            EnsureNoReparsePoints(_registryFile);
            if (!File.Exists(_registryFile))
            {
                if (Roots.Count != 0) throw new IOException("The protected source-root registry is missing. Restart after restoring it.");
                return;
            }
            if (new FileInfo(_registryFile).Length > 1024 * 1024)
                throw new IOException("The source-root registry exceeds its safety limit.");
            string[] lines = File.ReadAllLines(_registryFile, Encoding.UTF8);
            if (lines.Length > 256) throw new IOException("Too many protected source roots.");
            foreach (string line in lines)
                if (!string.IsNullOrWhiteSpace(line)) Roots.Add(NormalizePath(line));
            if (Roots.Count > 256) throw new IOException("Too many protected source roots across application instances.");
        }

        private static T WithRegistryLock<T>(Func<T> action)
        {
            using (var mutex = new Mutex(false, _mutexName))
            {
                bool acquired = false;
                try
                {
                    try { acquired = mutex.WaitOne(TimeSpan.FromSeconds(10)); }
                    catch (AbandonedMutexException) { acquired = true; }
                    if (!acquired) throw new IOException("Source protection is busy in another application instance.");
                    return action();
                }
                finally { if (acquired) mutex.ReleaseMutex(); }
            }
        }

        public static void RegisterRoot(string root)
        {
            lock (Sync)
            {
                EnsureInitialized();
                WithRegistryLock(() =>
                {
                    RefreshRoots();
                    string full = NormalizePath(root);
                    EnsureNoReparsePoints(full);
                    if (!Directory.Exists(full)) throw new DirectoryNotFoundException(full);
                    if (IsWithin(_registryFile, full))
                        throw new IOException("Select a specific source folder outside FighterPort Studio's settings and staging folder.");
                    if (Roots.Contains(full)) return true;
                    if (Roots.Count >= 256) throw new IOException("The 256 source-root limit has been reached.");
                    Roots.Add(full); // Protection remains active even if persistence fails.
                    string parent = Path.GetDirectoryName(_registryFile);
                    string temporary = _registryFile + "." + Guid.NewGuid().ToString("N") + ".tmp";
                    try
                    {
                        EnsureNoReparsePoints(parent);
                        Directory.CreateDirectory(parent);
                        File.WriteAllLines(temporary, Roots.OrderBy(x => x, StringComparer.OrdinalIgnoreCase), Encoding.UTF8);
                        if (File.Exists(_registryFile)) File.Replace(temporary, _registryFile, null);
                        else File.Move(temporary, _registryFile);
                    }
                    catch (Exception error)
                    {
                        _initializationError = error;
                        throw new IOException("The protected source root could not be saved. Native writes are disabled.", error);
                    }
                    finally { if (File.Exists(temporary)) File.Delete(temporary); }
                    return true;
                });
            }
        }

        public static void ThrowIfProtected(string outputPath)
        {
            lock (Sync)
            {
                EnsureInitialized();
                WithRegistryLock(() =>
                {
                    RefreshRoots();
                    string full = NormalizePath(outputPath);
                    // Reject all junction/symlink outputs: a different name must not bypass a source mount.
                    EnsureNoReparsePoints(full);
                    if (Roots.Any(root => IsWithin(full, root)))
                        throw new IOException("Source folders are read-only. Save or export to a different folder: " + full);
                    if (File.Exists(full))
                    {
                        // An outside hard link could otherwise modify a mounted original through an alias.
                        using (FileStream stream = new FileStream(full, FileMode.Open, FileAccess.Read,
                            FileShare.ReadWrite | FileShare.Delete))
                        {
                            FileInformation info;
                            if (!GetFileInformationByHandle(stream.SafeFileHandle.DangerousGetHandle(), out info))
                                throw new IOException("Unable to check output-file identity.");
                            if (info.NumberOfLinks > 1)
                                throw new IOException("Writing through a hard link is disabled. Choose a new output file.");
                        }
                    }
                    return true;
                });
            }
        }

        public static bool IsWithin(string path, string root)
        {
            return string.Equals(path, root, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase);
        }

        public static void ThrowIfProtected(FileStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            lock (Sync)
            {
                EnsureInitialized();
                WithRegistryLock(() =>
                {
                    RefreshRoots();
                    var handleName = new StringBuilder(32768);
                    uint nameLength = GetFinalPathNameByHandle(stream.SafeFileHandle.DangerousGetHandle(), handleName,
                        (uint)handleName.Capacity, 0);
                    if (nameLength == 0 || nameLength >= handleName.Capacity)
                        throw new IOException("Unable to resolve the output stream's file identity.");
                    string resolvedName = handleName.ToString();
                    if (resolvedName.StartsWith(@"\\?\", StringComparison.Ordinal)) resolvedName = resolvedName.Substring(4);
                    if (resolvedName.Length < 3 || resolvedName[1] != ':' || resolvedName[2] != '\\')
                        throw new IOException("Only local disk output streams are supported for source-safe editing.");
                    string full = NormalizePath(resolvedName);
                    EnsureNoReparsePoints(full);
                    if (Path.IsPathRooted(stream.Name))
                    {
                        EnsureNoReparsePoints(stream.Name);
                        string openedName = NormalizePath(stream.Name);
                        if (Roots.Any(root => IsWithin(openedName, root)))
                            throw new IOException("The output stream was opened through a protected source path.");
                    }
                    if (Roots.Any(root => IsWithin(full, root)))
                        throw new IOException("Source folders are read-only. Save or export to a different folder: " + full);
                    FileInformation info;
                    if (!GetFileInformationByHandle(stream.SafeFileHandle.DangerousGetHandle(), out info) || info.NumberOfLinks > 1)
                        throw new IOException("Unable to verify an independent output file. Choose a new destination.");
                    return true;
                });
            }
        }

        public static string ProtectOutputPath(string path)
        {
            ThrowIfProtected(path);
            return path;
        }

        public static string ProtectDeletionPath(string path)
        {
            ThrowIfProtected(path);
            string full = NormalizePath(path);
            lock (Sync)
                if (Roots.Any(root => IsWithin(root, full)))
                    throw new IOException("This folder contains a protected source root and cannot be deleted.");
            return path;
        }

        public static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new IOException("A local path is required.");
            string full = Path.GetFullPath(path);
            if (full.StartsWith(@"\\", StringComparison.Ordinal) || full.IndexOf(':', 2) >= 0)
                throw new IOException("Network/device paths and alternate streams are not supported for source-safe editing.");
            // Expand existing 8.3 ancestors so short-name aliases cannot bypass a mount.
            string existing = full;
            var tail = new Stack<string>();
            while (!File.Exists(existing) && !Directory.Exists(existing))
            {
                string parent = Path.GetDirectoryName(existing);
                if (string.IsNullOrEmpty(parent) || parent == existing) break;
                tail.Push(Path.GetFileName(existing));
                existing = parent;
            }
            var longName = new StringBuilder(32768);
            uint length = GetLongPathName(existing, longName, (uint) longName.Capacity);
            if (length > 0 && length < longName.Capacity) existing = longName.ToString();
            while (tail.Count > 0) existing = Path.Combine(existing, tail.Pop());
            return existing.Length > 3 ? existing.TrimEnd(Path.DirectorySeparatorChar) : existing;
        }

        public static void EnsureNoReparsePoints(string path)
        {
            string current = Path.GetFullPath(path);
            while (!string.IsNullOrEmpty(current))
            {
                try
                {
                    if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                        throw new IOException("Symbolic links and reparse points are not supported: " + current);
                }
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
                string parent = Path.GetDirectoryName(current);
                if (parent == current) break;
                current = parent;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetLongPathName(string path, StringBuilder result, uint capacity);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(IntPtr handle, out FileInformation information);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetFinalPathNameByHandle(IntPtr handle, StringBuilder path, uint capacity, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct FileInformation
        {
            public uint Attributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime, AccessTime, WriteTime;
            public uint VolumeSerialNumber, FileSizeHigh, FileSizeLow, NumberOfLinks, FileIndexHigh, FileIndexLow;
        }
    }
}
