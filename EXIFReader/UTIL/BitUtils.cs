//WunderVision 2020
//Utilities to Parse, Swap and move the offset
using System;
using System.Text;

namespace EXIFReader.UTIL
{
    public class BitUtils
    {
        public static UInt16 Swap16(UInt16 val)
        {
            return (UInt16)(val << 8 | val >> 8);
        }
        public static UInt32 Swap32(UInt32 val)
        {
            return ((UInt32)Swap16((UInt16)(val & 0xFFFF))) << 16 | ((UInt32)Swap16((UInt16)((val>>16))));
        }
        public static Int32 Swap32(Int32 val)
        {
            return ((Int32)Swap16((UInt16)(val & 0xFFFF))) << 16 | ((Int32)Swap16((UInt16)((val >> 16))));
        }
        public static void GetValue(ref UInt16 val, byte[] bytes, ref int offset, bool swap = false)
        {
            val = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            if (swap) val = Swap16(val);
        }
        public static void GetValue(ref UInt32 val, byte[] bytes, ref int offset, bool swap = false)
        {
            val = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            if (swap) val = Swap32(val);
        }
        public static void GetValue(ref String val, byte[] bytes, ref int offset, int length)
        {
            val = Encoding.ASCII.GetString(bytes, offset, length); offset += length;
        }
    }
}
