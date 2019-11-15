using System.Collections.ObjectModel;

namespace arte_7
{
    public class ViewModelSampleData : ViewModel
    {
        public ViewModelSampleData()
            : base()
        {
            base.VideoList.Videos = new ObservableCollection<Video>(){
                new Video(){
                    Title = "Video Title Text 1",
                    DisplayedAt = "12.01.2011",
                    Duration = "1:20",
                    TeaserText = "Das ist der Teaser",
                    Thumbnail = "../../media2.jpg"
                },
                new Video(){
                    Title = "Video Title Text 2",
                    DisplayedAt = "12.01.2011",
                    Duration = "1:20",
                    TeaserText = "Das ist der Teaser",
                    Thumbnail = "../../media2.jpg"
                },
                new Video(){
                    Title = "Video Title Text 3",
                    DisplayedAt = "12.01.2011",
                    Duration = "1:20",
                    TeaserText = "Das ist der Teaser",
                    Thumbnail = "../../media2.jpg"
                },
                new Video(){
                    Title = "Video Long long long long Title 1",
                    DisplayedAt = "Gestern",
                    Duration = "0:24",
                    TeaserText = "Das ist der Teaser",
                    Thumbnail = "media1.jpg"
                }
            };
        }
    }
}
