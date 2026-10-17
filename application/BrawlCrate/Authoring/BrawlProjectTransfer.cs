// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace BrawlCrate.Authoring
{
    internal static partial class BrawlProjectAdapter
    {
        private static RigPose[] Sample(MDL0Node model,CHR0Node motion,float time)
        {
            model.ApplyCHR(motion,time+1);
            return model.BoneCache.OfType<MDL0BoneNode>().Select(b=> {
                var p=b.FrameState;
                if(Math.Abs(p._scale._x-1)+Math.Abs(p._scale._y-1)+Math.Abs(p._scale._z-1)>1e-5)throw new IOException("Animated scale is unsupported by FK v1.");
                return new RigPose {X=p._translate._x,Y=p._translate._y,Z=p._translate._z,RX=p._rotate._x,RY=p._rotate._y,RZ=p._rotate._z};
            }).ToArray();
        }
        internal static string Bake(string sourceFile,SourceCharacterProfile source,AnimationAsset sourceAnimation,string targetFile,SourceCharacterProfile target,AnimationAsset targetAnimation,ReviewedRigMap map,string output,CancellationToken token)
        {
            RigTransfer.Validate(map,source,target);
            using(var sourceRoot=NodeFactory.FromFile(null,sourceFile))
            using(var targetRoot=NodeFactory.FromFile(null,targetFile))
            {
                var sm=(MDL0Node)At(sourceRoot,source.ModelPath);sm.Populate();var tm=(MDL0Node)At(targetRoot,target.ModelPath);tm.Populate();
                var motion=Motion(sourceRoot,source,sourceAnimation); var old=Motion(targetRoot,target,targetAnimation);
                if(motion.FrameCount<2 || motion.FrameCount>2048 || (long)motion.FrameCount*target.Bones.Count*6>1000000)throw new IOException("FK bake budget: 2..2048 frames and at most one million scalar keys.");
                long vertices=tm._objList.OfType<MDL0ObjectNode>().Sum(m=>(long)m.Vertices.Count);
                if(vertices*((motion.Loop?motion.FrameCount+1:motion.FrameCount-1)*4L+1)>8000000)throw new IOException("FK validation budget: at most eight million decoded vertex samples. Select a smaller clip/model.");
                foreach(CHR0EntryNode e in motion.Children) if(e.ScaleCompensateApply || e.ScaleCompensateParent || e.ClassicScaleOff)throw new IOException("Native motion scale compensation requires a different transfer route.");
                var values=new RigPose[motion.FrameCount][]; RigPose[] previous=null,priorSource=null;
                for(int f=0;f<motion.FrameCount;f++)
                {
                    token.ThrowIfCancellationRequested();var sampled=Sample(sm,motion,f);
                    if(priorSource!=null && sampled.Zip(priorSource,(a,b)=>Math.Abs(a.RX-b.RX)+Math.Abs(a.RY-b.RY)+Math.Abs(a.RZ-b.RZ)).Sum()>=179)
                        throw new IOException("FK v1 rejects ambiguous fast winding (combined native Euler travel >=179 degrees per baked frame); use a denser supported time representation.");
                    values[f]=RigTransfer.Transfer(map,source,target,sampled,previous);previous=values[f];priorSource=sampled;
                }
                var baked=new CHR0Node {Name=old.Name,Version=4,FrameCount=motion.FrameCount,Loop=motion.Loop};
                for(int bone=0;bone<target.Bones.Count;bone++)
                {
                    var entry=baked.CreateEntry(target.Bones[bone].Name);
                    for(int channel=3;channel<9;channel++)
                    {
                        Func<int,double> value=f=> {var p=values[f][bone];return channel==3?p.RX:channel==4?p.RY:channel==5?p.RZ:channel==6?p.X:channel==7?p.Y:p.Z;};
                        double slope=(value(values.Length-1)-value(0))/(values.Length-1);
                        bool linear=Enumerable.Range(0,values.Length).All(f=>Math.Abs(value(f)-(value(0)+slope*f))<1e-5);
                        var indices=linear?new[]{0,values.Length-1}:Enumerable.Range(0,values.Length).ToArray();
                        // Keep exact linear curves sparse. Inherited dense rotation encodings
                        // may reconstruct different fractional tangents; fresh decode below
                        // rejects any remaining approximation outside declared tolerances.
                        foreach(int f in indices)
                        {
                            var key=entry.SetKeyframe(channel,f,(float)value(f),true);
                            int left=Math.Max(0,f-1),right=Math.Min(values.Length-1,f+1);
                            key._tangent=(float)((value(right)-value(left))/(right-left));
                        }
                    }
                }
                var parent=old.Parent; int index=old.Index;
                // Keep the native group alive while replacing its sole motion.
                parent.InsertChild(baked,index);old.Remove();old.Dispose();token.ThrowIfCancellationRequested();targetRoot.Export(output);
            }
            ValidateEdit(targetFile,output,target,targetAnimation,Path.GetDirectoryName(output));
            double maxPosition=0,maxDegrees=0,maxMesh=0;
            using(var sourceRoot=NodeFactory.FromFile(null,sourceFile))
            using(var decoded=NodeFactory.FromFile(null,output))
            {
                var sm=(MDL0Node)At(sourceRoot,source.ModelPath);sm.Populate();var tm=(MDL0Node)At(decoded,target.ModelPath);tm.Populate();
                var motion=Motion(sourceRoot,source,sourceAnimation);var baked=Motion(decoded,target,targetAnimation);
                if(baked.FrameCount!=motion.FrameCount || baked.Loop!=motion.Loop)throw new IOException("Native bake changed timing/loop.");
                int end=motion.Loop?motion.FrameCount+1:motion.FrameCount-1;
                for(int q=0;q<=end*4;q++)
                {
                    token.ThrowIfCancellationRequested();float time=q/4f;if(motion.Loop && time==end)time=0;
                    var expected=RigTransfer.World(target.Bones,RigTransfer.Transfer(map,source,target,Sample(sm,motion,time)));
                    var actual=RigTransfer.World(target.Bones,Sample(tm,baked,time));
                    for(int i=0;i<expected.Length;i++)
                    {
                        maxPosition=Math.Max(maxPosition,Math.Sqrt(Math.Pow(actual[i].X-expected[i].X,2)+Math.Pow(actual[i].Y-expected[i].Y,2)+Math.Pow(actual[i].Z-expected[i].Z,2)));
                        maxDegrees=Math.Max(maxDegrees,2*Math.Asin(Math.Min(1,Math.Sqrt(actual[i].RotationError(expected[i])/8)))*180/Math.PI);
                    }
                    foreach(var mesh in tm._objList.OfType<MDL0ObjectNode>()) foreach(var vertex in mesh.Vertices)
                    {
                        double x=0,y=0,z=0;
                        var influence=vertex.MatrixNode??mesh.MatrixNode;
                        if(influence==null)throw new IOException("Unbound native mesh has no reviewed skinning route.");
                        foreach(var weight in influence.Weights)
                        {
                            int i=target.Bones.FindIndex(b=>b.Name==weight.Bone.Name); var m=weight.Bone.InverseBindMatrix;
                            var inv=new RigidTransform {X=m[12],Y=m[13],Z=m[14],R=new double[]{m[0],m[4],m[8],m[1],m[5],m[9],m[2],m[6],m[10]}};
                            var skin=RigidTransform.Multiply(expected[i],inv);var v=vertex.Position;
                            x+=weight.Weight*(skin.X+skin.R[0]*v._x+skin.R[1]*v._y+skin.R[2]*v._z);y+=weight.Weight*(skin.Y+skin.R[3]*v._x+skin.R[4]*v._y+skin.R[5]*v._z);z+=weight.Weight*(skin.Z+skin.R[6]*v._x+skin.R[7]*v._y+skin.R[8]*v._z);
                        }
                        var a=vertex.WeightedPosition;maxMesh=Math.Max(maxMesh,Math.Sqrt((a._x-x)*(a._x-x)+(a._y-y)*(a._y-y)+(a._z-z)*(a._z-z)));
                    }
                }
            }
            if(maxPosition>.002 || maxDegrees>.05 || maxMesh>.005)throw new IOException("Reopened FK bake exceeds measured limits: position="+maxPosition+", degrees="+maxDegrees+", mesh="+maxMesh+". No instance was assigned.");
            return string.Format(CultureInfo.InvariantCulture,"world-rest-delta-fk-v1; quarter-frame decode including native loop boundary: position={0:R} native units, orientation={1:R} degrees, mesh={2:R}; limits .002/.05/.005. Integer native bake; no IK, mesh rebinding or ancillary transfer; loop/timing retained.",maxPosition,maxDegrees,maxMesh);
        }
    }
}
