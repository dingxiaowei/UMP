using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System.Collections;

internal class VideoHostingYoutube : IVideoHosting
{
    private const string USER_AGENT = "Mozilla/5.0 \"(\"Windows NT 6.1; Win64; x64\")\" AppleWebKit/537.36 \"(\"KHTML, like Gecko\")\" Chrome/40.0.2214.115 Safari/537.36";

    private const string VIDEO_PARSER_NAME = "Youtube";
    private const string RATE_BYPASS_FLAG = "ratebypass";
    private const string SIGNATURE_QUERY = "signature";

    private string[] _parserUrlSignatures = { "youtu.be/", "www.youtube", "youtube.com/embed/" };

    public VideoHostingYoutube()
    {
    }

    public string GetVideoParserName()
    {
        return VIDEO_PARSER_NAME;
    }

    public string[] GetVideoParserSignatures()
    {
        return _parserUrlSignatures;
    }

    public bool TryNormalizeUrl(string url, out string normalizedUrl)
    {
        url = url.Trim();

        url = url.Replace(_parserUrlSignatures[0], "youtube.com/watch?v=");
        url = url.Replace(_parserUrlSignatures[1], "youtube");
        url = url.Replace(_parserUrlSignatures[2], "youtube.com/watch?v=");

        if (url.Contains("/v/"))
        {
            url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
        }

        url = url.Replace("/watch#", "/watch?");

        IDictionary<string, string> query = VideoHostingHelper.ParseQueryString(url);

        string v;

        if (!query.TryGetValue("v", out v))
        {
            normalizedUrl = null;
            return false;
        }

        normalizedUrl = "http://youtube.com/watch?v=" + v;

        return true;
    }

    public VideoHostingInfo[] Parse(VideoHostingJson data)
    {
        string videoTitle = GetVideoTitle(data);
        IEnumerable<ExtractionInfo> downloadUrls = ExtractDownloadUrls(data);
        IEnumerable<VideoHostingInfo> infos = GetVideoInfos(downloadUrls, videoTitle).ToList();

        return (VideoHostingInfo[])infos;
    }

    public IEnumerator Parse(string url, Action<string, VideoHostingInfo[]> completedCallback, Action<string> errorCallback)
    {
        var headers = new Dictionary<string, string>();
        headers.Add("User-Agent", USER_AGENT);
        var www = new WWW(url, null, headers);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            if (errorCallback != null)
                errorCallback("YoutubeVideoParser error: " + www.error);
            yield break;
        }

        try
        {
            string pageSource = www.text;
            if (IsVideoUnavailable(pageSource))
                Debug.Log("YoutubeVideoParser error: video is unavailable");

            var dataRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);
            string extractedJson = dataRegex.Match(pageSource).Result("$1");

            var data = new VideoHostingJson(extractedJson);
            string videoTitle = GetVideoTitle(data);
            IEnumerable<ExtractionInfo> downloadUrls = ExtractDownloadUrls(data);
            IEnumerable<VideoHostingInfo> videoInfos = GetVideoInfos(downloadUrls, videoTitle).ToList();
            string htmlPlayerVersion = GetHtml5PlayerVersion(data);

            foreach (VideoHostingInfo info in videoInfos)
                info.HtmlPlayerVersion = htmlPlayerVersion;

