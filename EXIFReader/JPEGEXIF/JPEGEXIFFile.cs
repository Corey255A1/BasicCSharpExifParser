//WunderVision 2020
//Parse the Header information of a JFIF File

using System;
using System.IO;
using System.Threading.Tasks;
using EXIFReader.UTIL;
namespace EXIFReader.JFIF
{
    public enum APPDataType { Undefined, APP0, APP0Ex, APP1 };
    public class JPEGEXIFFile
    {
        public UInt16 SOI;
        public APP0Marker APP0;
        public EXIF Exif;


        public JPEGEXIFFile(byte[] bytes, int offset)
        {
            BitUtils.GetValue(ref SOI, bytes, ref offset);
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
        }

        public static APPDataType GetSectionType(byte[] bytes, int offset)
        {
            if (APP0Marker.IsType(bytes, offset)) return APPDataType.APP0;
            if (EXIF.IsType(bytes, offset)) return APPDataType.APP1;

            return APPDataType.Undefined;
        }

        public static bool IsJPEGExif(byte[] bytes, int offset)
        {
            return bytes[offset] == 0xff && bytes[offset + 1] == 0xd8;
        }
        public static async Task<JPEGEXIFFile> Parse(string filepath)
        {
            try
            {
                byte[] bytes = await File.ReadAllBytesAsync(filepath);
                bool isjfif = IsJPEGExif(bytes, 0);

                if (isjfif)
                {
                    return new JPEGEXIFFile(bytes, 0);
                }
            }
            catch(FileNotFoundException)
            {
                Console.WriteLine("Could Not Find File");                
            }
            catch
            {
                Console.WriteLine("Failed To Parse the File");
            }
            return null;
        }
    }
}
