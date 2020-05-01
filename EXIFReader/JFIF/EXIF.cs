using System;
using System.Collections.Generic;
using System.Text;

using EXIFReader.UTIL;
namespace EXIFReader.JFIF
{
    public class TIFFDirectoryEntry
    {
        public UInt16 Tag;
        public UInt16 Type;
        public UInt32 Count;
        public UInt32 ValueOrOffset;
        public byte[] Bytes;
        public TIFFDirectoryEntry(byte[] bytes, int offset, int tiffoffset, bool tiffIsBigEndian)
        {
            BitUtils.GetValue(ref Tag, bytes, ref offset, tiffIsBigEndian);
            BitUtils.GetValue(ref Type, bytes, ref offset, tiffIsBigEndian);
            BitUtils.GetValue(ref Count, bytes, ref offset, tiffIsBigEndian);
            if(Type == 1)
            {
                ValueOrOffset = bytes[offset];
                offset += 4;
            }
            else if(Type == 3)
            {
                UInt16 val = 0;
                BitUtils.GetValue(ref val, bytes, ref offset, tiffIsBigEndian);
                ValueOrOffset = val;
                offset += 2;
            }
            else if(Type == 2)
            {
                Bytes = new byte[Count];
                BitUtils.GetValue(ref ValueOrOffset, bytes, ref offset, tiffIsBigEndian);
                Array.Copy(bytes, tiffoffset + ValueOrOffset, Bytes, 0, Count);
            }
            else
            {
                BitUtils.GetValue(ref ValueOrOffset, bytes, ref offset, tiffIsBigEndian);
            }

        }

        public const int Size = 12;

        public static TIFFDirectoryEntry Parse(byte[] bytes, ref int offset, int tiffoffset, bool tiffIsBigEndian)
        {
            var tde = new TIFFDirectoryEntry(bytes, offset, tiffoffset, tiffIsBigEndian);
            offset += Size;
            return tde;
        }
    }
    public class EXIF
    {
        private bool tiffIsBigEndian;
        public UInt16 Length;
        public string Identifier;
        public int TiffHeaderStart;
        public UInt16 TiffEndian;
        public UInt16 TiffID;
        public UInt32 IFD0Offset;
        public UInt16 DirectoryCount;
        public List<TIFFDirectoryEntry> Directories = new List<TIFFDirectoryEntry>();
        public Dictionary<TiffTag, TIFFDirectoryEntry> Tags = new Dictionary<TiffTag, TIFFDirectoryEntry>();
        public EXIF(byte[] bytes, int offset)
        {
            offset += 2; //Skip App0 marker;
            BitUtils.GetValue(ref Length, bytes, ref offset, true);
            BitUtils.GetValue(ref Identifier, bytes, ref offset, 6);//EXIF\0\0
            TiffHeaderStart = offset;
            BitUtils.GetValue(ref TiffEndian, bytes, ref offset, true);
            tiffIsBigEndian = (TiffEndian == 0x4D4D);
            BitUtils.GetValue(ref TiffID, bytes, ref offset, tiffIsBigEndian);
            BitUtils.GetValue(ref IFD0Offset, bytes, ref offset, tiffIsBigEndian);
            BitUtils.GetValue(ref DirectoryCount, bytes, ref offset, tiffIsBigEndian);
            for(int dIdx=0; dIdx<DirectoryCount; dIdx++)
            {
                var ifd = TIFFDirectoryEntry.Parse(bytes, ref offset, TiffHeaderStart, tiffIsBigEndian);
                try
                {
                    if (Enum.IsDefined(typeof(TiffTag), (Int32)ifd.Tag))
                    {

                        var tifftag = (TiffTag)ifd.Tag;
                        if(tifftag == TiffTag.ExifIFD)
                        {
                            GetExifIFDTags(Tags, bytes, TiffHeaderStart+(int)ifd.ValueOrOffset, tiffIsBigEndian);

                        }

                        Tags.Add((TiffTag)ifd.Tag, ifd);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Directories.Add(ifd);
            }
        }

        public static void GetExifIFDTags(Dictionary<TiffTag, TIFFDirectoryEntry> tags, byte[] bytes, int offset, bool swap)
        {
            UInt16 tagcount = 0;
            int exifstart = offset;
            BitUtils.GetValue(ref tagcount, bytes, ref offset, swap);
            for (int dIdx = 0; dIdx < tagcount; dIdx++)
            {
                var ifd = TIFFDirectoryEntry.Parse(bytes, ref offset, exifstart, swap);//False?
                try
                {
                    if (Enum.IsDefined(typeof(TiffTag), (Int32)ifd.Tag))
                    {
                        tags.Add((TiffTag)ifd.Tag, ifd);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        

        public static EXIF Parse(byte[] bytes, ref int offset)
        {
            var exif = new EXIF(bytes, offset);
            offset = offset + exif.Length + 2;

            return exif;
        }

        public static bool IsType(byte[] bytes, int offset)
        {
            return bytes[offset] == 0xff && bytes[offset + 1] == 0xe1;
        }

    }
}
