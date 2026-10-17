// FighterPort Studio addition, 2026-10-17. Licensed under the repository LGPL-3.0 terms.
using BrawlLib.Internal.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BrawlCrate.Sources
{
    public sealed class SourceEntry
    {
        public string Path { get; internal set; }
        public string Name { get { return System.IO.Path.GetFileName(Path); } }
        public bool IsDirectory { get; internal set; }
        public bool CanOpen { get; internal set; }
        public string Description { get; internal set; }
    }

    public sealed class StagedSource
    {
        public string SourcePath { get; internal set; }
        public string StagedPath { get; internal set; }
        public string Sha256 { get; internal set; }
        public long Length { get; internal set; }
    }

    /// <summary>Bounded loose-folder source slice. No archive extraction, decryption, script execution or guessed companions.</summary>
    public sealed class SourceFolderSession
    {
        public const long MaxFileBytes = 64L * 1024 * 1024;
        public const int MaxDirectoryEntries = 512;
        public const long MaxStagingBytes = 1024L * 1024 * 1024;
        private static readonly HashSet<string> RawTags = new HashSet<string>(StringComparer.Ordinal)
            {"MDL0", "CHR0", "TEX0", "PLT0", "SRT0", "PAT0", "CLR0", "VIS0", "SHP0", "SCN0"};
        private readonly string _storageRoot;
        public string RootPath { get; private set; }

        public SourceFolderSession(string root, string storageRoot = null)
        {
            RootPath = SourceWriteGuard.NormalizePath(root);
            SourceWriteGuard.EnsureNoReparsePoints(RootPath);
            if (!Directory.Exists(RootPath)) throw new DirectoryNotFoundException(RootPath);
            string applicationRoot = SourceWriteGuard.NormalizePath(AppDomain.CurrentDomain.BaseDirectory);
            if (SourceWriteGuard.IsWithin(RootPath, applicationRoot) || SourceWriteGuard.IsWithin(applicationRoot, RootPath))
                throw new IOException("The application's installation and helper folders cannot be mounted as read-only game sources. Choose a separate asset folder outside the application directory.");
            string temporaryRoot = SourceWriteGuard.NormalizePath(System.IO.Path.GetTempPath());
            if (SourceWriteGuard.IsWithin(RootPath, temporaryRoot) || SourceWriteGuard.IsWithin(temporaryRoot, RootPath))
                throw new IOException("The operating system's temporary working folder cannot be mounted as a read-only game source. Choose a separate asset folder outside the temporary directory.");
            string settingsRoot = SourceWriteGuard.NormalizePath(FighterPortPaths.DataRoot);
            _storageRoot = SourceWriteGuard.NormalizePath(storageRoot ?? settingsRoot);
            SourceWriteGuard.EnsureNoReparsePoints(_storageRoot);
            if (SourceWriteGuard.IsWithin(_storageRoot, RootPath) || SourceWriteGuard.IsWithin(RootPath, _storageRoot) ||
                SourceWriteGuard.IsWithin(settingsRoot, RootPath) || SourceWriteGuard.IsWithin(RootPath, settingsRoot))
                throw new IOException("Choose a source folder outside FighterPort Studio's staging and settings folder.");
            SourceWriteGuard.Initialize(System.IO.Path.Combine(_storageRoot, "source-roots.txt"));
            SourceWriteGuard.RegisterRoot(RootPath);
        }

        private string CheckedSourcePath(string path)
        {
            string full = SourceWriteGuard.NormalizePath(path);
            if (!SourceWriteGuard.IsWithin(full, RootPath)) throw new IOException("The path is outside this source root.");
            SourceWriteGuard.EnsureNoReparsePoints(full);
            return full;
        }

        public IList<SourceEntry> EnumerateDirectory(string directory, CancellationToken cancellationToken)
        {
            string full = CheckedSourcePath(directory);
            var entries = new List<SourceEntry>();
            foreach (string path in Directory.EnumerateFileSystemEntries(full))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (entries.Count == MaxDirectoryEntries)
                {
                    entries.Add(new SourceEntry { Path = full, Description = "Directory limit reached (512 entries). Select a more specific folder." });
                    break;
                }
                var entry = new SourceEntry { Path = path };
                entries.Add(entry);
                try
                {
                    CheckedSourcePath(path);
                    FileAttributes attributes = File.GetAttributes(path);
                    entry.IsDirectory = (attributes & FileAttributes.Directory) != 0;
                    if (entry.IsDirectory) { entry.Description = "Folder (browse on demand)"; continue; }
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (stream.Length > MaxFileBytes) { entry.Description = "Not opened: exceeds the 64 MiB source limit"; continue; }
                        if (stream.Length < 16) { entry.Description = "Unsupported or truncated header"; continue; }
                        byte[] header = ReadAt(stream, 0, 16);
                        string signature = Encoding.ASCII.GetString(header, 0, 4);
                        entry.CanOpen = RawTags.Contains(signature) || signature == "bres" || signature == "ARC\0";
                        entry.Description = entry.CanOpen ? signature.Replace("\0", "") + " signature; open validates the raw copy" :
                            "Unsupported signature (compressed/image/other-game adapters pending)";
                    }
                }
                catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
                {
                    entry.CanOpen = false;
                    entry.IsDirectory = false;
                    entry.Description = "Unavailable: " + error.Message;
                }
            }
            return entries.OrderByDescending(e => e.IsDirectory).ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public StagedSource Stage(string sourcePath, CancellationToken cancellationToken)
        {
            string source = CheckedSourcePath(sourcePath);
            string staging = System.IO.Path.Combine(_storageRoot, "StagedSources");
            SourceWriteGuard.ThrowIfProtected(staging);
            Directory.CreateDirectory(staging);
            CheckStagingQuota(staging, cancellationToken);
            string folder = System.IO.Path.Combine(staging, Guid.NewGuid().ToString("N"));
            string output = System.IO.Path.Combine(folder, System.IO.Path.GetFileName(source));
            string temporary = output + ".partial";
            SourceWriteGuard.ThrowIfProtected(output);
            Directory.CreateDirectory(folder);
            try
            {
                string sourceHash;
                long length;
                using (FileStream input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read,
                    65536, FileOptions.SequentialScan))
                {
                    length = input.Length;
                    if (length < 16 || length > MaxFileBytes) throw new IOException("Only raw resources from 16 bytes to 64 MiB can be staged.");
                    ValidateResource(input, 0, length, 0, cancellationToken);
                    // Use the detected native extension so a misleading .txt name cannot enter the GCT/script UI.
                    string tag = Encoding.ASCII.GetString(ReadAt(input, 0, 4));
                    string extension = tag == "ARC\0" ? ".pac" : tag == "bres" ? ".brres" : "." + tag.ToLowerInvariant();
                    output = System.IO.Path.ChangeExtension(output, extension);
                    temporary = output + ".partial";
                    SourceWriteGuard.ThrowIfProtected(output);
                    input.Position = 0;
                    using (FileStream copy = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    using (SHA256 hash = SHA256.Create())
                    {
                        byte[] buffer = new byte[65536];
                        long copied = 0;
                        int read;
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            copied += read;
                            if (copied > MaxFileBytes || copied > length) throw new IOException("The source changed during staging.");
                            copy.Write(buffer, 0, read);
                            hash.TransformBlock(buffer, 0, read, buffer, 0);
                        }
                        hash.TransformFinalBlock(new byte[0], 0, 0);
                        sourceHash = Hex(hash.Hash);
                        if (copied != length) throw new IOException("The source changed during staging.");
                        copy.Flush(true);
                    }
                    input.Position = 0;
                    using (SHA256 hash = SHA256.Create())
                        if (Hex(hash.ComputeHash(input)) != sourceHash) throw new IOException("The source changed during staging.");
                }
                cancellationToken.ThrowIfCancellationRequested();
                using (FileStream staged = File.OpenRead(temporary))
                using (SHA256 hash = SHA256.Create())
                    if (Hex(hash.ComputeHash(staged)) != sourceHash) throw new IOException("The staged copy failed hash verification.");
                File.Move(temporary, output);
                File.WriteAllLines(System.IO.Path.Combine(folder, "source-receipt.txt"), new[]
                {
                    "FighterPort Studio source receipt v1", "Source=" + source, "SHA256=" + sourceHash,
                    "Bytes=" + length, "StagedUtc=" + DateTime.UtcNow.ToString("O"),
                    "Companions=Not assembled. Open an enclosing supported BRRES/ARC for embedded resources."
                }, Encoding.UTF8);
                return new StagedSource { SourcePath = source, StagedPath = output, Sha256 = sourceHash, Length = length };
            }
            catch
            {
                if (File.Exists(temporary)) File.Delete(temporary);
                // Never remove user edits or other sessions while cleaning a failed new staging operation.
                if (!Directory.EnumerateFileSystemEntries(folder).Any()) Directory.Delete(folder);
                throw;
            }
        }

        private static void CheckStagingQuota(string staging, CancellationToken token)
        {
            long size = 0;
            int count = 0;
            foreach (string directory in Directory.EnumerateDirectories(staging))
            {
                token.ThrowIfCancellationRequested();
                if (++count >= 1024) throw new IOException("The staged-session limit was reached. Archive your edits before removing old staged sessions.");
                SourceWriteGuard.EnsureNoReparsePoints(directory);
                int files = 0;
                foreach (string file in Directory.EnumerateFiles(directory))
                {
                    token.ThrowIfCancellationRequested();
                    SourceWriteGuard.EnsureNoReparsePoints(file);
                    if (++files > 16) throw new IOException("Unexpected staging contents; archive them before continuing.");
                    size += new FileInfo(file).Length;
                    if (size + MaxFileBytes > MaxStagingBytes) throw new IOException("The 1 GiB staging quota was reached. Archive your edits before removing old staged sessions.");
                }
            }
        }

        private static void ValidateResource(FileStream input, long offset, long length, int depth, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (depth > 16 || length < 16 || offset < 0 || offset > input.Length - length)
                throw new IOException("Invalid resource range or nested archive limit exceeded.");
            byte[] header = ReadAt(input, offset, 16);
            string signature = Encoding.ASCII.GetString(header, 0, 4);
            if (RawTags.Contains(signature))
            {
                long size = Be32(header, 4);
                int minimum = MinimumHeaderSize(signature, Be32(header, 8));
                if (size < minimum || size > length) throw new IOException("The resource header size is outside the file.");
                return; // Native field validation remains the inherited parser's responsibility.
            }
            if (signature == "bres")
            {
                if (header[4] != 0xfe || header[5] != 0xff || Be32(header, 8) != length ||
                    Be16(header, 12) < 16 || Be16(header, 12) > length - 24)
                    throw new IOException("Invalid raw BRRES header or byte order.");
                byte[] root = ReadAt(input, offset + Be16(header, 12), 8);
                if (Encoding.ASCII.GetString(root, 0, 4) != "root" || Be32(root, 4) < 32 || Be32(root, 4) > length - Be16(header, 12))
                    throw new IOException("Invalid BRRES root section.");
                int resources = 0;
                ValidateBresGroup(input, offset, length, offset + Be16(header, 12) + 8,
                    offset + Be16(header, 12) + Be32(root, 4), true, depth, ref resources, token);
                return;
            }
            if (signature == "ARC\0")
            {
                if (length < 64 || header[4] != 1 || header[5] != 1) throw new IOException("Invalid raw ARC header.");
                if (Array.IndexOf(ReadAt(input, offset + 16, 48), (byte)0) < 0)
                    throw new IOException("Unterminated ARC resource name.");
                int count = Be16(header, 6);
                if (count > 4096) throw new IOException("ARC entry limit exceeded.");
                long current = offset + 64;
                for (int i = 0; i < count; i++)
                {
                    if (current > offset + length - 32) throw new IOException("ARC entry header exceeds the archive.");
                    byte[] entry = ReadAt(input, current, 32);
                    long size = Be32(entry, 4);
                    if (size > offset + length - current - 32) throw new IOException("ARC entry exceeds the archive.");
                    if (size > 0) ValidateResource(input, current + 32, size, depth + 1, token);
                    current = (current + 32 + size + 31) & ~31L;
                }
                return;
            }
            throw new IOException("This source slice accepts only raw BRRES/ARC and MDL0/CHR0/TEX0/PLT0/SRT0/PAT0/CLR0/VIS0/SHP0/SCN0 resources. Compressed or opaque ARC children are not opened.");
        }

        private static int MinimumHeaderSize(string tag, long version)
        {
            // Header layouts inspected in the pinned BrawlLib SSBB/Types source. This is not full native-field validation.
            switch (tag)
            {
                case "MDL0": if (version == 8 || version == 9) return 0x80; if (version == 10) return 0x88; if (version == 11) return 0x8c; break;
                case "CHR0": if (version == 3 || version == 4) return 0x28; if (version == 5) return 0x2c; break;
                case "TEX0": if (version >= 1 && version <= 3) return 0x40; break;
                case "PLT0": if (version == 1) return 0x40; if (version == 3) return 0x44; break;
                case "SRT0": if (version == 4) return 0x28; if (version == 5) return 0x2c; break;
                case "PAT0": if (version == 3) return 0x38; if (version == 4) return 0x3c; break;
                case "CLR0": case "VIS0": if (version == 3) return 0x24; if (version == 4) return 0x28; break;
                case "SHP0": if (version == 3) return 0x28; if (version == 4) return 0x2c; break;
                case "SCN0": if (version == 4) return 0x44; if (version == 5) return 0x48; break;
            }
            throw new IOException("This source preflight does not support " + tag + " header version " + version + ".");
        }

        private static void ValidateBresGroup(FileStream input, long start, long length, long group,
            long rootEnd, bool folders, int depth, ref int resources, CancellationToken token)
        {
            if (group < start || group > rootEnd - 24) throw new IOException("BRRES dictionary is outside its root section.");
            byte[] header = ReadAt(input, group, 24);
            long size = Be32(header, 0), count = Be32(header, 4);
            if (count > 4096 || size < 24 + count * 16 || size > rootEnd - group)
                throw new IOException("Invalid or oversized BRRES dictionary.");
            for (int i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();
                byte[] entry = ReadAt(input, group + 24 + i * 16, 16);
                long name = group + Be32(entry, 8), data = group + Be32(entry, 12);
                if (name < start || name >= start + length || data < start || data > start + length - 16)
                    throw new IOException("BRRES dictionary entry points outside the resource.");
                input.Position = name;
                bool terminated = false;
                for (int c = 0; c < 1024 && input.Position < start + length; c++)
                    if (input.ReadByte() == 0) { terminated = true; break; }
                if (!terminated) throw new IOException("BRRES dictionary name is unterminated or exceeds 1024 bytes.");
                if (folders)
                    ValidateBresGroup(input, start, length, data, rootEnd, false, depth, ref resources, token);
                else
                {
                    if (++resources > 4096) throw new IOException("BRRES resource limit exceeded.");
                    byte[] resource = ReadAt(input, data, 16);
                    if (!RawTags.Contains(Encoding.ASCII.GetString(resource, 0, 4)))
                        throw new IOException("Only recognized uncompressed BRRES child resources can be opened from a source folder.");
                    ValidateResource(input, data, Be32(resource, 4), depth + 1, token);
                    if (Be32(resource, 4) > start + length - data)
                        throw new IOException("BRRES resource data exceeds its container.");
                }
            }
        }

        private static byte[] ReadAt(FileStream input, long offset, int count)
        {
            input.Position = offset;
            byte[] result = new byte[count];
            int read = 0;
            while (read < count)
            {
                int next = input.Read(result, read, count - read);
                if (next == 0) throw new IOException("Truncated source header.");
                read += next;
            }
            return result;
        }
        private static long Be32(byte[] data, int at) { return ((long)data[at] << 24) | ((long)data[at + 1] << 16) | ((long)data[at + 2] << 8) | data[at + 3]; }
        private static int Be16(byte[] data, int at) { return (data[at] << 8) | data[at + 1]; }
        private static string Hex(byte[] bytes) { return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant(); }
    }
}
