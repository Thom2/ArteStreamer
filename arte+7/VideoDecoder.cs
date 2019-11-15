using HtmlAgilityPack;

namespace arte_7
{
    public class VideoDecoder
    {
        public static Video Decode(HtmlNode videoNode)
        {
            Video video = null;
            if (null != videoNode)
            {
                HtmlNode titleNode = videoNode.SelectSingleNode("./h2/a");
                // If there is no title node we don't have the complete video. Ignore it.
                if (null != titleNode)
                {
                    video = new Video();
                    video.Title = VideoDecoder.GetTitle(titleNode);
                    video.URL = VideoDecoder.GetURL(titleNode);
                    video.TeaserText = VideoDecoder.GetTeaserText(videoNode);
                    video.Thumbnail = VideoDecoder.GetThumbnail(videoNode);
                    video.Duration = VideoDecoder.GetVideoDuration(videoNode);
                    video.DisplayedAt = VideoDecoder.GetDisplayedAt(videoNode);
                }
            }
            return video;
        }

        private static string GetTitle(HtmlNode titleNode)
        {
            return HtmlEntity.DeEntitize(titleNode.InnerText);       
        }

        private static string GetURL(HtmlNode titleNode)
        {
            return GetNodeAttribute(titleNode, "href");           
        }

        private static string GetTeaserText(HtmlNode videoNode)
        {
            HtmlNode teaserTextNode = videoNode.SelectSingleNode("./div/div/p[@class='teaserText']");
            if (null != teaserTextNode)
            {
                return HtmlEntity.DeEntitize(teaserTextNode.InnerText);
            }
            return string.Empty;
        }

        private static string GetThumbnail(HtmlNode videoNode)
        {
            HtmlNode thumbNailNode = videoNode.SelectSingleNode("./div/a/img");
            if (null != thumbNailNode)
            {
                if (thumbNailNode.Attributes.Contains("src"))
                {
                    return Constants.URL_VIDEO_MAIN + thumbNailNode.Attributes["src"].Value;
                }
            }
            return string.Empty;
        }

        private static string GetVideoDuration(HtmlNode videoNode)
        {
            HtmlNode node = videoNode.SelectSingleNode("./div/div[@class='duration_thumbnail']");
            if (null != node)
            {
                return node.InnerText;
            }
            return string.Empty;
        }

        private static string GetDisplayedAt(HtmlNode videoNode)
        {
            HtmlNode displayedNode = videoNode.SelectSingleNode("./p");
            if (null != displayedNode)
            {
                return displayedNode.InnerText;
            }
            return string.Empty;
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
    }
}
