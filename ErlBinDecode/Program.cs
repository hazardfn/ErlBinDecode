using System;
using System.IO;
using System.Collections.Generic;

namespace ErlBinDecode
{
    class MainClass
    {
        const string fileName = "cursor.bin";
        const byte ERL_MAP = 116;
        const byte ERL_ATOM = 100;
        private static List<object> result = new List<object>();

        public static void Main(string[] args)
        {
            using (BigEndianBinaryReader reader = new BigEndianBinaryReader(File.Open(fileName, FileMode.Open)))
            {
                byte header = reader.ReadByte();

                if (header == 131)
                {
                    result.Add(ParseValue(reader));
                }
            }

            foreach (object item in result)
            {
                PrintToConsole(item);
            }
        }

        private static void PrintToConsole(object value)
        {
            switch (value)
            {
                case Dictionary<string, object> d:
                    foreach (KeyValuePair<string, object> entry in d)
                    {
                        PrintToConsole(entry.Key, entry.Value);
                    }
                    break;
                case List<object> l:
                    foreach (object entry in l)
                    {
                        Console.WriteLine(entry.ToString());
                    }
                    break;
                default:
                    Console.WriteLine(value.ToString());
                    break;
            }
        }

        private static void PrintToConsole(string key, object value)
        {
            Console.WriteLine($"{key}: ");
            PrintToConsole(value);
        }

        private static string DecodeAtom(BigEndianBinaryReader reader)
        {
            uint atom_length = reader.ReadUInt16();
            return new string(reader.ReadChars((int)atom_length));
        }

        private static object DecodeTuple(BigEndianBinaryReader reader)
        {
            List<object> list = new List<object>();

            byte arity = reader.ReadByte();

            for (int i = 0; i < arity; i++)
            {
                list.Add(ParseValue(reader));
            }

            return list;
        }

        private static object DecodeMap(BigEndianBinaryReader reader)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            uint arity = reader.ReadUInt32();
            string key = null;
            object value = null;

            for (int i = 0; i < arity; i++)
            {
                byte key_type = reader.ReadByte();

                if (key_type == ERL_ATOM)
                {
                    key = DecodeAtom(reader);
                }

                if (key != null)
                {
                    value = ParseValue(reader);
                }

                map.Add(key, value);
            }

            return map;
        }

        private static object ParseValue(BigEndianBinaryReader reader)
        {
            byte value_type = reader.ReadByte();
            object value = null;

            switch (value_type)
            {
                case 97:
                    value = reader.ReadByte();
                    break;
                case 98:
                    value = reader.ReadInt32();
                    break;
                case 100:
                    value = DecodeAtom(reader);
                    break;
                case 104:
                    value = DecodeTuple(reader);
                    break;
                case 116:
                    value = DecodeMap(reader);
                    break;
                default:
                    break;
            }

            return value;
        }
    }
}
