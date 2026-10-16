using System.Numerics;

namespace FighterPort.Core;

public sealed record BakeResult(CharacterProject Project,int ClipIndex,float MaxAngleDegrees,float MaxJointDistance,float MaxVertexDistance);

/// <summary>Explicit measured integer-sample bake. Fixed-grid evidence is not continuous correctness.</summary>
public static class NativeBake
{
    public static BakeResult Bake(CharacterProject original,int clipIndex,bool quaternion,float angleTolerance=.5f,float distanceTolerance=.05f)
    {
        ProjectStore.Validate(original);var source=original.Clips[clipIndex];
        if(quaternion&&source.Loop)throw new InvalidDataException("NUANMB subset cannot encode the source loop flag; choose a nonloop clip or retain the source without export");
        if(source.FrameCount>2000||(long)source.FrameCount*original.Bones.Count>100000)throw new InvalidDataException("Explicit bake sample quota");
        if(!float.IsFinite(angleTolerance)||!float.IsFinite(distanceTolerance)||angleTolerance<=0||distanceTolerance<=0)throw new InvalidDataException("Invalid bake tolerance");
        int count=source.FrameCount+(!quaternion&&source.Loop?1:0);
        var clip=new Clip{Name=source.Name+(quaternion?" • quaternion bake":" • CHR0 bake"),FrameCount=source.FrameCount,FramesPerSecond=source.FramesPerSecond,Loop=source.Loop,NativeVersion=quaternion?20:4,SkeletonFingerprint=Animation.Fingerprint(original),Losses=ProjectStore.Clone(source.Losses)};
        var tracks=original.Bones.Select(b=>new TransformTrack{Bone=b.Name}).ToArray();
        for(int f=0;f<count;f++)
        {
            var world=Animation.World(original,source,f);
            for(int i=0;i<world.Length;i++)
            {
                var local=world[i];if(original.Bones[i].Parent>=0){Matrix4x4.Invert(world[original.Bones[i].Parent],out var inverse);local*=inverse;}
                if(!Matrix4x4.Decompose(local,out var scale,out var rotation,out var translation)||scale.X<=0||scale.Y<=0||scale.Z<=0)throw new InvalidDataException("Bake encountered singular/reflected transform");
                var rebuilt=Matrix4x4.CreateScale(scale)*Matrix4x4.CreateFromQuaternion(rotation)*Matrix4x4.CreateTranslation(translation);
                if(Animation.A(rebuilt).Zip(Animation.A(local)).Any(x=>Math.Abs(x.First-x.Second)>.0001f))throw new InvalidDataException("Bake would discard shear");
                tracks[i].Samples.Add(new(){Scale=Animation.A(scale),Rotation=[rotation.X,rotation.Y,rotation.Z,rotation.W],Translation=Animation.A(translation)});
            }
        }
        if(quaternion)clip.TransformTracks=tracks.ToList();
        else foreach(var track in tracks)
        {
            float[] previous=[0,0,0];var curves=Enumerable.Range(0,9).Select(c=>new Curve{Bone=track.Bone,Channel=c}).ToArray();
            for(int f=0;f<count;f++)
            {
                var sample=track.Samples[f];var q=sample.Rotation;var m=Matrix4x4.CreateFromQuaternion(new(q[0],q[1],q[2],q[3]));
                float y=MathF.Asin(Math.Clamp(-m.M13,-1,1));
                if(Math.Abs(MathF.Cos(y))<.0001f)throw new InvalidDataException("Euler bake at gimbal singularity requires a tested alternate branch");
                float[] euler=[MathF.Atan2(m.M23,m.M33)*180/MathF.PI,y*180/MathF.PI,MathF.Atan2(m.M12,m.M11)*180/MathF.PI];
                if(f>0)for(int c=0;c<3;c++)euler[c]+=360*MathF.Round((previous[c]-euler[c])/360);
                previous=euler;float[] values=[..sample.Scale,..euler,..sample.Translation];
                for(int c=0;c<9;c++)curves[c].Keys.Add(new(){Frame=f,Value=values[c],Locked=true});
            }
            foreach(var curve in curves)
            {
                for(int i=0;i<count;i++){var a=curve.Keys[Math.Max(0,i-1)];var b=curve.Keys[Math.Min(count-1,i+1)];curve.Keys[i].Tangent=b.Frame>a.Frame?(b.Value-a.Value)/(b.Frame-a.Frame):0;}
                clip.Curves.Add(curve);
            }
        }
        var project=ProjectStore.Clone(original);project.Clips.Add(clip);project.Branch="target/measured-bake";ProjectStore.Validate(project);
        var vertices=original.Meshes.SelectMany(m=>m.Vertices).ToArray();int stride=Math.Max(1,vertices.Length/64);
        float maxAngle=0,maxJoint=0,maxVertex=0;
        for(float frame=0;frame<source.FrameCount;frame+=.25f)
        {
            var reference=Animation.World(original,source,frame);var baked=Animation.World(project,clip,frame);
            for(int i=0;i<reference.Length;i++)
            {
                Matrix4x4.Decompose(reference[i],out _,out var a,out _);Matrix4x4.Decompose(baked[i],out _,out var b,out _);
                maxAngle=Math.Max(maxAngle,2*MathF.Acos(Math.Clamp(Math.Abs(Quaternion.Dot(Quaternion.Normalize(a),Quaternion.Normalize(b))),0,1))*180/MathF.PI);
                maxJoint=Math.Max(maxJoint,Vector3.Distance(reference[i].Translation,baked[i].Translation));
            }
            for(int i=0;i<vertices.Length;i+=stride)maxVertex=Math.Max(maxVertex,Vector3.Distance(Animation.Skin(vertices[i],original,reference).Position,Animation.Skin(vertices[i],project,baked).Position));
            if(!float.IsFinite(maxAngle)||!float.IsFinite(maxJoint)||!float.IsFinite(maxVertex)||maxAngle>angleTolerance||maxJoint>distanceTolerance||maxVertex>distanceTolerance)throw new InvalidDataException($"Bake exceeds explicit tolerances: angle={maxAngle}, joint={maxJoint}, vertex={maxVertex}");
        }
        project.Limitations.Add($"Measured {(quaternion?"quaternion":"Euler Hermite")} integer bake: quarter-frame max angle {maxAngle:R} deg, joint {maxJoint:R} cm, sampled vertex {maxVertex:R} cm. Source retained. Not a continuous-error guarantee; no hidden tolerance relaxation.");
        return new(project,project.Clips.Count-1,maxAngle,maxJoint,maxVertex);
    }
}
