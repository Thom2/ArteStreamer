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
                HtmlNode titleNode = videoNode.SelectSingleNode("./div/a/div/img");
                HtmlNode teaserCaption = videoNode.SelectSingleNode(".//div/a/div[@class='next-teaser__caption']");
                // If there is no title node we don't have the complete video. Ignore it.
                if (null != titleNode)
                {
                    video = new Video();
                    video.Title = VideoDecoder.GetTitle(teaserCaption);
                    video.URL = VideoDecoder.GetURL(videoNode);
                    //video.TeaserText = VideoDecoder.GetTeaserText(videoNode);
                    video.Thumbnail = VideoDecoder.GetThumbnail(titleNode);
                    video.Duration = VideoDecoder.GetVideoDuration(teaserCaption);
                    //video.DisplayedAt = VideoDecoder.GetDisplayedAt(videoNode);
                }
            }
            return video;
        }

        private static string GetTitle(HtmlNode node)
        {
            return HtmlEntity.DeEntitize(node.SelectSingleNode("./h3").InnerText);
        }

        private static string GetURL(HtmlNode node)
        {
            return node.SelectSingleNode("./div/a").GetAttributeValue("href", "");
        }

        private static string GetTeaserText(HtmlNode videoNode)
        {
            return string.Empty;
        }

        private static string GetThumbnail(HtmlNode videoNode)
        {
            return HtmlEntity.DeEntitize(videoNode.GetAttributeValue("src", ""));
        }

        private static string GetVideoDuration(HtmlNode node)
        {
            return HtmlEntity.DeEntitize(node.SelectSingleNode("./p").InnerText);
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
