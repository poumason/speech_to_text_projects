using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using transcript_converter.stt;
using transcript_converter.stt.v3;

namespace transcript_converter
{
    class Program
    {
        static void Main(string[] args)
        {
            var currentPath = Directory.GetCurrentDirectory();
            var inFolder = Path.Combine(currentPath, "source");
            var outFolder = Path.Combine(currentPath, "destination");

            ConvertJsonSTT(inFolder, outFolder);
            ConvertVTTTToSimpleJson(inFolder, outFolder);

            Console.ReadLine();
        }

        static void ConvertVTTTToSimpleJson(string inFolder, string outFolder)
        {
            var vttFiles = Directory.GetFiles(inFolder, "*.vtt");
            var vttParser = new VttParser();
            foreach (var filePath in vttFiles)
            {
                var stream = File.OpenRead(filePath);
                var subItems = vttParser.ParseStream(stream, Encoding.UTF8);
                var simpleItems = subItems.Select(x => x.ToSimpleTranscriptItem()).ToList();
                var simple = new SimpleTranscriptData();
                simple.transcripts.AddRange(simpleItems);
                var json = JsonConvert.SerializeObject(simple);
                var newFileName = $"{Path.GetFileNameWithoutExtension(filePath)}.trans";
                var newFile = Path.Combine(outFolder, newFileName);
                File.WriteAllText(newFile, json);
                Console.WriteLine($"simple transcript file: {newFileName}");
            }
        }

        static void ConvertJsonSTT(string inFolder, string outFolder)
        {
            var jsonFiles = Directory.GetFiles(inFolder, "*.json");
            Stt3Helper.ParseContentV3(outFolder, jsonFiles);
        }
    }
}
