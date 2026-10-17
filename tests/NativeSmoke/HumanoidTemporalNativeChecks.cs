// Original authored continuous-motion/contact checks, FighterPort 2026-10-17.
// Repository LGPL-3.0 terms. No commercial anatomy, paths or native data.
using BrawlCrate.Authoring;
using BrawlCrate.Authoring.Humanoid;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

internal static partial class NativeSmoke
{
    private static void CheckHumanoidTemporalNative(CharacterProject parent,string tallClip,string stoutClip)
    {
        CheckHumanoidInteriorTangentFit();
        byte[] parentState=WorkspaceStateXml(parent);
        var pins=new[]{tallClip,stoutClip}.Select(id=>parent.SharedLibrary.Pin("animations",id)).ToArray();
        var payloads=pins.Select(pin=>parent.SharedLibrary.Read(pin).Animation.NativeHash).Distinct().ToArray();
        foreach(string hash in payloads)parent.SharedLibrary.ReadBlob(hash);
        var project=new CharacterProject(Path.Combine(_output,"storage"));
        project.UseSharedClip(tallClip,1,parent.SharedLibrary.Root);project.UseSharedClip(stoutClip,1,parent.SharedLibrary.Root);
        foreach(var character in project.State.Characters)project.DetectHumanoid(character.Id);
        Assert(project.State.Humanoids.All(p=>p.Reviewed) && project.State.Bodies.All(p=>p.Reviewed),"temporal original fixtures reuse their author's reviewed exact revisions");

        var frozen=new CharacterProject(Path.Combine(_output,"storage"));
        frozen.UseSharedClip(stoutClip,1,parent.SharedLibrary.Root);
        frozen.Assign(stoutClip,frozen.State.Animations.Single(a=>a.Id==stoutClip).SourceCharacterId,"Unchanged second project");
        string frozenFile=Path.Combine(_output,"humanoid-temporal-untouched.fpcharacter");frozen.Save(frozenFile);string frozenHash=Hash(frozenFile);

        string first=TemporalNativeCase(project,tallClip,stoutClip,true,"tall-to-stout-hold",true);
        TemporalNativeCase(project,stoutClip,tallClip,false,"stout-to-tall-hold",false);
        CheckTemporalFractionalContactRejection(project,tallClip,stoutClip);

        // This trajectory is independently authored from the original artwork's
        // dimensions. It approaches a deeply folded unequal-leg pose slowly, while
        // retaining the original first/last straight standing pose and exact duration.
        string source=WriteTemporalCrouch(project,tallClip),sourceHash=Hash(source);
        string deep=project.AddAnimation(project.State.Animations.Single(a=>a.Id==tallClip).SourceCharacterId,source);
        string deepRecipe=TemporalNativeCase(project,deep,stoutClip,true,"deep-crouch-hold",false);
        Assert(Hash(source)==sourceHash,"independent native crouch input remains unchanged after transfer");
        string loopSource=WriteTemporalHoldLoop(project,tallClip),loopHash=Hash(loopSource);
        string loop=project.AddAnimation(project.State.Animations.Single(a=>a.Id==tallClip).SourceCharacterId,loopSource);
        TemporalNativeCase(project,loop,stoutClip,true,"animated-finger-loop-hold",false);
        Assert(Hash(loopSource)==loopHash,"independent native finger-loop input remains unchanged after held seam transfer");

        string file=Path.Combine(_output,"humanoid-temporal.fpcharacter");project.Save(file);
        var loaded=CharacterProject.Load(file,Path.Combine(_output,"storage"));
        string restored=Path.Combine(_output,"temporal-assignment-reopened.brres");loaded.ExportInstance(first,restored);
        VerifyTemporalNative(restored,loaded.State.Characters.Single(c=>c.Id==loaded.State.Instances.Single(i=>i.Id==first).TargetCharacterId),loaded.NativeAnimation(first),loaded.HumanoidSetupFor(loaded.State.Instances.Single(i=>i.Id==first).TargetCharacterId).Body,true,31,"restored-assignment",null);
        using(var repeat=loaded.PreviewHumanoid(loaded.State.HumanoidRecipes.Single(r=>r.Id==deepRecipe)))
            Assert(Hash(Path.Combine(_output,"deep-crouch-hold.brres"))==repeat.CorrectedHash,"saved deep-crouch recipe regenerates identical native output after fresh project load");

        Assert(Hash(frozenFile)==frozenHash && parentState.SequenceEqual(WorkspaceStateXml(parent)),"temporal native tests preserve the independent second project and parent transaction state");
        foreach(var pin in pins)Assert(parent.SharedLibrary.Pin(pin.Kind,pin.Id,pin.Revision).MetadataHash==pin.MetadataHash,"shared original clip revisions remain immutable during temporal native authoring");
        foreach(string hash in payloads)parent.SharedLibrary.ReadBlob(hash);
        Log("PASS: independent original endpoint/withheld-fraction sole goals, near-straight/deep-fold native output, both directions, held contact edges, native CHR0/BRRES reopen, project reload and independent source/project preservation.");
    }

