using System;
using System.Drawing;
using System.Drawing.Imaging;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Textures;

internal static partial class ResourceOperations
{
    private static void CreateCi4Fixture(BRRESNode bres)
    {
        using(var image=new Bitmap(32,16,PixelFormat.Format4bppIndexed))
        {
            var colors=image.Palette;colors.Entries[0]=Color.Red;colors.Entries[1]=Color.Lime;colors.Entries[2]=Color.Blue;colors.Entries[3]=Color.FromArgb(109,255,0,255);colors.Entries[4]=Color.FromArgb(0,0,0,0);image.Palette=colors;
            var bits=image.LockBits(new Rectangle(0,0,32,16),ImageLockMode.WriteOnly,PixelFormat.Format4bppIndexed);
            try
            {
                for(int y=0;y<16;y++)
                {
                    var row=new byte[bits.Stride];for(int x=0;x<32;x+=2){int left=(x<16?0:1)+(y<8?0:2),right=left;if(x>=28&&y>=12)left=right=4;row[x/2]=(byte)((left<<4)|right);}System.Runtime.InteropServices.Marshal.Copy(row,0,IntPtr.Add(bits.Scan0,y*bits.Stride),row.Length);
                }
            }
            finally{image.UnlockBits(bits);}
            foreach(string name in new[]{"Indexed4Alpha","Indexed4Ambiguous","Indexed4Missing"})
            {
                BrawlLib.Internal.IO.FileMap palette;var tex=bres.CreateResource<TEX0Node>(name);tex.ReplaceRaw(TextureConverter.CI4.EncodeTEX0TextureIndexed(image,2,WiiPaletteFormat.RGB5A3,out palette));tex.Name=name;
                if(name=="Indexed4Missing")palette.Dispose();else {var pal=bres.CreateResource<PLT0Node>(name);pal.ReplaceRaw(palette);pal.Name=name;}
            }
            // Two unique native dictionaries, each containing one same-name palette. No duplicate
            // dictionary keys or patched bytes are needed to exercise ambiguity.
            colors.Entries[0]=Color.Yellow;image.Palette=colors;
            var bank=new BRESGroupNode("AlternatePaletteBank",BRESGroupNode.BRESGroupType.Palettes);bres.AddChild(bank);
            var alternate=new PLT0Node();bank.AddChild(alternate);alternate.ReplaceRaw(TextureConverter.EncodePLT0Palette(image.Palette,WiiPaletteFormat.RGB5A3));alternate.Name="Indexed4Ambiguous";
        }
    }
}
