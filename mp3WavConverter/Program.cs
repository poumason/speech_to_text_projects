using mp3WavConverter.Parser;
using mp3WavConverter.stt.v3;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace mp3WavConverter
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
            //ConvertMp3ToWav();
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

        static void ConvertMp3ToWav()
        {
            var currentPath = Directory.GetCurrentDirectory();
            var inFolder = Path.Combine(currentPath, "in");
            var outFolder = Path.Combine(currentPath, "out");

            ClearFiles(outFolder);

            ConvertFiles(inFolder, outFolder);

            Console.WriteLine("finished");
        }

        static void ConvertFiles(String sourcePath, String destPath)
        {
            var mp3Files = Directory.GetFiles(sourcePath, "*.mp3");

            if (mp3Files.Length == 0)
            {
                Console.WriteLine("not find any mp3 files");
                return;
            }


            foreach (var inFile in mp3Files)
            {
                if (!File.Exists(inFile))
                {
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(inFile);
                var outFile = Path.Combine(destPath, $"{name}.wav");

                using (var reader = new Mp3FileReader(inFile))
                {
                    WaveFileWriter.CreateWaveFile(outFile, reader);
                }

                Console.WriteLine($"converted: {outFile}");
            }
        }

        static void ClearFiles(String path)
        {
            var files = Directory.GetFiles(path);

            if (files.Length == 0)
            {
                return;
            }

            foreach (var filePath in files)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"delete: {filePath}");
                }
            }
        }
    }
}
