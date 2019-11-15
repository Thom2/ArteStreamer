using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Collections;
using System.Windows;

namespace arte_7
{
    public class ViewModel : INotifyPropertyChanged
    {
        private VideoList _videoList = new VideoList();

        public event EventHandler<VideoStreamerEventHandlerArgs> Error;

        public VideoList VideoList
        {
            get { return _videoList; }
        }

        public ObservableCollection<Video> Videos
        {
            get { return _videoList.Videos; }
        }

        public ObservableCollection<string> FilterDates
        {
            get { return _videoList.FilterDates; }
        }

        public string DestinationPath { get; set; }


        public void LoadInitialVideoList(General.Action finishedAction)
        {
            ArteVideoLoader.LoadInitialVideoList(_videoList, (e) =>
            {
                if (e != null)
                    Error(this, new VideoStreamerEventHandlerArgs(e));
                else
                    finishedAction();
            });
        }

        public void LoadAllVideoPages(General.Action finishedAction)
        {
            ArteVideoLoader.LoadAllVideoPages(_videoList, (e) =>
            {
                if (e != null)
                    Error(this, new VideoStreamerEventHandlerArgs(e));
                else
                    finishedAction();
            });
        }

        public void LoadNextVideoPage(General.Action finishedAction)
        {
            ArteVideoLoader.LoadNextVideoPage(_videoList, (e) =>
            {
                if (e != null)
                    Error(this, new VideoStreamerEventHandlerArgs(e));
                else
                    finishedAction();
            });
        }

        private void LoadVideoList(General.Action finishedAction, int pageId)
        {
            ArteVideoLoader.LoadVideoList(_videoList, pageId, (e) =>
            {
                if (e != null)
                    Error(this, new VideoStreamerEventHandlerArgs(e));
                else
                    finishedAction();
            });
        }

        public void GetStreamInfo(Video video, Action<Exception> finishedAction)
        {
            ArteVideoLoader.FindEmbeddedStreamInfo(video, (s, e) =>
                {
                    if (null == e)
                    {
                        video.MediaStream = s;
                        finishedAction(null);
                    }
                    else
                    {
                        Error(this, new VideoStreamerEventHandlerArgs(e));
                    }
                });
        }

        public void DownloadVideo(Video video)
        {
            if (null == video.MediaStream)
            {
                GetStreamInfo(video, (e) =>
                {
                    if (null != video.MediaStream)
                    {
                        DownloadVideo(video);
                    }
                });
            }
            else
            {
                string dest = DestinationPath + "\\" + video.Title + DateTime.Now.ToString("_yyMMdd_HHmmss") + ".flv";

                if (File.Exists(dest))
                {
                    if (MessageBox.Show("File already exist. Do you want to replace existing file?", "File Exist", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        return;

                    File.Delete(dest);
                }

                ArteVideoLoader.StartRtmpStream(video.MediaStream, dest);
            }
        }

        internal void FilterVideoList(string date)
        {
            if (date == "Alle")
            {
                _videoList.RemoveFilter();
            }
            else
            {
                _videoList.ApplyFilter(date);
            }

            OnPropertyChanged("Videos");
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string p)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }

 
    public class StreamInfo
    {
        public string StreamUrl { get; set; }
        public string PlayerUrl { get; set; }
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
