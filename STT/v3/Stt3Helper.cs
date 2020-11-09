using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace mp3WavConverter.stt.v3
{
    public class Stt3Helper
    {
        const int DEFAULT_CHANNEL = 0;

        public static void ParseContentV3(string outFolder, string[] jsonFiles)
        {
            foreach (var fileItem in jsonFiles)
            {
                var content = File.ReadAllText(fileItem);

                var result = JsonConvert.DeserializeObject<Stt3Result>(content);

                var transcriptObject = new Transcript();
                transcriptObject.full_text = result.combinedRecognizedPhrases.Where(x => x.channel == DEFAULT_CHANNEL).FirstOrDefault().display;

                // only maxest Confidence
                foreach (var segment in result.recognizedPhrases)
                {
                    segment.nBest = new Nbest[] { segment.nBest.OrderByDescending(x => x.confidence).First() };
                }

                var recognizedPhrases = result.recognizedPhrases.Where(x => x.channel == DEFAULT_CHANNEL).ToList();

                const string endSymbol = "。";
                Regex matchRegex = new Regex(@"\，|\。");

                VTT vttData = new VTT();

                foreach (var segment in recognizedPhrases)
                {
                    var nBest = segment.nBest.First();

                    // find symbol and previous word, combine it.
                    var matchs = matchRegex.Matches(nBest.display);
                    var currentIndex = 0;
                    var rawContent = "";

                    VTTItem tempVttItem = null;

                    foreach (var item in nBest.words)
                    {
                        // 1. init VTT item, and set start_at use millisecond unit.
                        if (tempVttItem == null)
                        {
                            tempVttItem = new VTTItem
                            {
                                StartAtMS = ConvertTickToMilliseconds(item.offsetInTicks)
                            };
                        }

                        currentIndex += item.word.Length;
                        rawContent += item.word;
                        var previousWord = matchs.Where(x => x.Index == currentIndex).FirstOrDefault();
                        if (previousWord != null)
                        {
                            item.word += previousWord.Value;
                            rawContent += previousWord.Value;
                            currentIndex += 1;
                            Debug.WriteLine($"item.word: {item.word}");
                            Debug.WriteLine($"rawContent: {rawContent}");

                            if (previousWord.Value == endSymbol)
                            {
                                // 2. when the symbol is end, must setting end_at, comments and adding to collection.
                                tempVttItem.EndAtMS = ConvertTickToMilliseconds(item.offsetInTicks + item.durationInTicks);
                                tempVttItem.Comments.Add(rawContent);
                                rawContent = string.Empty;
                                vttData.Cues.Add(tempVttItem);
                                tempVttItem = null;
                            }
                        }
                    }

                    // 3. when the VTT item is lasted, but end_at is empty, must feeding it.
                    if (tempVttItem != null && tempVttItem.EndAtMS == 0)
                    {
                        var last = nBest.words.Last();
                        tempVttItem.EndAtMS = ConvertTickToMilliseconds(last.offsetInTicks + last.durationInTicks);
                        tempVttItem.Comments.Add(rawContent);
                        rawContent = string.Empty;
                        vttData.Cues.Add(tempVttItem);
                        tempVttItem = null;
                    }

                    var sentence = new Sentence();
                    sentence.text = nBest.display;
                    sentence.start_at_ms = ConvertTickToMilliseconds(segment.offsetInTicks);
                    sentence.duration_ms = ConvertTickToMilliseconds(segment.durationInTicks);
                    sentence.words.AddRange(nBest.words.Select(x => new TextUnit
                    {
                        text = x.word,
                        start_at_ms = ConvertTickToMilliseconds(x.offsetInTicks),
                        duration_ms = ConvertTickToMilliseconds(x.durationInTicks)
                    }).ToList());

                    transcriptObject.sentences.Add(sentence);
                }

                var outputJson = JsonConvert.SerializeObject(transcriptObject);

                var fileName = Path.GetFileName(fileItem);
                var newFile = Path.Combine(outFolder, fileName);

                File.WriteAllText(newFile, outputJson);

                Console.WriteLine($"transcripted file: {fileName}");

                var vttFileName = $"{Path.GetFileNameWithoutExtension(fileItem)}.vtt";
                var vttNewFile = Path.Combine(outFolder, vttFileName);
                File.WriteAllText(vttNewFile, vttData.ToVTT());
                Console.WriteLine($"vtt file: {vttFileName}");

                var lrcFileName = $"{Path.GetFileNameWithoutExtension(fileItem)}.lrc";
                var lrcNewFile = Path.Combine(outFolder, lrcFileName);
                File.WriteAllText(lrcNewFile, vttData.ToLRC());
                Console.WriteLine($"lrc file: {lrcFileName}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tick">1 tick is 100 nanoseconds</param>
        static int ConvertTickToMilliseconds(float tick)
        {
            // 1 millisecond = 1000000 nanoseconds
            var milliseconds = tick * 100 / 1000000;
            return (int)milliseconds;
        }
    }
}