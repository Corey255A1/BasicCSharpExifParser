using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace EXIFReader.JFIF
{
    public enum APPDataType { Undefined, APP0, APP0Ex, APP1 };
    public class JFIFFile
    {
        public UInt16 SOI;
        public APP0Marker APP0;
        public EXIF Exif;


        public JFIFFile(byte[] bytes, int offset)
        {
            SOI = BitConverter.ToUInt16(bytes, offset); offset += 2;
            bool parse = true;
            while (parse)
            {
                switch(GetSectionType(bytes, offset))
                {
                    case APPDataType.APP0: APP0 = APP0Marker.Parse(bytes, ref offset); break;
                    case APPDataType.APP1: Exif = EXIF.Parse(bytes, ref offset); parse = false;  break;
                    default: parse = false; break;
                }
            }
            
            if(APP0 != null)
            {
                Console.WriteLine(APP0.Identifier);
            }
            Console.WriteLine(Encoding.ASCII.GetString(Exif.Tags[TiffTag.Make].Bytes));
            Console.WriteLine(Encoding.ASCII.GetString(Exif.Tags[TiffTag.Model].Bytes));
            Console.WriteLine(Exif.Tags[TiffTag.ISOSpeedRatings].ValueOrOffset);
            Console.WriteLine(Exif.Tags[TiffTag.Orientation].ValueOrOffset);
        }



        public static APPDataType GetSectionType(byte[] bytes, int offset)
        {
            if (APP0Marker.IsType(bytes, offset)) return APPDataType.APP0;
            if (EXIF.IsType(bytes, offset)) return APPDataType.APP1;

            return APPDataType.Undefined;
        }

        public static bool IsJFIF(byte[] bytes, int offset)
        {
            return bytes[offset] == 0xff && bytes[offset + 1] == 0xd8;
        }
        public static async Task<JFIFFile> Parse(string filepath)
        {
            byte[] bytes = await File.ReadAllBytesAsync(filepath);
            bool isjfif = IsJFIF(bytes, 0);
            Console.WriteLine(isjfif);
            
            if (isjfif)
            {
                return new JFIFFile(bytes, 0);
            }
            else
            {

                return null;
            }
        }
    }
}
