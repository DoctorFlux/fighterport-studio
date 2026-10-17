// FighterPort opt-in native output evidence, 2026-10-17. Repository LGPL-3.0 terms.
// Reads configured local inputs/outputs only. Reports and commercial data stay ignored.
using BrawlCrate.Authoring;
using BrawlCrate.Authoring.Humanoid;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;

internal static partial class NativeSmoke
{
    private static void CheckLocalPreviewFeet(ModelEditControl editor,CharacterProject project,HumanoidRecipe recipe,string label)
    {
        if(!recipe.Contacts.All(c=>c.HoldStart&&c.HoldEnd))return;
        var setup=project.HumanoidSetupFor(recipe.TargetCharacterId);var model=(MDL0Node)editor.TargetModel;
        foreach(int frame in new[]{1,editor.SelectedCHR0.FrameCount})
        {
            editor.SetFrame(frame);
            foreach(var contact in recipe.Contacts)
            {
                int bone=setup.Profile.Chain(contact.Chain).Bones.Last();var local=setup.Body.Landmark(contact.Chain).Local;
                var point=model.BoneCache[bone].Matrix*new Vector3((float)local.X,(float)local.Y,(float)local.Z);
                Assert((new HVec(point._x,point._y,point._z)-contact.Goal).Length<.002,"actual native preview's first/last standing sole meets its explicit goal");
            }
            CaptureViewport(editor,label+"-"+frame+".png");
        }
    }
    private static int LocalContactReport(string projectFile,string nativeFile,string evidence)
    {
        try
        {
            if(Directory.Exists(evidence))throw new IOException("Use a new private evidence directory.");
            Directory.CreateDirectory(evidence);_output=Path.GetFullPath(evidence);
            var storage=Path.Combine(_output,"storage");BrawlLib.Internal.IO.SourceWriteGuard.Initialize(Path.Combine(storage,"source-roots.txt"));
            string before=Hash(projectFile);var project=CharacterProject.Load(projectFile,storage);
            var recipe=project.State.HumanoidRecipes.Single();CheckLocalStandingOutput(project,recipe,nativeFile,"existing-output",false);
            Assert(Hash(projectFile)==before,"contact inventory does not rewrite the source project");
            Console.WriteLine("Recorded native endpoint diagnostics only; this is not a passing contact certificate.");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
    private static void CheckLocalStandingOutput(CharacterProject project,HumanoidRecipe recipe,string nativeFile,string label,bool requireHeld)
    {
        var state=project.State;var character=state.Characters.Single(c=>c.Id==recipe.TargetCharacterId);
        var animation=state.Animations.Single(a=>a.Id==recipe.TargetAnimationId);var setup=project.HumanoidSetupFor(character.Id);
        var contacts=recipe.Contacts.Where(c=>c.Anchor=="World" && c.Chain.EndsWith("Leg")).ToArray();
        Assert(contacts.Length==2,"explicit local standing evidence needs both configured world-foot goals");
        var rows=new List<string>{"time\tchain\tx\ty\tz\tgoal-error\tfoot-toe-min-y"};
        var first=new Dictionary<string,HVec>();var last=new Dictionary<string,HVec>();
        double firstError=0,lastError=0,maximum=0,floor=double.PositiveInfinity;
        using(var root=NodeFactory.FromFile(null,nativeFile))
        {
            var model=(MDL0Node)LocalNativeAt(root,character.ModelPath);model.Populate();var motion=(CHR0Node)LocalNativeAt(root,animation.NativePath);
            var times=new SortedSet<float>{0,motion.FrameCount-1};
            for(int f=0;f<motion.FrameCount-1;f++)foreach(float fraction in new[]{.125f,.137f,.619f,.875f})times.Add(f+fraction);
            foreach(float time in times)
            {
                model.ApplyCHR(motion,time+1);
                foreach(var contact in contacts)
                {
                    int bone=setup.Profile.Chain(contact.Chain).Bones.Last();var p=setup.Body.Landmark(contact.Chain).Local;
                    var point=model.BoneCache[bone].Matrix*new Vector3((float)p.X,(float)p.Y,(float)p.Z);var actual=new HVec(point._x,point._y,point._z);double error=(actual-contact.Goal).Length;
                    if(time==0){first[contact.Chain]=actual;firstError=Math.Max(firstError,error);}
                    if(time==motion.FrameCount-1){last[contact.Chain]=actual;lastError=Math.Max(lastError,error);}
                    maximum=Math.Max(maximum,error);
                    var bones=new HashSet<int>(setup.Profile.Chains.Where(c=>c.Role==contact.Chain.Replace("Leg","Toe")).SelectMany(c=>c.Bones)){bone};
                    var names=new HashSet<string>(bones.Select(i=>character.Bones[i].Name));
                    double minimum=model._objList.OfType<MDL0ObjectNode>().SelectMany(m=>m.Vertices).Where(v=>v.GetMatrixNode().Weights.Any(w=>w.Weight>.25 && names.Contains(w.Bone.Name))).Select(v=>(double)v.WeightedPosition._y).DefaultIfEmpty(double.NaN).Min();
                    Assert(HVec.Finite(minimum),"native local sole geometry must exist and remain finite");floor=Math.Min(floor,minimum);
                    rows.Add(string.Join("\t",new[]{time.ToString("R",CultureInfo.InvariantCulture),contact.Chain,actual.X.ToString("R",CultureInfo.InvariantCulture),actual.Y.ToString("R",CultureInfo.InvariantCulture),actual.Z.ToString("R",CultureInfo.InvariantCulture),error.ToString("R",CultureInfo.InvariantCulture),minimum.ToString("R",CultureInfo.InvariantCulture)}));
                }
            }
        }
        double drift=contacts.Max(c=>(last[c.Chain]-first[c.Chain]).Length);
        File.WriteAllLines(Path.Combine(_output,label+"-feet.tsv"),rows);
        string report=string.Format(CultureInfo.InvariantCulture,"Independent native world-sole goals: first={0:R}, last={1:R}, first-last drift={2:R}, maximum (endpoints + withheld .125/.137/.619/.875)={3:R}; foot/toe skin minY={4:R}, configured groundY={5:R}. Held endpoints required={6}.",firstError,lastError,drift,maximum,floor,setup.Body.GroundY,requireHeld);
        File.WriteAllText(Path.Combine(_output,label+"-feet.txt"),report);Console.WriteLine(report);
        if(requireHeld)
        {
            Assert(contacts.All(c=>c.HoldStart&&c.HoldEnd),"local standing endpoint check requires explicitly persisted holds");
            Assert(firstError<.002 && lastError<.002 && drift<.002,"native first and last standing sole positions match explicit goals and each other within .002 units");
            Assert(maximum<setup.Body.LegLength*.005,"withheld native samples satisfy unchanged target contact quality");
        }
    }
}
