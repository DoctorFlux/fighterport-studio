namespace FighterPort.Core;

public sealed class EditSession
{
    private readonly Stack<CharacterProject> undo=new(),redo=new();
    public CharacterProject Project { get; private set; }
    public EditSession(CharacterProject project) { Project=project; }
    public void EditKey(int clipIndex,string bone,int channel,float frame,float value)
    {
        var candidate=ProjectStore.Clone(Project);
        var clip=candidate.Clips[clipIndex];
        var curve=clip.Curves.FirstOrDefault(c=>c.Bone==bone&&c.Channel==channel);
        if(curve==null) { curve=new(){Bone=bone,Channel=channel}; clip.Curves.Add(curve); }
        var key=curve.Keys.FirstOrDefault(k=>k.Frame==frame);
        if(key==null) { key=new(){Frame=frame}; curve.Keys.Add(key); }
        key.Value=value; key.Locked=true; curve.Keys.Sort((a,b)=>a.Frame.CompareTo(b.Frame));
        ProjectStore.Validate(candidate); undo.Push(Project); Project=candidate; redo.Clear();
    }
    public bool Undo() { if(undo.Count==0)return false; redo.Push(Project);Project=undo.Pop();return true; }
    public bool Redo() { if(redo.Count==0)return false;undo.Push(Project);Project=redo.Pop();return true; }
}
