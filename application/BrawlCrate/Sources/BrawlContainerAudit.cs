// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
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
    public sealed class BrawlNativeRecord
    {
        public string Path { get; internal set; }
        public string Tag { get; internal set; }
        public long Version { get; internal set; }
        public long Offset { get; internal set; }
        public long Length { get; internal set; }
        public string ArcEntry { get; internal set; }
        public string ArchiveName { get; internal set; }
        public int Children { get; internal set; }
    }

    // Read-only bounded header inventory; this does not claim native evaluation/edit support.
    public sealed class BrawlContainerAudit
    {
        public readonly List<BrawlNativeRecord> Records = new List<BrawlNativeRecord>();
        public readonly List<string> Issues = new List<string>();
        private static readonly HashSet<string> Tags = new HashSet<string> { "MDL0", "TEX0", "PLT0", "CHR0", "SRT0", "PAT0", "CLR0", "VIS0", "SHP0", "SCN0" };
        public static BrawlContainerAudit Inspect(string path, CancellationToken token = default(CancellationToken))
        {
            path = BrawlDumpProfile.LocalAbsolute(path); SourceWriteGuard.EnsureNoReparsePoints(path);
            var audit = new BrawlContainerAudit();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length < 16 || stream.Length > SourceFolderSession.MaxFileBytes) throw new IOException("Native inventory requires 16 bytes to 64 MiB.");
                audit.Read(stream, 0, stream.Length, "", "root", 0, token);
            }
            return audit;
        }
        public void RequirePreservable()
        { if (Issues.Count != 0) throw new IOException("Container cannot be assembled without loss: " + string.Join("; ", Issues.Take(8))); }

        private void Read(FileStream input, long offset, long length, string path, string arcEntry, int depth, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (depth > 16 || Records.Count >= 4096 || length < 16 || offset < 0 || offset > input.Length - length)
                throw new IOException("Native inventory range/depth/resource budget exceeded at [" + path + "].");
            byte[] header = At(input, offset, 16); string tag = Encoding.ASCII.GetString(header, 0, 4);
            var record = new BrawlNativeRecord { Path = path, Offset = offset, Length = length, Tag = tag.Replace("\0", ""), ArcEntry = arcEntry };
            Records.Add(record);
            if (Tags.Contains(tag))
            {
                record.Version = Be32(header, 8);
                if (Be32(header, 4) < 16 || Be32(header, 4) > length) throw new IOException("Native child size is invalid at [" + path + "].");
                return;
            }
            if (tag == "ARC\0")
            {
                if (length < 64 || header[4] != 1 || header[5] != 1) throw new IOException("Unsupported ARC header at [" + path + "].");
                record.Version = 0x0101; record.Children = Be16(header, 6);
                if (record.Children > 4096) throw new IOException("ARC entry budget exceeded.");
                if (header.Skip(8).Any(b => b != 0)) Issue("Nonzero ARC reserved header at [" + path + "] would be discarded by the native writer");
                byte[] name = At(input, offset + 16, 48); int end = Array.IndexOf(name, (byte)0);
                if (end < 0) throw new IOException("Unterminated ARC name.");
                record.ArchiveName = Encoding.ASCII.GetString(name, 0, end);
                if (name.Any(b => b > 127) || name.Skip(end).Any(b => b != 0)) Issue("Noncanonical ARC name bytes at [" + path + "]");
                long current = offset + 64;
                for (int i = 0; i < record.Children; i++)
                {
                    token.ThrowIfCancellationRequested();
                    if (current > offset + length - 32) throw new IOException("ARC entry header exceeds container.");
                    byte[] entry = At(input, current, 32); long size = Be32(entry, 4);
                    if (size < 16 || size > offset + length - current - 32) throw new IOException("Empty/redirect/invalid ARC entries require a separate preserving path.");
                    if (entry[9] != 0 || entry.Skip(12).Any(b => b != 0)) Issue("Nonzero ARC entry reserved bytes at [" + Child(path, i) + "] would be discarded");
                    if (Be16(entry, 10) != 65535) Issue("ARC redirect at [" + Child(path, i) + "] is not a supported companion entry");
                    string identity = "type=" + Be16(entry, 0) + ",index=" + Be16(entry, 2) + ",group=" + entry[8] + ",redirect=" + Be16(entry, 10);
                    Read(input, current + 32, size, Child(path, i), identity, depth + 1, token);
                    long aligned = (current + 32 + size + 31) & ~31L;
                    if (aligned > offset + length) throw new IOException("ARC alignment exceeds container.");
                    if (aligned > current + 32 + size && At(input, current + 32 + size, (int)(aligned - current - 32 - size)).Any(b => b != 0))
                        Issue("Nonzero ARC alignment bytes at [" + Child(path, i) + "] would be discarded");
                    current = aligned;
                }
                if (current != offset + length) Issue("Trailing ARC bytes at [" + path + "] are not representable by the native writer");
                return;
            }
            if (tag == "bres")
            {
                if (header[4] != 254 || header[5] != 255 || Be32(header, 8) != length) throw new IOException("Invalid BRRES range/byte order.");
                record.Version = Be16(header, 6);
                long root = offset + Be16(header, 12);
                if (root < offset + 16 || root > offset + length - 32) throw new IOException("BRRES root outside container.");
                byte[] rootHeader = At(input, root, 8);
                if (Encoding.ASCII.GetString(rootHeader, 0, 4) != "root" || Be32(rootHeader, 4) < 32 || Be32(rootHeader, 4) > offset + length - root)
                    throw new IOException("Invalid BRRES root section.");
                ReadGroup(input, offset, length, root + 8, root + Be32(rootHeader, 4), path, true, depth, token);
                return;
            }
            Issue("Unsupported/compressed/opaque native entry '" + record.Tag + "' at [" + path + "]; no native decoder invoked");
        }
        private void ReadGroup(FileStream input, long start, long length, long group, long rootEnd, string path, bool folders, int depth, CancellationToken token)
        {
            if (group < start || group > rootEnd - 24) throw new IOException("BRRES group exceeds root section.");
            byte[] header = At(input, group, 24); long count = Be32(header, 4), size = Be32(header, 0);
            if (count > 4096 || size < 24 + count * 16 || size > rootEnd - group) throw new IOException("Invalid BRRES dictionary budget/range.");
            for (int i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested(); byte[] entry = At(input, group + 24 + i * 16, 16);
                long data = group + Be32(entry, 12);
                if (data < start || data > start + length - 16) throw new IOException("BRRES entry outside container.");
                if (folders) ReadGroup(input, start, length, data, rootEnd, Child(path, i), false, depth, token);
                else
                {
                    byte[] resource = At(input, data, 16); long bytes = Be32(resource, 4);
                    if (bytes > start + length - data) throw new IOException("BRRES child exceeds enclosing container.");
                    Read(input, data, bytes, Child(path, i), "BRRES entry", depth + 1, token);
                }
            }
        }
        private void Issue(string message) { if (Issues.Count >= 128) throw new IOException("Native inventory diagnostic budget exceeded."); Issues.Add(message); }
        private static string Child(string path, int index) => path.Length == 0 ? index.ToString() : path + "/" + index;
        private static int Be16(byte[] data, int at) => (data[at] << 8) | data[at + 1];
        private static long Be32(byte[] data, int at) => ((long)data[at] << 24) | ((long)data[at + 1] << 16) | ((long)data[at + 2] << 8) | data[at + 3];
        private static byte[] At(FileStream stream, long offset, int count)
        {
            if (offset < 0 || count < 0 || offset > stream.Length - count) throw new IOException("Native inventory read exceeds file.");
            stream.Position = offset; var bytes = new byte[count]; int read = 0;
            while (read < count) { int n = stream.Read(bytes, read, count - read); if (n == 0) throw new IOException("Truncated native header."); read += n; }
            return bytes;
        }
        public static void VerifyPreserved(string original, string output, string selectedBrres, CancellationToken token = default(CancellationToken))
        {
            var before = Inspect(original, token); before.RequirePreservable(); var after = Inspect(output, token); after.RequirePreservable();
            var selected = before.Records.SingleOrDefault(r => r.Path == selectedBrres && r.Tag == "bres");
            var rebuilt = after.Records.SingleOrDefault(r => r.Path == selectedBrres && r.Tag == "bres");
            if (selected == null || rebuilt == null || selected.ArcEntry != rebuilt.ArcEntry || selected.Version != rebuilt.Version)
                throw new IOException("Selected BRRES container identity/version changed.");
            foreach (var entry in before.Records)
            {
                token.ThrowIfCancellationRequested();
                if (selectedBrres.Length == 0 || entry.Path.StartsWith(selectedBrres + "/", StringComparison.Ordinal)) continue;
                var actual = after.Records.SingleOrDefault(r => r.Path == entry.Path);
                if (actual == null || actual.Tag != entry.Tag || actual.ArcEntry != entry.ArcEntry || actual.Version != entry.Version ||
                    actual.Children != entry.Children || actual.ArchiveName != entry.ArchiveName)
                    throw new IOException("Native container hierarchy/entry identity changed at [" + entry.Path + "].");
                if (entry.Path != selectedBrres && entry.Tag != "ARC" && HashRange(original, entry.Offset, entry.Length, token) != HashRange(output, actual.Offset, actual.Length, token))
                    throw new IOException("Unrelated native payload changed at [" + entry.Path + "].");
            }
        }
        private static string HashRange(string path, long offset, long length, CancellationToken token)
        {
            using (var stream = File.OpenRead(path)) using (var hash = SHA256.Create())
            {
                stream.Position = offset; var buffer = new byte[65536];
                while (length > 0) { token.ThrowIfCancellationRequested(); int n = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, length)); if (n == 0) throw new IOException("Truncated native payload."); hash.TransformBlock(buffer, 0, n, buffer, 0); length -= n; }
                hash.TransformFinalBlock(new byte[0], 0, 0); return BitConverter.ToString(hash.Hash).Replace("-", "");
            }
        }
    }
}
