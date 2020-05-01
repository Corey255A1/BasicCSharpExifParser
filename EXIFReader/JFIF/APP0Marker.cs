using System;
using System.Collections.Generic;
using System.Text;

using EXIFReader.UTIL;
namespace EXIFReader.JFIF
{
    public class APP0Marker
    {

        public UInt16 Length;
        public string Identifier;
        public UInt16 JFIFVersion;
        public byte DensityUnits;
        public UInt16 XDensity;
        public UInt16 YDensity;
        public byte XThumbnail;
        public byte YThumbnail;
        public byte[] ThumbnailData;

        public APP0Marker(byte[] bytes, int offset)
        {
            offset += 2; //Skip App0 marker;
            BitUtils.GetValue(ref Length, bytes, ref offset, true);
            BitUtils.GetValue(ref Identifier, bytes, ref offset, 5);
            BitUtils.GetValue(ref JFIFVersion, bytes, ref offset, true);
            DensityUnits = bytes[offset]; offset += 1;
            BitUtils.GetValue(ref XDensity, bytes, ref offset, true);
            BitUtils.GetValue(ref YDensity, bytes, ref offset, true);
            XThumbnail = bytes[offset]; offset += 1;
            YThumbnail = bytes[offset]; offset += 1;
            if (XThumbnail != 0 && YThumbnail != 0) {
                ThumbnailData = new byte[3 * YThumbnail * XThumbnail];
                Array.Copy(bytes, offset, ThumbnailData, 0, ThumbnailData.Length);
            }
        }

        public static bool IsType(byte[] bytes, int offset)
        {
            return bytes[offset] == 0xff && bytes[offset + 1] == 0xe0;
        }
        public static APP0Marker Parse(byte[] bytes, ref int offset)
        {
            var app0 = new APP0Marker(bytes, offset);
            offset += app0.Length + 2;

            return app0;
        }
    }
}
