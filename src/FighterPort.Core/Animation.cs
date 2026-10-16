using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace FighterPort.Core;

public static class Animation
{
    public static Vector3 V(float[] a) => new(a[0], a[1], a[2]);
    public static float[] A(Vector3 v) => [v.X, v.Y, v.Z];
    public static Matrix4x4 M(float[] a) => new(a[0],a[1],a[2],a[3],a[4],a[5],a[6],a[7],a[8],a[9],a[10],a[11],a[12],a[13],a[14],a[15]);
    public static float[] A(Matrix4x4 m) => [m.M11,m.M12,m.M13,m.M14,m.M21,m.M22,m.M23,m.M24,m.M31,m.M32,m.M33,m.M34,m.M41,m.M42,m.M43,m.M44];
    public static Matrix4x4 Local(float[] s, float[] r, float[] t) =>
        Matrix4x4.CreateScale(V(s)) * Matrix4x4.CreateRotationX(r[0]*MathF.PI/180) *
        Matrix4x4.CreateRotationY(r[1]*MathF.PI/180) * Matrix4x4.CreateRotationZ(r[2]*MathF.PI/180) * Matrix4x4.CreateTranslation(V(t));

    public static float Evaluate(Curve c, float frame)
    {
        if (c.Keys.Count == 0) throw new InvalidDataException("Empty curve");
        if (frame <= c.Keys[0].Frame) return c.Keys[0].Value;
        for (int i = 1; i < c.Keys.Count; i++)
        {
            var b = c.Keys[i]; var a = c.Keys[i-1];
            if (frame > b.Frame) continue;
            if (frame == b.Frame) return b.Value;
            if (c.Interpolation == "Step") return a.Value;
            float d = b.Frame-a.Frame, u = (frame-a.Frame)/d;
            if (c.Interpolation == "Linear") return a.Value+(b.Value-a.Value)*u;
            float u2=u*u,u3=u2*u;
            return (2*u3-3*u2+1)*a.Value+(u3-2*u2+u)*d*a.Tangent+(-2*u3+3*u2)*b.Value+(u3-u2)*d*b.Tangent;
        }
        return c.Keys[^1].Value;
    }
    public static float Wrap(Clip clip, float frame) => clip.Loop ? ((frame % clip.FrameCount)+clip.FrameCount)%clip.FrameCount : Math.Clamp(frame,0,clip.FrameCount-1);
    public static Matrix4x4[] World(CharacterProject project, Clip? clip, float frame)
    {
        if (clip != null) frame = Wrap(clip,frame);
        var result = new Matrix4x4[project.Bones.Count];
        for (int i=0;i<result.Length;i++)
        {
            var b=project.Bones[i]; float[] values=[..b.Scale,..b.Rotation,..b.Translation];
            if (clip != null) foreach (var c in clip.Curves.Where(c=>c.Bone==b.Name)) values[c.Channel]=Evaluate(c,frame);
            var local=Local(values[..3],values[3..6],values[6..9]);
            result[i]=b.Parent<0?local:local*result[b.Parent];
        }
        return result;
    }
    public static void SetInverseBinds(CharacterProject p)
    {
        var w=World(p,null,0);
        for(int i=0;i<w.Length;i++)
        {
            if(!Matrix4x4.Invert(w[i],out var inverse)) throw new InvalidDataException("Singular bind");
            p.Bones[i].InverseBind=A(inverse);
        }
    }
    public static (Vector3 Position,Vector3 Normal) Skin(Vertex v, CharacterProject p, Matrix4x4[] world)
    {
        Vector3 position=default, normal=default;
        for(int i=0;i<v.Bones.Length;i++)
        {
            int b=v.Bones[i]; var m=M(p.Bones[b].InverseBind)*world[b];
            position+=Vector3.Transform(V(v.Position),m)*v.Weights[i];
            if(!Matrix4x4.Invert(m,out var inverse)) throw new InvalidDataException("Singular animated transform");
            normal+=Vector3.TransformNormal(V(v.Normal),Matrix4x4.Transpose(inverse))*v.Weights[i];
        }
        return (position,normal.LengthSquared()>1e-12f?Vector3.Normalize(normal):Vector3.UnitY);
    }
    public static string Fingerprint(CharacterProject p) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
        string.Join("|",p.Bones.Select(b=>$"{b.Id}:{b.Name}:{b.Parent}:{string.Join(',',b.InverseBind.Select(f=>f.ToString("R",System.Globalization.CultureInfo.InvariantCulture)))}")))));
}
