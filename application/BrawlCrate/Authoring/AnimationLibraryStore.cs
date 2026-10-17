// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlLib.Internal.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace BrawlCrate.Authoring
{
    public sealed class LibraryPin
    {
        public string Kind { get; set; }
        public string Id { get; set; }
        public int Revision { get; set; }
        public string MetadataHash { get; set; }
    }
    public sealed class LibraryRecord
    {
        public SourceCharacterProfile Character { get; set; }
        public AnimationAsset Animation { get; set; }
    }
    public sealed class LibraryInfo
    {
        public int Version { get; set; } = 1;
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public long ReservedBytes { get; set; }
        public long Generation { get; set; }
        public int Records { get; set; }
    }
    public sealed class LibraryPage
    {
        public List<AnimationAsset> Clips { get; set; } = new List<AnimationAsset>();
        public int NextOffset { get; set; }
        public bool HasMore { get; set; }
        public long Generation { get; set; }
    }
    // Immutable record files and content-addressed payloads. No native decode or blob
    // read occurs while paging the metadata index. Writers serialize through a file lock.
    public sealed class AnimationLibraryStore
    {
        public string Root { get; }
        public string Id {get {using(Lock())return Info().Id;}}
        public static string DefaultRoot
        {
            get {string configured=Environment.GetEnvironmentVariable("FIGHTERPORT_LIBRARY_ROOT");return SourceWriteGuard.NormalizePath(string.IsNullOrWhiteSpace(configured)?Path.Combine(FighterPortPaths.DataRoot,"AnimationLibrary"):configured);}
        }
        public AnimationLibraryStore(string root=null,bool requireExisting=false)
        {
            Root=SourceWriteGuard.NormalizePath(root??DefaultRoot);SourceWriteGuard.ThrowIfProtected(Root);SourceWriteGuard.EnsureNoReparsePoints(Root);
            if(requireExisting && !File.Exists(Path.Combine(Root,"library.fplibrary")))throw new IOException("Pinned shared library is missing; restore its original directory. No replacement was created.");
            Directory.CreateDirectory(Root);Directory.CreateDirectory(Path.Combine(Root,"blobs"));Directory.CreateDirectory(Path.Combine(Root,"characters"));Directory.CreateDirectory(Path.Combine(Root,"animations"));
            using(Lock()) {if(!File.Exists(Path.Combine(Root,"library.fplibrary"))) WriteAtomic(Path.Combine(Root,"library.fplibrary"),CharacterProject.Serialize(new LibraryInfo()));else Info();}
        }
        private FileStream Lock()
        {
            string path=Path.Combine(Root,"write.lock");var watch=Stopwatch.StartNew();
            while(true)
                try {SourceWriteGuard.ThrowIfProtected(path);var stream=new FileStream(path,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);try{SourceWriteGuard.ThrowIfProtected(stream);return stream;}catch{stream.Dispose();throw;}}
                catch(IOException) {if(watch.ElapsedMilliseconds>5000)throw new IOException("Shared library is busy; retry after the other writer finishes.");Thread.Sleep(25);}
        }
        private LibraryInfo Info()
        {
            var info=CharacterProject.Deserialize<LibraryInfo>(CharacterProject.ReadBounded(Path.Combine(Root,"library.fplibrary"),4096),4096);
            if(info.Version!=1 || !Guid.TryParseExact(info.Id,"N",out _) || info.ReservedBytes<0 || info.ReservedBytes>4L*1024*1024*1024 || info.Generation<0 || info.Records<0 || info.Records>100000)throw new IOException("Invalid shared-library manifest.");return info;
        }
        public bool ContainsBlob(string hash)
        {
            string path=BlobPath(hash);SourceWriteGuard.EnsureNoReparsePoints(path);return File.Exists(path) && new FileInfo(path).Length<=CharacterProject.MaxNativeBytes;
        }
        public byte[] ReadBlob(string hash)
        {
            byte[] bytes=CharacterProject.ReadBounded(BlobPath(hash));if(CharacterProject.Hash(bytes)!=hash)throw new IOException("Shared native payload changed: "+hash);return bytes;
        }
        public string PutBlob(byte[] bytes)
        {
            if(bytes.Length>CharacterProject.MaxNativeBytes)throw new IOException("Selected library payload exceeds the 32 MiB working-file budget.");
            string hash=CharacterProject.Hash(bytes);using(Lock())
            {
                string path=BlobPath(hash);if(File.Exists(path)){ReadBlob(hash);return hash;}
                var info=Info();if(info.ReservedBytes+bytes.Length>4L*1024*1024*1024)throw new IOException("Library disk budget reached (4 GiB); no pinned data is automatically deleted.");
                // Reserve first: a crash may overcount, never silently exceed the budget.
                info.ReservedBytes+=bytes.Length;info.Generation++;WriteAtomic(Path.Combine(Root,"library.fplibrary"),CharacterProject.Serialize(info));WriteImmutable(path,bytes);
            }
            return hash;
        }
        public LibraryPin Publish(SourceCharacterProfile source) => PublishRecord(new LibraryRecord {Character=source},"characters",source.Id,source.Revision);
        public LibraryPin Publish(AnimationAsset animation) => PublishRecord(new LibraryRecord {Animation=animation},"animations",animation.Id,animation.Revision);
        private LibraryPin PublishRecord(LibraryRecord record,string kind,string id,int revision)
        {
            byte[] data=CharacterProject.Serialize(record);if(data.Length>(kind=="animations"?128*1024:1024*1024))throw new IOException("Library record exceeds its bounded metadata budget (animation 128 KiB / character 1 MiB).");string hash=CharacterProject.Hash(data),path=RecordPath(kind,id,revision);
            using(Lock())
            {
                if(File.Exists(path)) {if(CharacterProject.Hash(CharacterProject.ReadBounded(path,1024*1024))!=hash)throw new IOException("Immutable library revision conflict; choose a new asset/revision.");}
                else
                {
                    var info=Info();if(info.Records>=100000 || info.ReservedBytes+data.Length>4L*1024*1024*1024)throw new IOException("Library metadata/disk budget reached (100,000 records / 4 GiB). Pinned data is retained.");
                    info.Records++;info.ReservedBytes+=data.Length;info.Generation++;WriteAtomic(Path.Combine(Root,"library.fplibrary"),CharacterProject.Serialize(info));WriteImmutable(path,data);
                }
            }
            return new LibraryPin {Kind=kind,Id=id,Revision=revision,MetadataHash=hash};
        }
        public LibraryRecord Read(LibraryPin pin)
        {
            byte[] bytes=CharacterProject.ReadBounded(RecordPath(pin.Kind,pin.Id,pin.Revision),1024*1024);
            if(CharacterProject.Hash(bytes)!=pin.MetadataHash)throw new IOException("Pinned shared-library record changed.");return CharacterProject.Deserialize<LibraryRecord>(bytes,1024*1024);
        }
        public LibraryPin Pin(string kind,string id,int revision=1)
        {
            byte[] bytes=CharacterProject.ReadBounded(RecordPath(kind,id,revision),1024*1024);return new LibraryPin {Kind=kind,Id=id,Revision=revision,MetadataHash=CharacterProject.Hash(bytes)};
        }
        public LibraryPage Page(int offset=0,int limit=32,string search="",CancellationToken token=default(CancellationToken))
        {
            if(offset!=0)throw new IOException("Use a resumable library cursor for later pages; offset rescans are not supported.");
            using(var cursor=OpenCursor(search))return cursor.Next(limit,token);
        }
        public LibraryCursor OpenCursor(string search="")=>new LibraryCursor(this,search);
        public sealed class LibraryCursor : IDisposable
        {
            private readonly AnimationLibraryStore _store;
            private readonly IEnumerator<string> _files;
            private readonly string _search;
            private readonly long _generation;
            private int _visited;
            private bool _done;
            internal LibraryCursor(AnimationLibraryStore store,string search)
            {
                if(search==null || search.Length>120)throw new IOException("Library search is limited to 120 characters.");
                _store=store;_search=search;
                using(store.Lock()){_generation=store.Info().Generation;_files=Directory.EnumerateFiles(Path.Combine(store.Root,"animations"),"*.fplibrecord").GetEnumerator();}
            }
            public LibraryPage Next(int limit=32,CancellationToken token=default(CancellationToken))
            {
                token.ThrowIfCancellationRequested();
                using(_store.Lock())return NextLocked(limit,token);
            }
            private LibraryPage NextLocked(int limit,CancellationToken token)
            {
                if(limit<1 || limit>64)throw new IOException("Library page budget: 1..64 records.");
                token.ThrowIfCancellationRequested();if(_store.Info().Generation!=_generation)throw new IOException("Library changed; refresh the cursor.");
                var page=new LibraryPage {Generation=_generation};int visited=0;
                while(!_done && visited<256 && page.Clips.Count<limit)
                {
                    token.ThrowIfCancellationRequested();if(!_files.MoveNext()){_done=true;break;}visited++;_visited++;
                    var record=CharacterProject.Deserialize<LibraryRecord>(CharacterProject.ReadBounded(_files.Current,128*1024),128*1024);
                    if(record.Animation==null)throw new IOException("Invalid animation index record.");
                    if(record.Animation.Name.IndexOf(_search,StringComparison.OrdinalIgnoreCase)>=0)page.Clips.Add(record.Animation);
                }
                if(_store.Info().Generation!=_generation)throw new IOException("Library changed during indexing; refresh the cursor.");
                page.NextOffset=_visited;page.HasMore=!_done;return page;
            }
            public void Dispose()=>_files.Dispose();
        }
        private string BlobPath(string hash)
        {
            if(hash==null || hash.Length!=64 || hash.Any(c=>!((c>='0'&&c<='9')||(c>='a'&&c<='f'))))throw new IOException("Invalid native content hash.");return Path.Combine(Root,"blobs",hash+".fpblob");
        }
        private string RecordPath(string kind,string id,int revision)
        {
            if((kind!="characters" && kind!="animations") || !Guid.TryParseExact(id,"N",out _) || revision<1 || revision>999999)throw new IOException("Invalid library revision identity.");
            return Path.Combine(Root,kind,id+"-"+revision.ToString("D6")+".fplibrecord");
        }
        private static void WriteImmutable(string path,byte[] data)
        {
            string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
            try {WriteNew(temp,data);SourceWriteGuard.ThrowIfProtected(path);File.Move(temp,path);File.SetAttributes(path,File.GetAttributes(path)|FileAttributes.ReadOnly);}finally{if(File.Exists(temp))File.Delete(temp);}
        }
        private static void WriteAtomic(string path,byte[] data)
        {
            string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
            try {WriteNew(temp,data);SourceWriteGuard.ThrowIfProtected(path);if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}finally{if(File.Exists(temp))File.Delete(temp);}
        }
        private static void WriteNew(string path,byte[] data)
        {
            SourceWriteGuard.ThrowIfProtected(path);using(var stream=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None)){SourceWriteGuard.ThrowIfProtected(stream);stream.Write(data,0,data.Length);stream.Flush(true);}
        }
    }
}
