using System;
using System.ComponentModel;

namespace arte_7
{
    public class Video
    {
        public string Title { get; set; }

        public string TeaserText { get; set; }

        public string Thumbnail { get; set; }

        public string URL { get; set; }

        public string Duration { get; set; }

        private string _displayedAt;
        public string DisplayedAt 
        {
            get { return _displayedAt; }
            set { _displayedAt = value; CalculateInternalDateTime(); }
        }

        public DateTime InternalDateTime { get; set; }

        public StreamInfo MediaStream { get; set; }


        private const string STR_HEUTE = "Heute";
        private const string STR_GESTERN = "Gestern";

        private void CalculateInternalDateTime()
        {
            if (DisplayedAt.StartsWith(STR_HEUTE))
            {
                string timeString = DisplayedAt.Substring(STR_HEUTE.Length + 1).Trim();
                if (timeString.Length > 0)
                {
                    DateTime time;
                    if (DateTime.TryParse(timeString, out time))
                    {
                        InternalDateTime = time;
                    }
                }
            }
            else if (DisplayedAt.StartsWith(STR_GESTERN))
            {
                string timeString = DisplayedAt.Substring(STR_GESTERN.Length + 1).Trim();
                if (timeString.Length > 0)
                {
                    DateTime time;
                    if (DateTime.TryParse(timeString, out time))
                    {
                        InternalDateTime = time.Subtract(new TimeSpan(1, 0, 0, 0));
                    }
                }
            }
            else
            {
                DateTime time;
                if (DateTime.TryParse(DisplayedAt, out time))
                {
                    InternalDateTime = time;
                }
            }
        }
    }
}
