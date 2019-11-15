using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace arte_7
{
    public class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Video> _videoList = new ObservableCollection<Video>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<VideoStreamerEventHandlerArgs> Error;

        public ObservableCollection<Video> VideoList
        {
            get { return _videoList; }
        }

        public string DestinationPath { get; set; }

        public void LoadViewList(Action finishedAction)
        {
            _videoList.Clear();

            ArteMediaStreamer.LoadVideoList(_videoList, (e) =>
                {
                    if (e != null)
                        Error(this, new VideoStreamerEventHandlerArgs(e));
                });
        }

        public void DownloadVideo(Video video)       
        {
            ArteMediaStreamer.FindEmbeddedStreamInfo(video, (s, e) =>
                {
                    if (null == e)
                    {
                        string dest = DestinationPath + "\\" + video.Title + ".flv";

                        ArteMediaStreamer.StartRtmpStream(s, dest);
                    }
                    else
                    {
                        Error(this, new VideoStreamerEventHandlerArgs(e));
                    }
                });
        }
    }

    public class Video : INotifyPropertyChanged
    {
        private string _title;
        public string Title 
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        private string _teaserText;
        public string TeaserText
        {
            get { return _teaserText; }
            set
            {
                if (_teaserText != value)
                {
                    _teaserText = value;
                    OnPropertyChanged("TeaserText");
                }
            }
        }

        public string Thumbnail { get; set; }

        public string URL { get; set; }

        public string Duration { get; set; }

        public string DisplayedAt { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string p)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }

    public class VideoStreamerEventHandlerArgs : EventArgs
    {
        public VideoStreamerEventHandlerArgs(Exception ex)
        {
            Error = ex;
        }

        public Exception Error { get; set; }
    }
}
