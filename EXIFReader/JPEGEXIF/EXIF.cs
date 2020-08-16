//WunderVision 2020
// Parsing the Exif Data portion of a JPEG File.

using System;
using System.Collections.Generic;

using EXIFReader.UTIL;
namespace EXIFReader.JFIF
{
    public enum IFDType
    {
        BYTE = 1, // An 8-bit unsigned integer
        ASCII = 2, // An 8-bit byte containing one 7-bit ASCII code. The final byte is terminated with NULL
        SHORT = 3, // A 16-bit (2-byte) unsigned integer
        LONG = 4, // A 32-bit (4-byte) unsigned integer
        RATIONAL = 5, // Two LONGs. The first LONG is the numerator and the second LONG expresses the denominator
        UNDEFINED = 7, // An 8-bit byte that can take any value depending on the field definition
        SLONG = 9, // A 32-bit (4-byte) signed integer (2's complement notation)
        SRATIONAL = 10 // Two SLONGs. The first SLONG is the numerator and the second SLONG is the denominator
    }

    public class ImageFileDirectoryEntry
    {
        public UInt16 Tag;
        public UInt16 Type;
        public UInt32 Count;
        public UInt32 ValueOrOffset;
        public byte[] Bytes;
        private string valstr = null;
        public string ValueString
        {
            get
            {
                if (valstr == null)
                {
                    if(Bytes != null)
                    {
                        valstr = ((IFDType)Type) switch
                        {
                            IFDType.RATIONAL => ((double)BitUtils.Swap32(BitConverter.ToUInt32(Bytes, 0)) / (double)BitUtils.Swap32(BitConverter.ToUInt32(Bytes, 4))).ToString(),
                            IFDType.SRATIONAL => ((double)BitUtils.Swap32(BitConverter.ToInt32(Bytes, 0)) / (double)BitUtils.Swap32(BitConverter.ToInt32(Bytes, 4))).ToString(),
                            _ => System.Text.Encoding.ASCII.GetString(Bytes)
                        };
                    }
                    else
                    {
                        valstr = ValueOrOffset.ToString();
                    }                    
                }
                return valstr;
            }
        }
        public ImageFileDirectoryEntry(byte[] bytes, int offset, int tiffoffset, bool tiffIsBigEndian)
        {
            BitUtils.GetValue(ref Tag, bytes, ref offset, tiffIsBigEndian);
            BitUtils.GetValue(ref Type, bytes, ref offset, tiffIsBigEndian);
            BitUtils.GetValue(ref Count, bytes, ref offset, tiffIsBigEndian);
            switch ((IFDType)Type)
            {
                case IFDType.UNDEFINED:
                case IFDType.BYTE: 
                    { 
                        ValueOrOffset = bytes[offset]; 
                        offset += 4; 
                    } 
                    break;
                case IFDType.ASCII:
                    {
                        Bytes = new byte[Count];
                        BitUtils.GetValue(ref ValueOrOffset, bytes, ref offset, tiffIsBigEndian);
                        if (Count > 2)
                        {
                            Array.Copy(bytes, tiffoffset + ValueOrOffset, Bytes, 0, Count);
                        }
                        else
                        {
                            ValueOrOffset = ((ValueOrOffset >> 24) & 0xFF);
                            Bytes[0] = (byte)ValueOrOffset;
                        }
                    }
                    break;
                case IFDType.SHORT:
                    {
                        UInt16 val = 0;
                        BitUtils.GetValue(ref val, bytes, ref offset, tiffIsBigEndian);
                        ValueOrOffset = val;
                        offset += 2;
                    }
                    break;
                case IFDType.RATIONAL:
                case IFDType.SRATIONAL:
                    {
                        Bytes = new byte[8];
                        BitUtils.GetValue(ref ValueOrOffset, bytes, ref offset, tiffIsBigEndian);
                        Array.Copy(bytes, tiffoffset + ValueOrOffset, Bytes, 0, 8);

                    }
                    break;
                default: 
                    { 
                        BitUtils.GetValue(ref ValueOrOffset, bytes, ref offset, tiffIsBigEndian); 
                    } 
                    break;
            }

        }

        public const int Size = 12;

        public static ImageFileDirectoryEntry Parse(byte[] bytes, ref int offset, int tiffoffset, bool tiffIsBigEndian)
        {
            var tde = new ImageFileDirectoryEntry(bytes, offset, tiffoffset, tiffIsBigEndian);
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
        public Dictionary<IFDTag, ImageFileDirectoryEntry> Tags = new Dictionary<IFDTag, ImageFileDirectoryEntry>();
        public EXIF(byte[] bytes, int offset)
        {
            offset += 2; //Skip App1 marker;
            BitUtils.GetValue(ref Length, bytes, ref offset, true);
            BitUtils.GetValue(ref Identifier, bytes, ref offset, 6);//EXIF\0\0
            TiffHeaderStart = offset;
            BitUtils.GetValue(ref TiffEndian, bytes, ref offset, true);
            tiffIsBigEndian = (TiffEndian == 0x4D4D);
            BitUtils.GetValue(ref TiffID, bytes, ref offset, tiffIsBigEndian);
            BitUtils.GetValue(ref IFD0Offset, bytes, ref offset, tiffIsBigEndian);
            int bytecount = GetIFDTags(Tags, bytes, offset, TiffHeaderStart, tiffIsBigEndian);
            offset += bytecount;
            if (Tags.ContainsKey(IFDTag.ExifIFD))
            {
                int sectionOffset = TiffHeaderStart + (int)Tags[IFDTag.ExifIFD].ValueOrOffset;
                GetIFDTags(Tags, bytes, sectionOffset, TiffHeaderStart, tiffIsBigEndian);
            }            
        }

        public string GetTagString(IFDTag tag)
        {
            if (Tags.ContainsKey(tag))
            {
                return Tags[tag].ValueString;
            }
            return "";
        }

        public static int GetIFDTags(Dictionary<IFDTag, ImageFileDirectoryEntry> tags, byte[] bytes, int offset, int sectionstart, bool swap)
        {
            UInt16 tagcount = 0;
            int start = offset;
            BitUtils.GetValue(ref tagcount, bytes, ref offset, swap);
            for (int dIdx = 0; dIdx < tagcount; dIdx++)
            {
                var ifd = ImageFileDirectoryEntry.Parse(bytes, ref offset, sectionstart, swap);//False?
                try
                {
                    if (Enum.IsDefined(typeof(IFDTag), (Int32)ifd.Tag))
                    {
                        tags.Add((IFDTag)ifd.Tag, ifd);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return (offset - start);
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
