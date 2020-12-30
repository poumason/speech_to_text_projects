using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace transcript_converter
{
    public class VTT
    {
        public List<VTTItem> Cues { get; set; }

        public VTT()
        {
            Cues = new List<VTTItem>();
        }

        public string ToVTT()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("WEBVTT");
            builder.AppendLine("");

            foreach (var item in Cues)
            {
                builder.Append(item.ToVTTFormat());
            }

            return builder.ToString();
        }

        public string ToLRC()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var item in Cues)
            {
                builder.Append(item.ToLRCFormat());
            }

            return builder.ToString();
        }

        public string ToLyrics()
        {
            var lyricsData = new LyricsData
            {
                composer = "test",
                lyricist = "test",
                nick = "test",
                creator = "creator",
                lyrics = new List<LyricsItem>()
            };

            foreach (var item in Cues)
            {
                lyricsData.lyrics.AddRange(item.ToLyricsItems());
            }

            var json = JsonConvert.SerializeObject(lyricsData);

            return json;
        }

        public string ToSimpleTranscript()
        {
            var simple = new SimpleTranscriptData();

            foreach (var item in Cues)
            {
                simple.transcripts.AddRange(item.ToSimpleTranscriptItem());
            }

            var json = JsonConvert.SerializeObject(simple);

            return json;
        }
    }

    public class VTTItem
    {
        const string FORMAT = @"hh\:mm\:ss\.fff";

        public double StartAtMS { get; set; }

        public double EndAtMS { get; set; }

        public List<string> Comments { get; set; }

        public VTTItem()
        {
            Comments = new List<string>();
            StartAtMS = EndAtMS = 0;
        }

        public string ToVTTFormat()
        {
            var startAt = TimeSpan.FromMilliseconds(StartAtMS).ToString(FORMAT);
            var endAt = TimeSpan.FromMilliseconds(EndAtMS).ToString(FORMAT);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"{startAt} --> {endAt}");
            /*
             A comment cannot contain the string "-->", the ampersand character (&), or the less-than sign (<). 
             If you wish to use such characters, you need to escape them using for example &amp; for ampersand and &lt; for less-than. 
             It is also recommended that you use the greater-than escape sequence (&gt;) instead of the greater-than character (>) to avoid confusion with tags.
            */
            Comments.ForEach(x => builder.AppendLine(x));
            builder.AppendLine("");

            return builder.ToString();
        }

        public string ToLRCFormat()
        {
            var startAt = TimeSpan.FromMilliseconds(StartAtMS).ToString(FORMAT);
            var endAt = TimeSpan.FromMilliseconds(EndAtMS).ToString(FORMAT);

            StringBuilder builder = new StringBuilder();
            Comments.ForEach(x => builder.AppendLine($"[{startAt} {endAt}]<type=0>{x}"));

            return builder.ToString();
        }

        public List<LyricsItem> ToLyricsItems()
        {
            List<LyricsItem> items = new List<LyricsItem>();
            var startAt = TimeSpan.FromMilliseconds(StartAtMS);
            var endAt = TimeSpan.FromMilliseconds(EndAtMS);

            Comments.ForEach(x =>
            {
                var lyrics = new LyricsItem
                {
                    content = x,
                    start_time = (int)startAt.TotalMilliseconds,
                    end_time = (int)endAt.TotalMilliseconds,
                };
                items.Add(lyrics);
            });

            return items;
        }

        public List<SimpleTranscriptItem> ToSimpleTranscriptItem()
        {
            List<SimpleTranscriptItem> items = new List<SimpleTranscriptItem>();
            var startAt = TimeSpan.FromMilliseconds(StartAtMS);
            var endAt = TimeSpan.FromMilliseconds(EndAtMS);

            Comments.ForEach(x =>
            {
                var simpleItem = new SimpleTranscriptItem
                {
                    content = x,
                    start_at_ms = (int)startAt.TotalMilliseconds,
                    end_at_ms = (int)endAt.TotalMilliseconds,
                };
                items.Add(simpleItem);
            });

            return items;
        }
    }

    public class LyricsData
    {
        public string composer { get; set; }

        public string creator { get; set; }

        public string lyricist { get; set; }

        public List<LyricsItem> lyrics { get; set; }

        public string nick { get; set; }
    }

    public class LyricsItem
    {
        public string content { get; set; }

        public int type { get; set; } = 0;

        public int start_time { get; set; }

        public int end_time { get; set; }

        public bool is_session_start { get; set; } = false;
    }

    public class SimpleTranscriptData
    {
        public List<SimpleTranscriptItem> transcripts { get; set; }

        public SimpleTranscriptData()
        {
            transcripts = new List<SimpleTranscriptItem>();
        }
    }

    public class SimpleTranscriptItem
    {
        public string content { get; set; }
        public int start_at_ms { get; set; }
        public int end_at_ms { get; set; }
    }
}