    // Returns the assigned instance for the first case, otherwise the saved recipe.
    private static string TemporalNativeCase(CharacterProject project,string clip,string native,bool targetStout,string label,bool assign)
    {
        var animation=project.State.Animations.Single(a=>a.Id==clip);var destination=project.State.Animations.Single(a=>a.Id==native);
        var target=project.State.Characters.Single(c=>c.Id==destination.SourceCharacterId);var body=project.HumanoidSetupFor(target.Id).Body;
        var recipe=project.CreateHumanoidRecipe(clip,native);recipe.ReachBlend=.05;
        foreach(string side in new[]{"Left","Right"})recipe.Contacts.Add(new ContactInterval{
            Chain=side+"Leg",Start=0,End=animation.Frames-1,Margin=4,HoldStart=true,HoldEnd=true,
            PolePolicy="TransportedRest",ReachPolicy="RestHemisphere",
            Goal=new HVec((side=="Left"?1:-1)*(targetStout?1.5:1),0,targetStout?.4:.405),
            Evidence="Original artwork author's standing sole center: floor Y=0, authored hip-width X, foot/toe extent center Z. Explicit held start/end; no solver-derived target or automatic contact classification."});
        Assert(recipe.Contacts.All(c=>c.Weight(0)==1 && c.Weight(animation.Frames-1)==1),"explicit full-clip standing hold includes first/last frames despite a saved blend margin");
        project.SaveHumanoidRecipe(recipe);
        using(var trial=project.PreviewHumanoid(recipe))
        {
            Assert(trial.Report.UnreachableSamples==0 && trial.Report.CorrectedContactError<body.LegLength*.005,"held temporal native output meets unchanged contact/reach acceptance");
            string output=Path.Combine(_output,label+".brres");project.ExportHumanoidTrial(trial,output);
            string chr=Path.Combine(_output,label+".chr0");using(var root=NodeFactory.FromFile(null,output))((CHR0Node)LocalNativeAt(root,destination.NativePath)).Export(chr);
            VerifyTemporalNative(output,target,destination,body,targetStout,animation.Frames,label,null,animation.Loop);
            VerifyTemporalNative(output,target,destination,body,targetStout,animation.Frames,label+"-standalone",chr,animation.Loop);
            File.WriteAllText(Path.Combine(_output,label+"-report.txt"),trial.Report.ToString());
        }
        if(!assign)return recipe.Id;
        string instance=project.AssignHumanoid(recipe.Id,"Original endpoint-preserving standing motion");
        project.Undo();Assert(!project.State.Instances.Any(i=>i.Id==instance),"native held-stance assignment undoes atomically");project.Redo();Assert(project.State.Instances.Any(i=>i.Id==instance),"native held-stance assignment redoes atomically");return instance;
    }

