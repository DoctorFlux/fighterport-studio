// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
// Read-only ARC directory selection. Unknown payloads are never sent to native decoders.
using BrawlLib.Internal.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BrawlCrate.Sources
{
    public sealed class BrawlContainerChoice
    {
        public string Path {get;internal set;}
        public long Offset {get;internal set;}
        public long Length {get;internal set;}
        public override string ToString()=>"BRRES ["+Path+"] / "+Length+" bytes (explicit selection; native validation on stage)";
    }
    public static class BrawlContainerSelection
    {
        public static IReadOnlyList<BrawlContainerChoice> List(string source,CancellationToken token=default(CancellationToken))
        {
            source=BrawlDumpProfile.LocalAbsolute(source);SourceWriteGuard.EnsureNoReparsePoints(source);
            using(var input=new FileStream(source,FileMode.Open,FileAccess.Read,FileShare.Read))return List(input,token);
        }
        internal static List<BrawlContainerChoice> List(FileStream input,CancellationToken token)
        {
            if(input.Length<16 || input.Length>SourceFolderSession.MaxFileBytes)throw new IOException("Selected-container directory requires 16 bytes..64 MiB.");
            var result=new List<BrawlContainerChoice>();int visited=0;Walk(input,0,input.Length,"",0,result,ref visited,token);return result;
        }
        private static void Walk(FileStream input,long offset,long size,string path,int depth,List<BrawlContainerChoice> result,ref int visited,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();if(depth>16 || ++visited>4096 || offset<0 || size<0 || offset>input.Length-size)throw new IOException("Container selection range/depth/entry budget exceeded.");
            if(size<16)return; // Empty redirects/opaque bytes are directory entries, never native inputs.
            var h=Read(input,offset,16);string tag=Encoding.ASCII.GetString(h,0,4);
            if(tag=="bres")
            {
                if(h[4]!=254 || h[5]!=255 || U32(h,8)!=size)throw new IOException("Selected BRRES byte order/declared size changed.");
                result.Add(new BrawlContainerChoice{Path=path,Offset=offset,Length=size});return;
            }
            if(tag!="ARC\0")return; // No decompression, native moveset/script parser or inferred embedded magic scan.
            if(size<64 || h[4]!=1 || h[5]!=1)throw new IOException("Unsupported raw ARC directory.");
            int count=(h[6]<<8)|h[7];if(count>4096)throw new IOException("ARC directory budget exceeded.");long cursor=offset+64;
            for(int i=0;i<count;i++)
            {
                token.ThrowIfCancellationRequested();if(cursor>offset+size-32)throw new IOException("ARC directory header outside its range.");
                byte[] entry=Read(input,cursor,32);long bytes=U32(entry,4);if(bytes>offset+size-cursor-32)throw new IOException("ARC directory payload outside its range.");
                Walk(input,cursor+32,bytes,path.Length==0?i.ToString():path+"/"+i,depth+1,result,ref visited,token);
                cursor=(cursor+32+bytes+31)&~31L;if(cursor>offset+size)throw new IOException("ARC directory alignment outside its range.");
            }
        }
        internal static byte[] Read(FileStream stream,long offset,int count)
        {if(offset<0 || offset>stream.Length-count)throw new IOException("Container selection read outside input.");stream.Position=offset;var bytes=new byte[count];int at=0,n;while(at<count && (n=stream.Read(bytes,at,count-at))>0)at+=n;if(at!=count)throw new IOException("Truncated container directory.");return bytes;}
        private static long U32(byte[] bytes,int offset)=>((long)bytes[offset]<<24)|((long)bytes[offset+1]<<16)|((long)bytes[offset+2]<<8)|bytes[offset+3];
    }
    public sealed partial class SourceFolderSession
    {
        public StagedSource StageContainer(string sourcePath,string containerPath,CancellationToken token=default(CancellationToken))
        {
            string source=CheckedSourcePath(sourcePath),staging=System.IO.Path.Combine(_storageRoot,"StagedSources");
            SourceWriteGuard.ThrowIfProtected(staging);Directory.CreateDirectory(staging);CheckStagingQuota(staging,token);
            string folder=System.IO.Path.Combine(staging,Guid.NewGuid().ToString("N")),output=System.IO.Path.Combine(folder,"selected.brres");Directory.CreateDirectory(folder);
            try
            {
                string originalHash,stagedHash;long length;
                using(var input=new FileStream(source,FileMode.Open,FileAccess.Read,FileShare.Read))
                {
                    var selected=BrawlContainerSelection.List(input,token).SingleOrDefault(c=>c.Path==containerPath)??throw new IOException("Explicit selected BRRES no longer exists.");
                    length=input.Length;ValidateResource(input,selected.Offset,selected.Length,0,token);input.Position=0;
                    using(var hash=SHA256.Create())originalHash=Hex(hash.ComputeHash(input));
                    SourceWriteGuard.ThrowIfProtected(output);
                    using(var copy=new FileStream(output,FileMode.CreateNew,FileAccess.Write,FileShare.None))
                    using(var hash=SHA256.Create())
                    {
                        SourceWriteGuard.ThrowIfProtected(copy);input.Position=selected.Offset;long left=selected.Length;var buffer=new byte[65536];
                        while(left>0){token.ThrowIfCancellationRequested();int n=input.Read(buffer,0,(int)Math.Min(left,buffer.Length));if(n==0)throw new IOException("Truncated selected BRRES.");copy.Write(buffer,0,n);hash.TransformBlock(buffer,0,n,buffer,0);left-=n;}
                        hash.TransformFinalBlock(new byte[0],0,0);stagedHash=Hex(hash.Hash);copy.Flush(true);
                    }
                    input.Position=0;using(var hash=SHA256.Create())if(Hex(hash.ComputeHash(input))!=originalHash)throw new IOException("Original changed during container selection.");
                }
                BrawlContainerAudit.Inspect(output,token).RequirePreservable();
                return new StagedSource{SourcePath=source,StagedPath=output,Sha256=originalHash,StagedHash=stagedHash,SelectedContainer=containerPath,Length=length};
            }
            catch{if(File.Exists(output))File.Delete(output);if(!Directory.EnumerateFileSystemEntries(folder).Any())Directory.Delete(folder);throw;}
        }
    }
}
