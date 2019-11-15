using System;
using System.Text;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Diagnostics;
using System.ComponentModel;
using System.Web;

namespace arte_7
{
    public class ArteVideoLoader
    {
        public static void LoadInitialVideoList(VideoList videoList, Action<Exception> finishedAction)
        {
            // Clear video list
            videoList.Clear();

            // Load first video page (page 1) asynchronously
            LoadVideoPageAsync(videoList, 1, finishedAction);
        }

        public static void LoadVideoList(VideoList videoList, int pageId, Action<Exception> finishedAction)
        {
            // Load video page asynchronously
            LoadVideoPageAsync(videoList, pageId, finishedAction );
        }

        public static void LoadNextVideoPage(VideoList videoList, Action<Exception> finishedAction)
        {
            if (videoList.LoadedVideoPages < videoList.NumberOfVideoPages)
            {
                LoadVideoPageAsync(videoList, ++videoList.LoadedVideoPages, finishedAction);
            }
        }

        public static void LoadAllVideoPages(VideoList videoList, Action<Exception> finishedAction)
        {
            while (videoList.LoadedVideoPages < videoList.NumberOfVideoPages)
            {
                LoadVideoPageAsync(videoList, ++videoList.LoadedVideoPages, finishedAction);
            }
        }

        private static void LoadVideoPageAsync(VideoList videoList, int pageId, Action<Exception> finishedAction)
        {
            if (Constants.IS_LOCAL)
            {
                using (Stream reader = new FileStream(string.Format(Constants.LOCAL_VIDEO_LOCATION, pageId), FileMode.Open))
                {
                    ProcessResponse(reader, videoList, finishedAction);
                }
            }
            else
            {
                WebClient webClient = new WebClient();

                webClient.OpenReadCompleted += (s, e) =>
                    {
                        if (null == e.Error)
                        {
                            ProcessResponse(e.Result, videoList, finishedAction);
                        }
                        else
                        {
                            finishedAction(e.Error);
                        }
                    };
                string pageUri = string.Format(Constants.URL_VIDEO_PAGE, pageId);
                webClient.OpenReadAsync(new Uri(pageUri, UriKind.Absolute));
            }
        }

        private static void LoadVideoPage(VideoList videoList, int pageId, Action<Exception> finishedAction)
        {
            try
            {
                WebClient webClient = new WebClient();
                using (Stream reader = webClient.OpenRead(new Uri(string.Format(Constants.URL_VIDEO_PAGE, pageId), UriKind.Absolute)))
                {
                    HtmlDocument document = new HtmlDocument();
                    document.Load(reader, Encoding.UTF8);
                    ProcessVideoList(document, videoList);
                }
            }
            catch (Exception ex) 
            { 
                finishedAction(ex);
            }
        }

        private static void ProcessResponse(Stream response, VideoList videoList, Action<Exception> finishedAction)
        {
            try
            {
                // Load HTML document
                HtmlDocument document = new HtmlDocument();
                document.Load(response, Encoding.UTF8);

                // Load videos from video page
                ProcessVideoList(document, videoList);

                // Get number of video pages
                if (0 == videoList.NumberOfVideoPages)
                {
                    videoList.NumberOfVideoPages = GetNumberOfPages(document);
                    videoList.LoadedVideoPages = 1;
                }

                // Everything OK, no exception
                finishedAction(null);
            }
            catch (Exception ex)
            {
                finishedAction(ex);
            }
        }

        private static int GetNumberOfPages(HtmlDocument document)
        {
            // find number of available pages
            HtmlNode pagination = document.DocumentNode.SelectSingleNode("//div[@class='pagination inside']");

            if (null != pagination)
            {
                HtmlNodeCollection pages = pagination.SelectNodes("ul/li");
                if (null != pages && pages.Count > 0)
                {
                    // Last page is the "Next" button
                    return pages.Count - 1;
                }
            }
            return 0;
        }

        private static void ProcessVideoList(HtmlDocument document, VideoList videoList)
        {
            int i = 0;

            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//div[@class='video']");
            foreach (HtmlNode node in nodes)
            {
                if (Constants.IS_LOCAL && i++ == 50) return; // Process only first 5 videos

                var video = ProcessVideoNode(node);

                if (null != video)
                {
                    videoList.AddVideo(video);
                }
            }
        }

