using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Language.V1;

namespace transcript_converter.stt.plugin
{
    public class GCPAnalyze
    {
        LanguageServiceClient client = LanguageServiceClient.Create();

        public void Anaylize(string rawContent, string fileItem, string outFolder)
        {
            Dictionary<string, int> score = new Dictionary<string, int>();

            var response = client.AnalyzeEntities(Document.FromPlainText(rawContent, "zh-Hant"));
            Debug.WriteLine($"Detected language: {response.Language}");
            Debug.WriteLine("Detected entities:");
            foreach (Entity entity in response.Entities)
            {
                var key = $"{entity.Name}_{entity.Type}";
                var exist = score.Keys.Where(x => x == key).FirstOrDefault();
                if (exist == null)
                {
                    score.Add(key, 1);
                }
                else
                {
                    score[key] += 1;
                }

                Debug.WriteLine($"  {entity.Name} (type: {entity.Type})({(int)(entity.Salience * 100)}%)");
            }

            StringBuilder builder = new StringBuilder();

            foreach (var item in score.OrderByDescending(x => x.Value))
            {
                string[] format = item.Key.Split('_');
                Debug.WriteLine($"{format[0]} , {format[1]}, count: {item.Value}");
                builder.AppendLine($"{format[0]} , {format[1]}, count: {item.Value}");
            }
            var scoreFile = $"{Path.GetFileNameWithoutExtension(fileItem)}.score";
            var newScoreFile = Path.Combine(outFolder, scoreFile);

            File.WriteAllText(newScoreFile, builder.ToString());
        }

        //static void InvokeGCPSTTService(string outFolder)
        //{
        //    var speech = SpeechClient.Create();
        //    var longOperation = speech.LongRunningRecognize(new LongRunningRecognizeRequest()
        //    {
        //        Config = new RecognitionConfig
        //        {
        //            Encoding = RecognitionConfig.Types.AudioEncoding.Flac,
        //            SampleRateHertz = 44100,
        //            AudioChannelCount = 2,
        //            LanguageCode = "zh-TW",
        //            EnableWordTimeOffsets = true
        //        },
        //        Audio = new RecognitionAudio
        //        {
        //            Uri = ""
        //        }
        //    });

        //    StringBuilder builder = new StringBuilder();

        //    longOperation = longOperation.PollUntilCompleted();
        //    var response = longOperation.Result;
        //    foreach (var result in response.Results)
        //    {
        //        foreach (var alternative in result.Alternatives)
        //        {
        //            Debug.WriteLine($"Transcript: { alternative.Transcript}");
        //            builder.AppendLine(alternative.Transcript);
        //        }
        //    }

        //    var testFileName = $"gcp.txt";
        //    var testFile = Path.Combine(outFolder, testFileName);
        //    File.WriteAllText(testFile, builder.ToString());
        //    Console.WriteLine($"test GCP file: {testFileName}");
        //}
    }
}