    private static void VerifyTemporalNative(string file,SourceCharacterProfile target,AnimationAsset native,BodyProfile body,bool stout,int frames,string label,string standalone,bool loop=false)
    {
        int end=loop?frames+1:frames-1;var times=new SortedSet<float>{0,frames-1,end};
        // Neither thirds used to fit native tangents nor quarter-frame acceptance
        // samples cover these independently chosen withheld phases.
        for(int f=0;f<end;f++)foreach(float phase in new[]{.125f,.137f,.619f,.875f})times.Add(f+phase);
        var rows=new List<string>{"time\tside\tlandmarkError\tsoleFloorError\tkneeZ"};double worst=0,soleWorst=0,adjacent=0;var previous=new Dictionary<string,HRotation>();var starts=new Dictionary<string,HVec>();
        using(var root=NodeFactory.FromFile(null,file))using(var loose=standalone==null?null:NodeFactory.FromFile(null,standalone))
        {
            var model=(MDL0Node)LocalNativeAt(root,target.ModelPath);model.Populate();var motion=(CHR0Node)(loose??LocalNativeAt(root,native.NativePath));
            Assert(motion.Version==4 && motion.FrameCount==frames && motion.Loop==loop,"temporal output preserves actual native duration/version/loop without padding or relabelling");
            if(loop)
            {
                var finger=motion.Children.OfType<CHR0EntryNode>().Single(e=>e.Name=="LeftIndex1");
                Assert(Math.Abs(finger.GetFrameValue(5,15)-finger.GetFrameValue(5,0))>19 && Math.Abs(finger.GetFrameValue(5,30.619f)-finger.GetFrameValue(5,0))>.1,"held-contact loop retains real finger motion, including nonempty inherited closure samples");
            }
            foreach(float time in times)
            {
                // CHR0's inherited key-array loop period includes its two-frame
                // closure interval after last authored index (30 -> 32 here).
                float nativeTime=loop && time==end?0:time;model.ApplyCHR(motion,nativeTime+1);
                foreach(string side in new[]{"Left","Right"})
                {
                    var foot=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name==side+"Foot");var shin=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name==side+"Shin");var landmark=body.Landmark(side+"Leg").Local;
                    var v=foot.Matrix*new Vector3((float)landmark.X,(float)landmark.Y,(float)landmark.Z);var point=new HVec(v._x,v._y,v._z);var goal=new HVec((side=="Left"?1:-1)*(stout?1.5:1),0,stout?.4:.405);
                    double error=(point-goal).Length;worst=Math.Max(worst,error);Assert(error<.005,"withheld native sole landmark matches independently authored standing goal, including both endpoints");
                    var sole=model._objList.OfType<MDL0ObjectNode>().SelectMany(m=>m.Vertices).Where(vertex=>vertex.GetMatrixNode().IsPrimaryNode && vertex.GetMatrixNode().Weights[0].Bone==foot).ToArray();
                    Assert(sole.Length>0,"native target keeps actual rigid foot geometry and original weights");double floor=Math.Abs(sole.Min(vertex=>vertex.WeightedPosition._y));soleWorst=Math.Max(soleWorst,floor);Assert(floor<.005,"fresh decoded target sole geometry stays on the independently authored floor");
                    Assert(shin.Matrix[14]>=-.005,"independent reviewed +Z knee-bend half-plane does not flip across straight/folded poses");
                    foreach(string joint in new[]{side+"Thigh",side+"Shin"})
                    {
                        var state=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name==joint).FrameState;var local=RigidTransform.From(new RigPose{RX=state._rotate._x,RY=state._rotate._y,RZ=state._rotate._z});var rotation=new HRotation{V=local.R};
                        if(previous.TryGetValue(joint,out var prior)){double delta=rotation.DegreesFrom(prior);adjacent=Math.Max(adjacent,delta);Assert(delta<15,"slow original native trajectory has no discontinuous joint/pole branch jump between withheld samples");}previous[joint]=rotation;
                    }
                    if(time==0)starts.Add(side,point);if(time==frames-1)Assert((point-starts[side]).Length<.005,"explicit standing start/end sole positions agree in freshly reopened native motion");
                    rows.Add(string.Join("\t",time.ToString("R",CultureInfo.InvariantCulture),side,error.ToString("R",CultureInfo.InvariantCulture),floor.ToString("R",CultureInfo.InvariantCulture),shin.Matrix[14].ToString("R",CultureInfo.InvariantCulture)));
                }
            }
        }
        rows.Add("worst landmark="+worst.ToString("R",CultureInfo.InvariantCulture)+"; floor="+soleWorst.ToString("R",CultureInfo.InvariantCulture)+"; adjacent local degrees="+adjacent.ToString("R",CultureInfo.InvariantCulture));File.WriteAllLines(Path.Combine(_output,label+"-independent-goals.tsv"),rows);
    }

    private static string WriteTemporalCrouch(CharacterProject project,string clip)
    {
        const int frames=121;const double first=4.5,second=5,depth=5.4;
        string source=project.PreviewSource(clip),folder=Path.Combine(_output,"temporal-original-motion");Directory.CreateDirectory(folder);string output=Path.Combine(folder,"original-deep-crouch.chr0");
        var character=project.State.Characters.Single(c=>c.Id==project.State.Animations.Single(a=>a.Id==clip).SourceCharacterId);
        try
        {
            using(var root=NodeFactory.FromFile(null,source))using(var motion=new CHR0Node{Name="OriginalSlowDeepCrouch",Version=4,FrameCount=frames,Loop=false})
            {
                var model=(MDL0Node)LocalNativeAt(root,character.ModelPath);model.Populate();
                foreach(var bone in model.BoneCache.OfType<MDL0BoneNode>())
                {
                    var rest=bone.BindState;var values=new float[frames][];
                    for(int f=0;f<frames;f++)
                    {
                        double drop=depth*Math.Pow(Math.Sin(f*Math.PI/(frames-1)),2),distance=first+second-drop;
                        double alpha=Math.Acos((first*first+distance*distance-second*second)/(2*first*distance))*180/Math.PI,beta=Math.Acos((second*second+distance*distance-first*first)/(2*second*distance))*180/Math.PI;
                        values[f]=new[]{1f,1,1,rest._rotate._x,rest._rotate._y,rest._rotate._z,rest._translate._x,rest._translate._y,rest._translate._z};
                        if(bone.Name=="Pelvis")values[f][7]-=(float)drop;if(bone.Name.EndsWith("Thigh"))values[f][3]=(float)-alpha;if(bone.Name.EndsWith("Shin"))values[f][3]=(float)(alpha+beta);if(bone.Name.EndsWith("Foot"))values[f][3]=(float)-beta;
                    }
                    var entry=motion.CreateEntry(bone.Name);entry.PreserveInterpolation=true;
                    for(int ch=0;ch<9;ch++)for(int f=0;f<frames;f++){int a=Math.Max(0,f-1),b=Math.Min(frames-1,f+1);var key=entry.SetKeyframe(ch,f,values[f][ch],true);key._tangent=bone.Name=="Pelvis" && ch==7?(float)(-depth*Math.PI/(frames-1)*Math.Sin(2*f*Math.PI/(frames-1))):(values[b][ch]-values[a][ch])/(b-a);}
                }
                motion.Export(output);
                using(var decoded=(CHR0Node)NodeFactory.FromFile(null,output))foreach(float t in new[]{0f,.137f,19.619f,59.875f,60f,100.137f,119.875f,120f})
                {
                    model.ApplyCHR(decoded,t+1);foreach(string side in new[]{"Left","Right"}){var foot=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name==side+"Foot");var sole=foot.Matrix*new Vector3(0,-.5f,.4f);Assert(Math.Abs(sole._y)<.005 && Math.Abs(sole._z-.4)<.005,"independently authored source deep crouch keeps its physical sole on the floor before transfer");}
                }
            }
            return output;
        }
        finally{File.Delete(source);}
    }

    private static string WriteTemporalHoldLoop(CharacterProject project,string clip)
    {
        string source=project.PreviewSource(clip),folder=Path.Combine(_output,"temporal-original-loop-motion");Directory.CreateDirectory(folder);string output=Path.Combine(folder,"original-held-finger-loop.chr0");
        var character=project.State.Characters.Single(c=>c.Id==project.State.Animations.Single(a=>a.Id==clip).SourceCharacterId);
        try
        {
            using(var root=NodeFactory.FromFile(null,source))using(var motion=new CHR0Node{Name="OriginalHeldFingerLoop",Version=4,FrameCount=31,Loop=true})
            {
                var model=(MDL0Node)LocalNativeAt(root,character.ModelPath);model.Populate();
                foreach(var bone in model.BoneCache.OfType<MDL0BoneNode>())
                {
                    var rest=bone.BindState;var entry=motion.CreateEntry(bone.Name);entry.PreserveInterpolation=true;
                    var defaults=new[]{1f,1,1,rest._rotate._x,rest._rotate._y,rest._rotate._z,rest._translate._x,rest._translate._y,rest._translate._z};
                    for(int ch=0;ch<9;ch++)
                    {
                        bool finger=ch==5 && bone.Name.Contains("Index");
                        foreach(int f in finger?Enumerable.Range(0,31):new[]{0,30})
                        {
                            var key=entry.SetKeyframe(ch,f,defaults[ch]+(finger?(float)(20*Math.Sin(f*Math.PI/30)):0),true);
                            key._tangent=finger?(float)(20*Math.PI/30*Math.Cos(f*Math.PI/30)):0;
                        }
                    }
                }
                motion.Export(output);
                using(var decoded=(CHR0Node)NodeFactory.FromFile(null,output))foreach(float t in new[]{0f,.137f,15.619f,30f,30.137f,31.619f,31.875f})
                {
                    model.ApplyCHR(decoded,t+1);foreach(string side in new[]{"Left","Right"}){var foot=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name==side+"Foot");var sole=foot.Matrix*new Vector3(0,-.5f,.4f);Assert(Math.Abs(sole._y)<.005 && Math.Abs(sole._z-.4)<.005,"independent original loop's actual native sole stays fixed through inherited seam before transfer");}
                }
            }
            return output;
        }
        finally{File.Delete(source);}
    }

    private static void CheckTemporalFractionalContactRejection(CharacterProject project,string clip,string native)
    {
        byte[] before=WorkspaceStateXml(project);int count=project.State.Instances.Count;
        var shortContact=project.CreateHumanoidRecipe(clip,native);
        // Every integer/third/quarter/.137/.619 sample misses this authored peak.
        // Its independently selected reachable sideways sole shift must not silently
        // disappear merely because the native bake has no integer key in the interval.
        shortContact.Contacts.Add(new ContactInterval{Chain="LeftLeg",Start=.14,End=.16,Margin=.005,Goal=new HVec(1.75,0,.4),PolePolicy="TransportedRest",ReachPolicy="RestHemisphere",Evidence="Original author deliberately requests a narrow .25-unit lateral contact correction between all regular native bake/validation samples."});
        shortContact.Validate(project.State.Animations.Single(a=>a.Id==clip).Frames);
        IOException failure=null;
        try{using(var unexpected=project.PreviewHumanoid(shortContact)){} }
        catch(IOException error){failure=error;}
        File.WriteAllText(Path.Combine(_output,"temporal-narrow-contact-rejection.txt"),failure?.ToString()??"No IOException observed; the omitted correction was not rejected.");
        Assert(failure!=null && failure.Message.Contains("serialization/evaluation tolerance"),"exact fractional contact peak is evaluated and rejects a native curve that omits the independently requested correction; observed: "+(failure?.Message??"no IOException"));
        var subFloat=project.CreateHumanoidRecipe(clip,native);double start=15+1e-12,end=start+1e-12;
        subFloat.Contacts.Add(new ContactInterval{Chain="LeftLeg",Start=start,End=end,Margin=(end-start)/2,Goal=new HVec(1.75,0,.4),PolePolicy="TransportedRest",ReachPolicy="RestHemisphere",Evidence="Original negative interval intentionally collapses below the native floating-point playhead resolution."});
        subFloat.Validate(project.State.Animations.Single(a=>a.Id==clip).Frames);failure=null;
        try{using(var unexpected=project.PreviewHumanoid(subFloat)){} }
        catch(IOException error){failure=error;}
        File.WriteAllText(Path.Combine(_output,"temporal-subfloat-contact-rejection.txt"),failure?.ToString()??"No IOException observed; collapsed contact timing was not rejected.");
        Assert(failure!=null && failure.Message.Contains("float time precision"),"contact interval below native time resolution rejects before a silent zero-sample output; observed: "+(failure?.Message??"no IOException"));
        Assert(count==project.State.Instances.Count && before.SequenceEqual(WorkspaceStateXml(project)),"unrepresentable fractional contact output never commits an assignment or mutates saved project state");
        Log("PASS: explicitly sampled narrow fractional contact rejects the omitted native correction; intervals below native time resolution reject without project changes.");
    }

    private static void CheckHumanoidInteriorTangentFit()
    {
        var method=typeof(CharacterProject).Assembly.GetType("BrawlCrate.Authoring.BrawlProjectAdapter",true).GetMethod("FitHumanoidTangents",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
        var phases=new[]{1d/6,1d/3,.5,2d/3,5d/6};
        Func<double,double> authored=time=>.13*time*time*time-.7*time*time+1.2*time-4;
        Func<double,double> derivative=time=>.39*time*time-1.4*time+1.2;
        var values=Enumerable.Range(0,9).Select(f=>authored(f)).ToArray();Func<int,int,double> sample=(f,part)=>authored(f+phases[part]);
        var tangents=(double[])method.Invoke(null,new object[]{values,phases,sample,System.Threading.CancellationToken.None});
        for(int f=0;f<values.Length;f++)Assert(Math.Abs(tangents[f]-derivative(f))<1e-10,"five-phase constrained native tangent fit recovers independent analytic cubic derivatives");
        string folder=Path.Combine(_output,"temporal-fit-original");Directory.CreateDirectory(folder);string output=Path.Combine(folder,"original-cubic.chr0");
        using(var curve=new CHR0Node{Name="OriginalCubic",Version=4,FrameCount=values.Length,Loop=false})
        {
            var track=curve.CreateEntry("Root");track.PreserveInterpolation=true;
            for(int f=0;f<values.Length;f++)track.SetKeyframe(5,f,(float)values[f],true)._tangent=(float)tangents[f];curve.Export(output);
        }
        string original=Hash(output);using(var decoded=(CHR0Node)NodeFactory.FromFile(null,output))
        {
            var track=decoded.Children.OfType<CHR0EntryNode>().Single();
            for(int f=0;f<values.Length-1;f++)foreach(float phase in new[]{.125f,.137f,.25f,.619f,.75f,.875f})
                Assert(Math.Abs(track.GetFrameValue(5,f+phase)-authored(f+phase))<.0001,"fresh native Hermite encoding reproduces independently authored cubic at withheld fractional phases");
        }
        Assert(Hash(output)==original,"independent cubic native output remains unchanged during decode validation");
    }
}
