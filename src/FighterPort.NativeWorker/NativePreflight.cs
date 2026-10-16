using System;
using System.IO;
using System.Text;

// Bounded common header/resource dictionary checks derived from pinned BrawlLib
// SSBB/Types/Common.cs, MDL0.cs, Animations/CHR0.cs. Not a complete parser validator.
internal static class NativePreflight
{
    internal static void Check(string path,string magic,int version)
    {
        byte[] data=File.ReadAllBytes(path);
        void Need(bool condition,string why){if(!condition)throw new InvalidDataException("Native preflight: "+why);}
        Need(data.Length>=64&&data.Length<=8*1024*1024,"header/size quota");
        int Read(int offset){Need(offset>=0&&offset<=data.Length-4,"integer bounds");return (data[offset]<<24)|(data[offset+1]<<16)|(data[offset+2]<<8)|data[offset+3];}
        int U16(int offset)=>data[offset]*256+data[offset+1];
        int actualVersion=Read(8);
        Need(Encoding.ASCII.GetString(data,0,4)==magic&&(actualVersion==version||(magic=="MDL0"&&version==-1&&(actualVersion==9||actualVersion==11))),"magic/version");
        int size=Read(4);Need(size>=64&&size<=data.Length,"declared size");
        void StringAt(long offset)
        {
            Need(offset>0&&offset<data.Length,"string offset");
            int p=(int)offset,end=Math.Min(data.Length,p+4096);while(p<end&&data[p]!=0)p++;
            Need(p<end,"unterminated/oversize string");
        }
        void Group(int group)
        {
            Need(group>=0&&group<=size-24,"group offset");
            int count=Read(group+4),length=Read(group);
            Need(count>=0&&count<=4096&&length>=24L+16L*count&&(long)group+length<=size,"group count/length");
            for(int i=0;i<count;i++)
            {
                int e=group+24+i*16;
                Need(U16(e+4)<=count&&U16(e+6)<=count,"dictionary child index");
                StringAt((long)group+Read(e+8));
                long payload=(long)group+Read(e+12);Need(payload>=0&&payload<=size-8,"entry payload offset");
            }
        }
        if(magic=="CHR0")
        {
            Group(Read(16));StringAt(Read(20));if(Read(24)!=0)StringAt(Read(24));
            Need(U16(28)>0&&U16(30)==Read(Read(16)+4),"frame/entry count");Need(Read(32)==0||Read(32)==1,"loop flag");
        }
        else
        {
            Need(size>=0x80,"MDL0 header");
            int end=actualVersion==9?0x3c:0x44;
            for(int offset=0x10;offset<end;offset+=4)if(Read(offset)!=0)Group(Read(offset));
            StringAt(Read(actualVersion==9?0x3c:0x48));
            if(actualVersion==11){int user=Read(0x44);Need(user==0||(user>0&&user<size),"user data offset");}
        }
    }
}
