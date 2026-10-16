using System.Collections.Generic;

namespace FighterPort.Core
{
    // Versioned worker/project contract. Arrays use explicit documented ordering.
    public class CharacterProject
    {
        public int Version { get; set; } = 1;
        public string Name { get; set; } = "Untitled";
        public string TargetProfile { get; set; } = "Custom (no game profile)";
        public string Branch { get; set; } = "source";
        public List<Bone> Bones { get; set; } = new List<Bone>();
        public List<Mesh> Meshes { get; set; } = new List<Mesh>();
        public List<Clip> Clips { get; set; } = new List<Clip>();
        public List<NativeResource> Resources { get; set; } = new List<NativeResource>();
        public List<string> Limitations { get; set; } = new List<string>();
    }
    public class Bone
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int NativeIndex { get; set; }
        public int Parent { get; set; } = -1;
        public float[] Translation { get; set; } = new float[3];
        public float[] Rotation { get; set; } = new float[3];
        public float[] Scale { get; set; } = new float[] { 1, 1, 1 };
        public float[] InverseBind { get; set; } = new float[16];
    }
    public class Mesh
    {
        public string Name { get; set; } = "mesh";
        public string Region { get; set; } = "cloth";
        public float[] Color { get; set; } = new float[] { .2f, .65f, .8f };
        public List<Vertex> Vertices { get; set; } = new List<Vertex>();
        public List<int> Indices { get; set; } = new List<int>();
    }
    public class Vertex
    {
        public float[] Position { get; set; } = new float[3];
        public float[] Normal { get; set; } = new float[] { 0, 1, 0 };
        public float[] UV { get; set; } = new float[2];
        public int[] Bones { get; set; } = new int[0];
        public float[] Weights { get; set; } = new float[0];
    }
    public class Clip
    {
        public string Name { get; set; } = "clip";
        public string SkeletonFingerprint { get; set; } = "";
        public int NativeVersion { get; set; } = 4;
        public int FrameCount { get; set; } = 61;
        public float FramesPerSecond { get; set; } = 30;
        public bool Loop { get; set; }
        public List<Curve> Curves { get; set; } = new List<Curve>();
    }
    public class Curve
    {
        public string Bone { get; set; } = "";
        // 0..2 scale, 3..5 XYZ Euler degrees, 6..8 translation centimeters.
        public int Channel { get; set; }
        public string Interpolation { get; set; } = "Hermite";
        public List<Key> Keys { get; set; } = new List<Key>();
    }
    public class Key
    {
        public float Frame { get; set; }
        public float Value { get; set; }
        public float Tangent { get; set; }
        public bool Locked { get; set; }
    }
    public class NativeResource
    {
        public string Name { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public string Base64 { get; set; } = "";
    }
}
