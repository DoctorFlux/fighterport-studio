// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms. App preferences only.
using BrawlCrate.Authoring;
using BrawlLib.Internal.IO;
using System;
using System.IO;
namespace BrawlCrate.UI
{
    public sealed class PreviewLayoutSettings
    {
        public int Version {get;set;}=1;
        public bool Visible {get;set;}=true;
        public int Width {get;set;}=340;
        private static string PathName=>Path.Combine(FighterPortPaths.DataRoot,"preview-layout.xml");
        public static PreviewLayoutSettings Read()
        {
            if(!File.Exists(PathName))return new PreviewLayoutSettings();
            var result=CharacterProject.Deserialize<PreviewLayoutSettings>(CharacterProject.ReadBounded(PathName,8192),8192);
            if(result.Version!=1 || result.Width<240 || result.Width>800)throw new IOException("Invalid preview layout; use Reset layout.");return result;
        }
        public void Save()
        {
            string path=PathName,temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";SourceWriteGuard.ThrowIfProtected(path);
            try{using(var stream=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)){SourceWriteGuard.ThrowIfProtected(stream);byte[] data=CharacterProject.Serialize(this);stream.Write(data,0,data.Length);stream.Flush(true);}if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}
            finally{if(File.Exists(temp))File.Delete(temp);}
        }
    }
}
