using System;
using System.Collections.ObjectModel;

namespace arte_7
{
    public class VideoList
    {
        private ObservableCollection<Video> _videoList = new ObservableCollection<Video>();
        private ObservableCollection<Video> _filteredVideoList = new ObservableCollection<Video>();
        private ObservableCollection<string> _filterDates = new ObservableCollection<string>();
        private bool _filterApplied = false;

        public ObservableCollection<Video> Videos
        {
            get { return _filterApplied ? _filteredVideoList : _videoList; }
            set { _videoList = value; }
        }

        public ObservableCollection<string> FilterDates
        {
            get { return _filterDates; }
            set { _filterDates = value; }
        }

        public int NumberOfVideoPages { get; set; }

        public int LoadedVideoPages { get; set; }

        public void Clear()
        {
            _videoList.Clear();
            _filteredVideoList.Clear();
            _filterDates.Clear();
            NumberOfVideoPages = 0;
            LoadedVideoPages = 0;
        }

        public void AddVideo(Video video)
        {
            _videoList.Add(video);

            UpdateDatesFilter(video);
        }

        public void ApplyFilter(string filterName)
        {
            _filterApplied = true;
            _filteredVideoList.Clear();

            string filterDateString = (filterName == "Heute" ? DateTime.Today.ToShortDateString() :
                                        filterName == "Gestern" ? DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)).ToShortDateString() :
                                        filterName);

            foreach (Video video in _videoList)
            {
                if (video.InternalDateTime.ToShortDateString() == filterDateString)
                {
                    _filteredVideoList.Add(video);
                }
            }
        }

        public void RemoveFilter()
        {
            _filterApplied = false;
        }

        private void UpdateDatesFilter(Video video)
        {
            if (0 == FilterDates.Count)
            {
                FilterDates.Add("Alle");
            }

            string date = null;

            if (video.InternalDateTime.Date == DateTime.Today)
            {
                date = "Heute";
            }
            else if (video.InternalDateTime.Date == DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)))
            {
                date = "Gestern";
            }
            else
            {
                date = video.InternalDateTime.ToShortDateString();
            }

            if (!FilterDates.Contains(date))
            {
                FilterDates.Add(date);
            }
        }
    }
}
