using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;

#if SILVERLIGHT
using utils=System.Windows.Browser;
#endif

namespace arte_7
{
    public class ArteMediaStreamer
    {
        private const string UrlArte = "http://videos.arte.tv";
        private const string UrlArteAllVideos = "http://videos.arte.tv/de/videos/alleVideos";
        private const string UrlLocalAllVideos = "../../../Test/alleVideos.htm";
        private const string PlayerCLSID = "clsid:d27cdb6e-ae6d-11cf-96b8-444553540000";

        public static void LoadVideoList(ObservableCollection<Video> videoList, Action<Exception> finishedAction)
        {
            if (false)
            {
                Stream reader = new FileStream(UrlLocalAllVideos, FileMode.Open);
                ProcessResponse(reader, videoList, finishedAction);
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

                webClient.OpenReadAsync(new Uri(UrlArteAllVideos, UriKind.Absolute));
            }
        }

        private static void ProcessResponse(Stream response, ObservableCollection<Video> videoList, Action<Exception> finishedAction)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.Load(response, Encoding.UTF8);
                ProcessVideoList(document, videoList);

                finishedAction(null);
            }
            catch (Exception ex)
            {
                finishedAction(ex);
            }
        }

        private static void ProcessVideoList(HtmlDocument document, ObservableCollection<Video> videoList)
        {
            int i = 0;
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//div[@class='video']");
            foreach (HtmlNode node in nodes)
            {
                if (i++ == 5) return;

                var video = ProcessVideoNode(node);

                if (null != video)
                    videoList.Add(video);
            }
        }

        private static Video ProcessVideoNode(HtmlNode videoNode)
        {
            try
            {
                var video = new Video();

                HtmlNode titleNode = videoNode.SelectSingleNode("./h2/a");
                video.Title = HtmlEntity.DeEntitize(titleNode.InnerText);
                video.URL = GetNodeAttribute(titleNode, "href");
                video.TeaserText = HtmlEntity.DeEntitize(videoNode.SelectSingleNode("./div/div/p[@class='teaserText']").InnerText);
                video.Thumbnail = UrlArte + videoNode.SelectSingleNode("./div/a/img").Attributes["src"].Value;
                video.Duration = videoNode.SelectSingleNode("./div/div[@class='duration_thumbnail']").InnerText;
                video.DisplayedAt = videoNode.SelectSingleNode("./p").InnerText;

                return video;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetNodeAttribute(HtmlNode node, string attributeName)
        {
            if (null != node)
            {
                if (node.Attributes.Contains(attributeName))
                    return node.Attributes[attributeName].Value;
            }
            return string.Empty;
        }

        public static void FindEmbeddedStreamInfo(Video video, Action<EmbedStream, Exception> finishedAction)
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

                webClient.OpenReadAsync(new Uri(UrlArte + video.URL, UriKind.Absolute));
            }
            catch (Exception ex)
            {
                finishedAction(null, ex);
            }
        }

        private static void ProcessVideoPage(Stream stream, Action<EmbedStream, Exception> finishedAction)
        {
            HtmlDocument document = new HtmlDocument();
            document.Load(stream, Encoding.UTF8);

            HtmlNode objectNode = document.DocumentNode.SelectSingleNode(string.Format("//object[@classid='{0}']", PlayerCLSID));
            HtmlNode embedNode = objectNode.SelectSingleNode("./embed");

            string playerUrl = 
                utils.HttpUtility.UrlDecode(HtmlEntity.DeEntitize(embedNode.Attributes["src"].Value));

            HtmlNode movieNode = objectNode.SelectSingleNode("./param[@name='movie']");
            string movieParams = movieNode.Attributes["value"].Value;
            movieParams = HtmlEntity.DeEntitize(movieParams);

            string videoUrl = movieParams.Substring(movieParams.IndexOf("videorefFileUrl") + "videorefFileUrl".Length + 1);
            videoUrl = utils.HttpUtility.UrlDecode(videoUrl);

            WebClient c = new WebClient();
            c.OpenReadCompleted += (sender, e) =>
            {
                HtmlDocument videoDocument = new HtmlDocument();
                videoDocument.Load(e.Result);
                HtmlNode videoNode = videoDocument.DocumentNode.SelectSingleNode("//video[@lang='de']");
                videoUrl = videoNode.Attributes["ref"].Value;

                c.OpenReadCompleted += (sender2, e2) =>
                    {
                        videoDocument.Load(e2.Result);
                        videoNode = videoDocument.DocumentNode.SelectSingleNode("//urls/url[@quality='hd']");

                        finishedAction(
                            new EmbedStream
                            {
                                StreamUrl = videoNode.InnerText,
                                PlayerUrl = playerUrl
                            }, 
                        null);
                    };

                c.OpenReadAsync(new Uri(videoUrl, UriKind.Absolute));
            };

            c.OpenReadAsync(new Uri(videoUrl, UriKind.Absolute));
        }

        public static void StartRtmpStream(EmbedStream video, string destination)
        {
#if SILVERLIGHT
            dynamic cmd = System.Runtime.InteropServices.Automation.AutomationFactory.CreateObject("WScript.Shell");
            cmd.Run(@"..\..\..\rtmpdump\rtmpdump.exe", 1, true);
#else
            Process rtmpProcess = new Process();

            //string arguments = string.Format("--rtmp {0} --swfVfy {1}", streamLocation, PlayerUrl);
            string arguments = string.Format("--rtmp {0} --flv {1} --swfVfy {2}", video.StreamUrl, destination, video.PlayerUrl);

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
            }
#endif
        }
    }

    public class EmbedStream
    {
        public string StreamUrl { get; set; }
        public string PlayerUrl { get; set; }
    }
}
