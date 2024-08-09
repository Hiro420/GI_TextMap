using System.Reflection;
using System.Text.Json.Serialization;
using GI_TextMap;

namespace GI_TextMap
{
    public class FileParser
    {
        public Dictionary<ulong,string> ParseTextmapFile(string filepath)
        {
            Dictionary<ulong, string> result = new Dictionary<ulong, string>();
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(fs))
            {
                while (fs.CanRead)
                {
                    try
                    {
                        reader.ReadByte();
                        List<bool> bitfield = reader.ReadExcelBitfield();
                        Textmap mapEntry = ReadEntry(bitfield, reader);
                        result[mapEntry.textMapId] = mapEntry.textMapContent;
                    } catch (Exception)
                    {
                        // The file ended
                        return result;
                    }
                }
            }
            return result;
        }

        internal Textmap ReadEntry(List<bool> bitfield, EndianBinaryReader reader)
        {
            Textmap textmap = new Textmap();
            int bitIndex = 0;

            foreach (var field in typeof(Textmap).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (bitIndex < bitfield.Count && bitfield[bitIndex])
                {
                    switch (field.FieldType)
                    {
                        case Type t when t == typeof(ulong):
                            field.SetValue(textmap, reader.ReadVarInt());
                            break;

                        case Type t when t == typeof(string):
                            field.SetValue(textmap, reader.ReadString());
                            break;

                        case Type t when t == typeof(byte):
                            field.SetValue(textmap, reader.ReadByte());
                            break;

                        default:
                            throw new InvalidOperationException($"Unsupported field type: {field.FieldType}");
                    }
                }
                bitIndex++;
            }
            // Console.WriteLine($"{textmap.textMapId.ToString()}, {textmap.textMapContent}");
            return textmap;
        }
    }

    public class Textmap
    {
        public ulong textMapId;
        public string textMapContent = "";
    }

}
