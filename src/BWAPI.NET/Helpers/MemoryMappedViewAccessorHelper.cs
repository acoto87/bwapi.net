using System;
using System.IO.MemoryMappedFiles;

namespace BWAPI.NET
{
    public static class MemoryMappedViewAccessorHelper
    {
        public static string ReadString(this MemoryMappedViewAccessor accessor, int offset, int length)
        {
            var buf = new char[length];
            long pos = offset;
            for (var i = 0; i < length; i++)
            {
                var b = accessor.ReadByte(pos);
                if (b == 0)
                {
                    break;
                }

                buf[i] = (char)(b & 0xff);
                pos++;
            }
            return new string(buf, 0, (int)(pos - offset));
        }

        public static void Write(this MemoryMappedViewAccessor accessor, int offset, int length, string str)
        {
            long pos = offset;
            for (var i = 0; i < Math.Min(str.Length, length - 1); i++)
            {
                accessor.Write(pos, (byte)str[i]);
                pos++;
            }
            accessor.Write(pos, (byte)0);
        }
    }
}
