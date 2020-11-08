using System;
using System.Collections.Generic;
using System.Text;

namespace mp3WavConverter
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
                builder.AppendLine(item.ToVTTFormat());
            }

            return builder.ToString();
        }

        public string ToLRC()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var item in Cues)
            {
                builder.AppendLine(item.ToLRCFormat());
            }

            return builder.ToString();
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
    }
}
