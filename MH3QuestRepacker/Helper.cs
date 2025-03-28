using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MH3QuestRepacker
{
    class Helper
    {
        public static string readNullterminated(BinaryReader reader)
        {
            var char_array = new List<byte>();
            string str = "";
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                byte[] char_bytes2 = char_array.ToArray();
                str = Encoding.UTF8.GetString(char_bytes2);
                return str;
            }
            byte b = reader.ReadByte();
            while ((b != 0x00) && (reader.BaseStream.Position != reader.BaseStream.Length))
            {
                char_array.Add(b);
                b = reader.ReadByte();
            }
            byte[] char_bytes = char_array.ToArray();
            str = Encoding.UTF8.GetString(char_bytes);
            return str;
        }

        public static byte[] readCharacterAmount(BinaryReader reader, UInt32 amount)
        {
            var char_array = new List<byte>();
            string str = "";
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                byte[] char_bytes2 = char_array.ToArray();
                str = Encoding.UTF8.GetString(char_bytes2);
                return char_bytes2;
            }
            UInt32 counter = 0;
            while ((counter < amount) && (reader.BaseStream.Position != reader.BaseStream.Length))
            {
                byte b = reader.ReadByte();
                if (b == 0x0A) // \n
                {
                    char_array.Add(0x3C);
                    char_array.Add(0x4C);
                    char_array.Add(0x49);
                    char_array.Add(0x4E);
                    char_array.Add(0x45);
                    char_array.Add(0x3E);
                }
                else if (b != 0x0D) // \r
                {
                    char_array.Add(b);
                }
                counter++;
            }
            byte[] char_bytes = char_array.ToArray();
            return char_bytes;
        }

        // from http://stackoverflow.com/a/3294698/5343630
        public static uint swapEndianness(uint x)
        {
            return ((x & 0x000000ff) << 24) +  // First byte
                   ((x & 0x0000ff00) << 8) +   // Second byte
                   ((x & 0x00ff0000) >> 8) +   // Third byte
                   ((x & 0xff000000) >> 24);   // Fourth byte
        }
    }
}
