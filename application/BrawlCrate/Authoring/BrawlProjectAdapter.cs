// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Sources;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BrawlCrate.Authoring
{
    // This native adapter does not define an intermediate for other games.
    internal static partial class BrawlProjectAdapter
    {
        internal const string Interpretation = "brawl-native-v1; native XYZ local basis; Euler degrees; native units (physical scale unknown); no semantic roles approved";
        internal static ResourceNode At(ResourceNode root, string path)
        {
            foreach (string part in path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(part, out int i) || i < 0 || i >= root.Children.Count) throw new IOException("Native indexed path changed.");
                root = root.Children[i];
            }
            return root;
        }
        internal static string PathOf(ResourceNode root, ResourceNode node)
        {
            var parts = new Stack<int>();
            while (node != root)
            {
                if (node.Parent == null) throw new IOException("Native resource is outside this character.");
                parts.Push(node.Index); node = node.Parent;
            }
            return string.Join("/", parts);
        }
        internal static CHR0Node Motion(ResourceNode root, SourceCharacterProfile profile, AnimationAsset animation)
        {
            var motion = At(root, animation.NativePath) as CHR0Node;
            if (motion == null || motion.Name != animation.Name || motion.Version != 4) throw new IOException("Bound native motion identity changed.");
            var model = At(root, profile.ModelPath) as MDL0Node ?? throw new IOException("Bound native model missing.");
            var names = new HashSet<string>(model.BoneCache.Select(b => b.Name), StringComparer.Ordinal);
            if (motion.FrameCount < 1 || motion.FrameCount > 100000 || motion.Children.Count > 512 ||
                motion.Children.Select(c => c.Name).Distinct(StringComparer.Ordinal).Count() != motion.Children.Count)
                throw new IOException("Motion timing/track budget or unique identity violated.");
            int keys = 0;
            foreach (CHR0EntryNode entry in motion.Children)
            {
                if (!names.Contains(entry.Name)) throw new IOException("Motion contains an unbound bone; explicitly bind it to a supported source rig before transfer.");
                foreach (var array in entry.KeyArrays)
                    foreach (var k in array.Entries)
                        if (++keys > 1000000 || k._index < 0 || k._index > motion.FrameCount || float.IsNaN(k._value) || float.IsInfinity(k._value) || float.IsNaN(k._tangent) || float.IsInfinity(k._tangent))
                            throw new IOException("Invalid or excessive native motion keys.");
            }
            return motion;
        }
        internal static List<BoneSnapshot> Skeleton(MDL0Node model)
        {
            model.Populate(); var bones = model.BoneCache.OfType<MDL0BoneNode>().ToList();
            if (bones.Count == 0 || bones.Count > 512 || bones.Select(b => b.Name).Distinct(StringComparer.Ordinal).Count() != bones.Count)
                throw new IOException("This project adapter requires 1..512 uniquely named native bones.");
            return bones.Select(b => new BoneSnapshot {
                Name = b.Name, Parent = b.Parent is MDL0BoneNode parent ? bones.IndexOf(parent) : -1,
                Rest = Floats(new[] { b.BindState._scale._x, b.BindState._scale._y, b.BindState._scale._z,
                    b.BindState._rotate._x, b.BindState._rotate._y, b.BindState._rotate._z,
                    b.BindState._translate._x, b.BindState._translate._y, b.BindState._translate._z }),
                Bind = MatrixText(b.BindMatrix), InverseBind = MatrixText(b.InverseBindMatrix),
                Flags = (uint)b._boneFlags, Billboard = (uint)b._billboardFlags }).ToList();
        }
        private static string MatrixText(Matrix matrix) => Floats(Enumerable.Range(0, 16).Select(i => matrix[i]));
        private static string Floats(IEnumerable<float> values)
        {
            var data = values.ToArray();
            if (data.Any(f => float.IsNaN(f) || float.IsInfinity(f))) throw new IOException("Nonfinite rig values are unsupported.");
            return string.Join(",", data.Select(f => f.ToString("R", CultureInfo.InvariantCulture)));
        }
        internal static string Fingerprint(List<BoneSnapshot> bones) => CharacterProject.Hash(CharacterProject.Serialize(bones));
        internal static string ExportHash(ResourceNode node, string scratch)
        {
            string file = System.IO.Path.Combine(scratch, Guid.NewGuid().ToString("N") + "." + (node is MDL0Node ? "mdl0" : node is CHR0Node ? "chr0" : "native"));
            try { node.Export(file); return CharacterProject.Hash(CharacterProject.ReadBounded(file)); }
            finally { if (File.Exists(file)) File.Delete(file); }
        }
        internal static string PayloadHash(string file, string nativePath)
        {
            var record = BrawlContainerAudit.Inspect(file).Records.Single(r => r.Path == nativePath);
            using (var stream = File.OpenRead(file))
            {
                stream.Position = record.Offset;
                var data = new byte[checked((int)record.Length)];
                int read = 0, count;
                while (read < data.Length && (count = stream.Read(data, read, data.Length - read)) != 0) read += count;
                if (read != data.Length) throw new IOException("Source changed while reading native evidence.");
                return CharacterProject.Hash(data);
            }
        }
        internal static void ValidateArchive(string path, SourceCharacterProfile profile, AnimationAsset animation, string scratch, bool original)
        {
            BrawlContainerAudit.Inspect(path).RequirePreservable();
            using (var root = NodeFactory.FromFile(null, path))
            {
                if ((root is ARCNode ? ".pac" : root is BRRESNode ? ".brres" : "") != profile.Extension) throw new IOException("Native root type changed.");
                var model = At(root, profile.ModelPath) as MDL0Node;
                if (model == null || model.Version != 11 || Fingerprint(Skeleton(model)) != profile.RigFingerprint || ExportHash(model, scratch) != profile.ModelExportHash)
                    throw new IOException("Model/rig identity changed; saved assumptions are invalid. Register a new source revision.");
                var motion = Motion(root, profile, animation);
                if (original && (ExportHash(motion, scratch) != animation.NativeExportHash || motion.FrameCount != animation.Frames || motion.Loop != animation.Loop))
                    throw new IOException("Immutable library animation changed.");
            }
        }
        internal static void ValidateEdit(string baseline, string edited, SourceCharacterProfile profile, AnimationAsset animation, string scratch, bool importingClip=false)
        {
            ValidateArchive(edited, profile, animation, scratch, false);
            BrawlContainerAudit.VerifyPreserved(baseline, edited, profile.ContainerPath);
            using (var before = NodeFactory.FromFile(null, baseline))
            using (var after = NodeFactory.FromFile(null, edited))
            {
                // Capture supports selected skeletal motion edits only. Preserve all other native entries.
                var a = ((BRRESNode)At(before, profile.ContainerPath)).Children.SelectMany(g => g.Children).ToList();
                var b = ((BRRESNode)At(after, profile.ContainerPath)).Children.SelectMany(g => g.Children).ToList();
                if (a.Count != b.Count) throw new IOException("Adding/removing native resources is not supported by this project increment.");
                for (int i = 0; i < a.Count; i++)
                    if (a[i].GetType() != b[i].GetType() || (a[i].Name != b[i].Name && !(importingClip && PathOf(before,a[i])==animation.NativePath)) ||
                        (PathOf(before, a[i]) != animation.NativePath && ExportHash(a[i], scratch) != ExportHash(b[i], scratch)))
                        throw new IOException("Only the bound CHR0 may change in this project increment. Save other native edits separately.");
            }
        }
    }
}
