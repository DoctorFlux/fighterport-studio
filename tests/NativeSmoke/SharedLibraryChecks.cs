// FighterPort original shared-library/migration acceptance, 2026-10-17. No game data.
using BrawlCrate.Authoring;
using BrawlCrate.UI;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

internal static partial class NativeSmoke
{
    private static void ExerciseShared(bool gui)
    {
        string a=FkFixture(false),b=FkFixture(true),storage=Path.Combine(_output,"storage"),library=Path.Combine(_output,"shared-library");
        var seed=new CharacterProject(storage);string aid=seed.RegisterSource(LibraryResolution(a,"source-a"),"Shared A"),bid=seed.RegisterSource(LibraryResolution(b,"source-b"),"Shared B");
        var aa=seed.State.Animations.Single(c=>c.SourceCharacterId==aid);var ab=seed.State.Animations.Single(c=>c.SourceCharacterId==bid);
        string legacyMove=seed.CreateDraft(aa.Id,"Legacy compatibility data only",7,3,18,9.5f);string legacy=seed.Assign(aa.Id,aid,"LegacyPreserved",legacyMove);
        var oldLease=seed.OpenInstance(legacy);using(var root=NodeFactory.FromFile(null,oldLease.Path)){Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single().SetKeyframe(5,15,77);seed.CaptureNativeEdits(oldLease,root,oldLease.Path);}
        string v1=Path.Combine(_output,"original-v1.fpcharacter");seed.Save(v1);
        // Reproduce the b45fcc3 schema, which has none of the new optional fields.
        var legacyXml=new XmlDocument {XmlResolver=null};legacyXml.Load(v1);
        foreach(XmlNode node in legacyXml.SelectNodes("//AnimationAsset/NativeHash|//AnimationAsset/AdditionalOriginals|//SourceCharacterProfile/InitialAnimationId|//State/RigMaps|//State/LibraryRoot|//State/LibraryId|//State/LibraryPins|//MoveInstance/NativeAnimationId|//MoveInstance/MappingId|//MoveInstance/MappingRevision|//MoveInstance/BakeEvidence").Cast<XmlNode>().ToArray())node.ParentNode.RemoveChild(node);
        legacyXml.Save(v1);string oldHash=Hash(v1);
        seed=CharacterProject.Load(v1,storage);seed.MigrateToSharedLibrary(library);
        Assert(seed.State.Version==2 && seed.State.LibraryId==seed.SharedLibrary.Id,"deliberate v1 migration into shared library");
        Assert(seed.State.Moves.Single().Damage==9.5f && seed.State.Instances.Single().MoveId==legacyMove && Hash(v1)==oldHash,"v1 native edits/draft identity and recovery file preserved");
        seed.ExportInstance(legacy,Path.Combine(_output,"migrated-v1-edit.brres"));AssertSharedKey(Path.Combine(_output,"migrated-v1-edit.brres"),77);
        string extra=Path.Combine(_output,"additional-originals");Directory.CreateDirectory(extra);string[] motions=new string[2];
        for(int i=0;i<2;i++)using(var native=NodeFactory.FromFile(null,a))
        {
            var motion=Find<CHR0Node>(native);motion.Name="Shared alternative "+i;motion.Children.OfType<CHR0EntryNode>().Single().SetKeyframe(5,15,50+i);
            motions[i]=Path.Combine(extra,"clip"+i+".chr0");motion.Export(motions[i]);
        }
        var originals=new[]{a,b}.Concat(motions).ToDictionary(p=>p,Hash);
        string added=seed.AddAnimation(aid,motions[0]);seed.AddAnimation(aid,motions[1]);
        Assert(seed.State.Characters.Count==2 && seed.State.Animations.Count(c=>c.SourceCharacterId==aid)==3,"one source registration with several clips");
        var page=seed.SharedLibrary.Page(0,2);Assert(page.Clips.Count==2 && page.HasMore,"bounded metadata page");
        Expect<OperationCanceledException>(()=>seed.SharedLibrary.Page(0,2,"",new CancellationToken(true)),"cancelled library index");
        Expect<IOException>(()=>seed.SharedLibrary.Page(0,65),"library page budget enforced");
        Expect<OperationCanceledException>(()=>seed.AddAnimation(aid,motions[0],new CancellationToken(true)),"cancelled import");
        var altered=seed.State.Animations.Single(c=>c.Id==aa.Id);altered.Name="Changed immutable revision";
        Expect<IOException>(()=>seed.SharedLibrary.Publish(altered),"immutable metadata conflict rejects");
        using(var cursor=seed.SharedLibrary.OpenCursor())
        {
            seed.SharedLibrary.PutBlob(new byte[]{1,2,3,4});Expect<IOException>(()=>cursor.Next(),"generation change invalidates lazy cursor");
        }
        System.Threading.Tasks.Parallel.For(0,4,i=>seed.SharedLibrary.PutBlob(new byte[]{5,6,7,8}));
        string missingRoot=Path.Combine(_output,"absent-library");Expect<IOException>(()=>new AnimationLibraryStore(missingRoot,true),"missing pinned library rejects");Assert(!Directory.Exists(missingRoot),"no empty replacement library created");
        string metadata=Path.Combine(library,"animations",aa.Id+"-000001.fplibrecord");var metadataBytes=File.ReadAllBytes(metadata);
        File.SetAttributes(metadata,FileAttributes.Normal);try{File.WriteAllBytes(metadata,new byte[]{0});Expect<IOException>(()=>seed.SharedLibrary.Read(seed.State.LibraryPins.Single(p=>p.Id==aa.Id)),"changed pinned metadata rejects");}finally{File.WriteAllBytes(metadata,metadataBytes);File.SetAttributes(metadata,FileAttributes.ReadOnly);}
        string map=seed.ReviewRigMap(OriginalMap(seed.State.Characters.Single(c=>c.Id==aid),seed.State.Characters.Single(c=>c.Id==bid)));
        string first=seed.AssignRetargeted(aa.Id,ab.Id,map,"SharedRetargeted");
        var second=new CharacterProject(storage);second.UseSharedClip(aa.Id,1,library);second.UseSharedClip(added,1,library);string other=second.Assign(aa.Id,aid,"OtherProject");
        string file1=Path.Combine(_output,"project-one.fpcharacter"),file2=Path.Combine(_output,"project-two.fpcharacter");seed.Save(file1);second.Save(file2);
        string secondHash=Hash(file2),clipHash=seed.SharedLibrary.Pin("animations",aa.Id).MetadataHash,nativeHash=aa.NativeHash;
        var doc=new XmlDocument {XmlResolver=null};doc.Load(file1);Assert(doc.SelectNodes("//Blobs/ProjectBlob").Count==0,"shared native library data is not embedded in project XML");
        Assert(new FileInfo(file1).Length<100000,"project stores a bounded selected graph, not the whole shared library");
        var lease=seed.OpenInstance(first);using(var root=NodeFactory.FromFile(null,lease.Path))
        {
            Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single(e=>e.Name=="Paddle").SetKeyframe(5,15,25);seed.CaptureNativeEdits(lease,root,lease.Path);
        }
        seed.Save(file1);Assert(Hash(file2)==secondHash && seed.SharedLibrary.Pin("animations",aa.Id).MetadataHash==clipHash,"editing one project does not change other project or shared revision");
        Assert(seed.SharedLibrary.ReadBlob(nativeHash).Length>0 && seed.State.Animations.Single(c=>c.Id==aa.Id).NativeHash==nativeHash,"shared source native revision immutable");
        seed.ExportInstance(first,Path.Combine(_output,"shared-edited.brres"));second.ExportInstance(other,Path.Combine(_output,"shared-other.brres"));AssertSharedKey(Path.Combine(_output,"shared-edited.brres"),25);AssertSharedKey(Path.Combine(_output,"shared-other.brres"),180);
        seed=CharacterProject.Load(file1,storage);second=CharacterProject.Load(file2,storage);Assert(seed.State.LibraryPins.Single(p=>p.Id==aa.Id).MetadataHash==second.State.LibraryPins.Single(p=>p.Id==aa.Id).MetadataHash,"two independently reopened project files pin the same clip revision");
        File.WriteAllText(Path.Combine(_output,"other-project.sha256"),secondHash);File.WriteAllText(Path.Combine(_output,"shared-clip-id.txt"),aa.Id);File.WriteAllText(Path.Combine(_output,"shared-clip.sha256"),clipHash);
        if(gui) SharedGui(second);
        using(var child=Process.Start(new ProcessStartInfo {FileName=Process.GetCurrentProcess().MainModule.FileName,Arguments="--verify-shared \""+_output+"\"",UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardOutput=true,RedirectStandardError=true}))
        {
            if(!child.WaitForExit(25000)){child.Kill();throw new TimeoutException("Shared project restart");}Assert(child.ExitCode==0,child.StandardError.ReadToEnd());Log(child.StandardOutput.ReadToEnd().Trim());
        }
        foreach(var original in originals)Assert(Hash(original.Key)==original.Value,"shared-library originals unchanged");File.WriteAllLines(Path.Combine(_output,"shared-originals.sha256"),originals.Select(p=>p.Value+"\t"+p.Key));
        Log("PASS: disk-backed shared library, multi-clip source, deliberate v1 migration, separate project files, independent target edits, lazy metadata pages, native outputs and unchanged originals");
    }
    private static void AssertSharedKey(string file,float value)
    {
        using(var root=NodeFactory.FromFile(null,file))Assert(Math.Abs(Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single(e=>e.Name=="Paddle").GetFrameValue(5,15)-value)<.01,"decoded shared/native instance key");
    }
    private static int VerifySharedRestart(string directory)
    {
        try
        {
            _output=directory;Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT",Path.Combine(directory,"settings"));BrawlLib.Internal.IO.SourceWriteGuard.Initialize(Path.Combine(directory,"storage","source-roots.txt"));
            var first=CharacterProject.Load(Path.Combine(directory,"project-one.fpcharacter"),Path.Combine(directory,"storage"));var second=CharacterProject.Load(Path.Combine(directory,"project-two.fpcharacter"),Path.Combine(directory,"storage"));
            string id=File.ReadAllText(Path.Combine(directory,"shared-clip-id.txt"));Assert(first.State.LibraryPins.Single(p=>p.Id==id).MetadataHash==File.ReadAllText(Path.Combine(directory,"shared-clip.sha256")),"shared immutable revision after restart");
            first.ExportInstance(first.State.Instances.Single(i=>i.Slot=="SharedRetargeted").Id,Path.Combine(directory,"restart-one.brres"));second.ExportInstance(second.State.Instances.Single(i=>i.Slot=="OtherProject").Id,Path.Combine(directory,"restart-two.brres"));
            AssertSharedKey(Path.Combine(directory,"restart-one.brres"),25);AssertSharedKey(Path.Combine(directory,"restart-two.brres"),180);
            string firstInstance=first.State.Instances.Single(i=>i.Slot=="SharedRetargeted").Id;var lease=first.OpenInstance(firstInstance);
            using(var root=NodeFactory.FromFile(null,lease.Path)){Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single(e=>e.Name=="Paddle").SetKeyframe(5,15,35);first.CaptureNativeEdits(lease,root,lease.Path);}
            first.ExportInstance(firstInstance,Path.Combine(directory,"restart-edited.brres"));AssertSharedKey(Path.Combine(directory,"restart-edited.brres"),35);
            first.Undo();first.ExportInstance(firstInstance,Path.Combine(directory,"restart-undo.brres"));AssertSharedKey(Path.Combine(directory,"restart-undo.brres"),25);first.Redo();
            Assert(first.SharedLibrary.Pin("animations",id).MetadataHash==File.ReadAllText(Path.Combine(directory,"shared-clip.sha256")),"shared revision unchanged by edits after restart");
            Assert(Hash(Path.Combine(directory,"project-two.fpcharacter"))==File.ReadAllText(Path.Combine(directory,"other-project.sha256")),"other project bytes unchanged after restart");
            Console.WriteLine("PASS: independent restart of two separate shared projects retains immutable clip and edited/native-independent outputs");return 0;
        }catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
    private static void SharedGui(CharacterProject project)
    {
        var main=MainForm.Instance;Exception failure=null;main.Shown+=(s,e)=>main.BeginInvoke(new Action(()=> {
            try
            {
                using(var form=new SharedAnimationLibraryForm(project))
                {
                    form.Show(main);Pump(100);var clips=(ListView)form.Controls.Find("sharedClips",true).Single();Assert(clips.Items.Count==4,"real shared catalogue contains all source clips");CaptureWindow(form,"shared-library.png");
                    clips.Items[0].Selected=true;((Button)form.Controls.Find("sharedUse",true).Single()).PerformClick();Assert(!form.Visible,"actual shared Use button");
                }
            }catch(Exception error){failure=error;}finally{main.Dispose();Application.ExitThread();}
        }));Application.Run(main);if(failure!=null)throw new InvalidOperationException("Shared GUI",failure);
    }
}