            if (completedCallback != null)
                completedCallback(url, videoInfos.ToArray());
        }
        catch (Exception e)
        {
            if (errorCallback != null)
                errorCallback("YoutubeVideoParser error: maybe your youtube link is incorrect or not supported - " + e.ToString());
        }
    }

    public IEnumerator DecryptDownloadUrl(VideoHostingInfo videoInfo, Action<VideoHostingInfo> completeCallback)
    {
        IDictionary<string, string> queries = VideoHostingHelper.ParseQueryString(videoInfo.DownloadUrl);

        if (queries.ContainsKey(SIGNATURE_QUERY))
        {
            string encryptedSignature = queries[SIGNATURE_QUERY];

            string decrypted;

            string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/player{0}.js", videoInfo.HtmlPlayerVersion);
            WWW www = new WWW(jsUrl);
            yield return www;

            try
            {
                decrypted = DeciphererYoutube.DecipherWithVersion(www.text, encryptedSignature, videoInfo.HtmlPlayerVersion);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not decipher signature", ex);
            }

            videoInfo.DownloadUrl = VideoHostingHelper.ReplaceQueryStringParameter(videoInfo.DownloadUrl, SIGNATURE_QUERY, decrypted);
            videoInfo.RequiresDecryption = false;

            if (completeCallback != null)
                completeCallback(videoInfo);
        }
    }

    private string GetVideoTitle(VideoHostingJson json)
    {
        string title = Regex.Unescape(json["args"]["title"].str);
        return title == null ? string.Empty : title;
    }

    private bool IsVideoUnavailable(string pageSource)
    {
        const string unavailableContainer = "<div id=\"watch-player-unavailable\">";

        return pageSource.Contains(unavailableContainer);
    }

    private static string GetStreamMap(VideoHostingJson json)
    {
        string streamMap = null;
        if (json["args"].keys.Contains("url_encoded_fmt_stream_map"))
            streamMap = Regex.Unescape(json["args"]["url_encoded_fmt_stream_map"].str);

        string streamMapString = streamMap == null ? null : streamMap.ToString();

        if (streamMapString == null || streamMapString.Contains("been+removed"))
        {
            throw new Exception("Video is removed or has an age restriction.");
        }

        return streamMapString;
    }

    private static string GetAdaptiveStreamMap(VideoHostingJson json)
    {
        string streamMap = null;
        if (json["args"].keys.Contains("adaptive_fmts"))
            streamMap = Regex.Unescape(json["args"]["adaptive_fmts"].str);

        if (streamMap == null)
        {
            streamMap = Regex.Unescape(json["args"]["url_encoded_fmt_stream_map"].str);
        }

        return streamMap.ToString();
    }

    private class ExtractionInfo
    {
        public bool RequiresDecryption { get; set; }

        public Uri Uri { get; set; }
    }

    private static IEnumerable<ExtractionInfo> ExtractDownloadUrls(VideoHostingJson json)
    {
        string[] splitByUrls = (GetStreamMap(json) + ',' + GetAdaptiveStreamMap(json)).Split(',');

        foreach (string s in splitByUrls)
        {
            IDictionary<string, string> queries = VideoHostingHelper.ParseQueryString(s);
            string url;
            bool requiresDecryption = false;

            if (queries.ContainsKey("s") || queries.ContainsKey("sig"))
            {
                requiresDecryption = queries.ContainsKey("s");
                string signature = queries.ContainsKey("s") ? queries["s"] : queries["sig"];

                url = string.Format("{0}&{1}={2}", queries["url"], SIGNATURE_QUERY, signature);

                string fallbackHost = queries.ContainsKey("fallback_host") ? "&fallback_host=" + queries["fallback_host"] : String.Empty;

                url += fallbackHost;
            }
            else
            {
                url = queries["url"];
            }

            url = VideoHostingHelper.UrlDecode(url);

            IDictionary<string, string> parameters = VideoHostingHelper.ParseQueryString(url);
            if (!parameters.ContainsKey(RATE_BYPASS_FLAG))
                url += string.Format("&{0}={1}", RATE_BYPASS_FLAG, "yes");

            yield return new ExtractionInfo { RequiresDecryption = requiresDecryption, Uri = new Uri(url) };
        }
    }

    private static IEnumerable<VideoHostingInfo> GetVideoInfos(IEnumerable<ExtractionInfo> extractionInfos, string videoTitle)
    {
        var downLoadInfos = new List<VideoHostingInfo>();

        foreach (ExtractionInfo extractionInfo in extractionInfos)
        {
            string itag = VideoHostingHelper.ParseQueryString(extractionInfo.Uri.Query)["itag"];

            int formatCode = int.Parse(itag);

            VideoHostingInfo info = VideoHostingInfo.Defaults.SingleOrDefault(videoInfo => videoInfo.FormatCode == formatCode);

            if (info != null)
            {
                info = new VideoHostingInfo(info)
                {
                    DownloadUrl = extractionInfo.Uri.ToString(),
                    Title = videoTitle,
                    RequiresDecryption = extractionInfo.RequiresDecryption
                };
            }

            else
            {
                info = new VideoHostingInfo(formatCode)
                {
                    DownloadUrl = extractionInfo.Uri.ToString()
                };
            }

            downLoadInfos.Add(info);
        }

        return downLoadInfos;
    }

    private static string GetHtml5PlayerVersion(VideoHostingJson json)
    {
        var regex = new Regex(@"player(.+?).js");

        string js = Regex.Unescape(json["assets"]["js"].str);
        return regex.Match(js).Result("$1");
    }
}