        private static Video ProcessVideoNode(HtmlNode videoNode)
        {
            try
            {
                return VideoDecoder.Decode(videoNode);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static void FindEmbeddedStreamInfo(Video video, General.Action<StreamInfo, Exception> finishedAction)
        {
            if (string.IsNullOrEmpty(video.URL))
                return;

            try
            {
                WebClient webClient = new WebClient();

                webClient.OpenReadCompleted += (s, e) =>
                    {
                        if (null == e.Error)
                        {
                            ProcessVideoPage(e.Result, finishedAction);
                        }
                        else
                        {
                            finishedAction(null, e.Error);
                        }
                    };

                webClient.OpenReadAsync(new Uri(Constants.URL_VIDEO_MAIN + video.URL, UriKind.Absolute));
            }
            catch (Exception ex)
            {
                finishedAction(null, ex);
            }
        }

        private static void ProcessVideoPage(Stream stream, General.Action<StreamInfo, Exception> finishedAction)
        {
            HtmlDocument document = new HtmlDocument();
            document.Load(stream, Encoding.UTF8);

            HtmlNode videoNode = document.DocumentNode.SelectSingleNode("//div[@class='video-container']");
            if (null != videoNode)
            {
                string videoURL = videoNode.GetAttributeValue("arte_vp_url", "");
                if (videoURL.Length > 0)
                {
                    WebClient videoWebClient = new WebClient();
                    Stream s = videoWebClient.OpenRead(videoURL);
                    StreamReader rd = new StreamReader(s);                   
                    string str2 = rd.ReadToEnd();

                    const string VSR = "\"VSR\"";
                    const string STREAMER = "\"streamer\"";
                    const string URL = "\"url\"";

                    // Find MP4 Streaming URL
                    int startVSR = str2.IndexOf(VSR);
                    if (startVSR > 0)
                    {
                        string mp4StreamUrl = null;
                        startVSR += VSR.Length;

                        // Search list of available streams
                        while (startVSR > 0 && null == mp4StreamUrl)
                        {
                            // Search only inside of section
                            int sectionLenth = str2.IndexOf("},", startVSR) - startVSR;
                            if (sectionLenth > 0)
                            {
                                int startSTREAMER = str2.IndexOf(STREAMER, startVSR, sectionLenth);
                                if (startSTREAMER < 0)
                                {
                                    int startURL = str2.IndexOf(URL, startVSR, sectionLenth);
                                    if (startURL > 0)
                                    {
                                        startURL += URL.Length + 2;
                                        int endURL = str2.IndexOf('"', startURL);
                                        if (endURL > startURL)
                                        {
                                            mp4StreamUrl = str2.Substring(startURL, endURL - startURL);
                                        }
                                        else
                                        {
                                            // Error
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    // Continue search
                                    startVSR += sectionLenth + 1;
                                }
                            }
                            else
                            {
                                // End of section not found, abort
                                startVSR = -1;
                            }
                        }

                        if (null != mp4StreamUrl)
                        {
                            videoWebClient.DownloadFile(mp4StreamUrl, "./video.mp4");
                        }
                    }


                    //const string VIDEO_PLAYER_URL = "videoPlayerUrl";
                    //int startUrl = str2.IndexOf(VIDEO_PLAYER_URL);
                    //if (startUrl > 0)
                    //{
                    //    startUrl += VIDEO_PLAYER_URL.Length + 3;
                    //    int endUrl = str2.IndexOf('"', startUrl);
                    //    if (endUrl > 0 && endUrl > startUrl)
                    //    {
                    //        string videoUrl = str2.Substring(startUrl, endUrl - startUrl);
                    //        videoUrl = videoUrl.Replace("/player/", "/");

                    //        s = videoWebClient.OpenRead(videoUrl);
                    //        rd = new StreamReader(s);

                    //        str2 = rd.ReadToEnd();
                    //    }
                    //}
                }
            }

            //HtmlNode objectNode = document.DocumentNode.SelectSingleNode(string.Format("//object[@classid='{0}']", Constants.PLAYER_CLSID));
            //HtmlNode embedNode = objectNode.SelectSingleNode("./embed");

            //string playerUrl = 
            //    HttpUtility.UrlDecode(HtmlEntity.DeEntitize(embedNode.Attributes["src"].Value));

            //HtmlNode movieNode = objectNode.SelectSingleNode("./param[@name='movie']");
            //string movieParams = movieNode.Attributes["value"].Value;
            //movieParams = HtmlEntity.DeEntitize(movieParams);

            //string videoRefUrl = movieParams.Substring(movieParams.IndexOf("videorefFileUrl") + "videorefFileUrl".Length + 1);
            //videoRefUrl = HttpUtility.UrlDecode(videoRefUrl);

            //WebClient c = new WebClient();
            //c.OpenReadCompleted += (sender, e) =>
            //{
            //    HtmlDocument videoDocument = new HtmlDocument();
            //    videoDocument.Load(e.Result);
            //    HtmlNode videoNode = videoDocument.DocumentNode.SelectSingleNode("//video[@lang='de']");
            //    string videoUrl = videoNode.Attributes["ref"].Value;

            //    WebClient d = new WebClient();
            //    d.OpenReadCompleted += (sender2, e2) =>
            //    {
            //        videoDocument.Load(e2.Result);
            //        videoNode = videoDocument.DocumentNode.SelectSingleNode("//urls/url[@quality='hd']");

            //        finishedAction(
            //            new StreamInfo
            //            {
            //                StreamUrl = videoNode.InnerText,
            //                PlayerUrl = playerUrl
            //            },
            //        null);
            //    };

            //    d.OpenReadAsync(new Uri(videoUrl, UriKind.Absolute));
            //};

            //c.OpenReadAsync(new Uri(videoRefUrl, UriKind.Absolute));           
        }

        public static void StartRtmpStream(StreamInfo video, string destination)
        {
            Process rtmpProcess = new Process();

            //string arguments = string.Format("--rtmp {0} --swfVfy {1}", streamLocation, PlayerUrl);
            string arguments = string.Format("--rtmp {0} --flv {1} --swfVfy {2}", video.StreamUrl, "\"" + destination + "\"", video.PlayerUrl);

            if (File.Exists(destination))
                arguments += " --resume";

            try
            {
                // Get the path that stores user documents.
                rtmpProcess.StartInfo.FileName = @"..\..\..\rtmpdump\rtmpdump.exe";
                rtmpProcess.StartInfo.Arguments = arguments;
                rtmpProcess.StartInfo.CreateNoWindow = true;
                rtmpProcess.Start();
            }
            catch (Win32Exception)
            {
                if (File.Exists(destination))
                    File.Delete(destination);
            }
        }

        private static WebClient CreateWebClient()
        {
            WebClient webClient = new WebClient();
            webClient.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.0; de; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15 ( .NET CLR 3.5.30729; .NET4.0E)";
            return webClient;
        }
    }
}
