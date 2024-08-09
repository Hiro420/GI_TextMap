using Newtonsoft.Json;
using GI_TextMap;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> target, IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        foreach (var element in source)
            target.Add(element.Key, element.Value);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 2 || !Directory.Exists(args[0]))
        {
            throw new ArgumentException("Please enter a valid location of a folder containing the raw binary files and the name of the output JSON file");
        }

        Dictionary<ulong, string> finalTextMap = new Dictionary<ulong, string>();
        string outputDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "output");
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string outputFilePath = Path.Combine(outputDirectory, args[1]);
        foreach (var file in Directory.GetFiles(args[0]))
        {
            FileParser parser = new FileParser();
            Dictionary<ulong, string> entries = parser.ParseTextmapFile(file);
            finalTextMap.AddRange(entries);
        }

        using (StreamWriter outputFile = new StreamWriter(outputFilePath))
        {
            outputFile.Write(DataToJson(finalTextMap));
        }

        Console.WriteLine("Done!");
    }

    private static string DataToJson<T>(T data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        return JsonConvert.SerializeObject(data, settings);
    }
}
