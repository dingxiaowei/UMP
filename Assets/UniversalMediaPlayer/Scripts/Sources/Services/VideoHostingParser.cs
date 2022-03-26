using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoHostingParser
{
    private MonoBehaviour _monoObject;
    private Dictionary<string, VideoHostingInfo[]> _cachedVideoUrls;
    private List<IVideoHosting> _videoParsers;
    private IVideoHosting _currentVideoParser;
    private IEnumerator _videoParserEnum;
    private IEnumerator _videoDecryptEnum;
    private bool _inProcess;
    private Action<VideoHostingInfo[]> _parsingDoneAction;
    private Action<string> _parsingErrorAction;
    private Action<VideoHostingInfo> _decryptionDoneAction;

    public bool InProcess
    {
        get { return _inProcess; }
    }

    public VideoHostingParser(MonoBehaviour monoObject)
    {
        _monoObject = monoObject;
        _cachedVideoUrls = new Dictionary<string, VideoHostingInfo[]>();
        _videoParsers = new List<IVideoHosting>();
        _videoParsers.Add(new VideoHostingYoutube());
    }

    public bool IsVideoHostingUrl(string url)
    {
        foreach(var parser in _videoParsers)
        {
            foreach (var signature in parser.GetVideoParserSignatures())
            {
                if (url.Contains(signature))
                {
                    _currentVideoParser = parser;
                    return true;
                }
            }
        }
        return false;
    }

    public VideoHostingInfo[] GetCachedVideoInfos(string url)
    {
        if (_cachedVideoUrls.Count > 0 && _cachedVideoUrls.ContainsKey(url))
            return _cachedVideoUrls[url];

        return null;
    }

    public VideoHostingInfo GetBestCompatibleVideo(VideoHostingInfo[] videoInfos)
    {
        int maxResolution = 0;
        int resultIndex = 0;

        for (int i = 0; i < videoInfos.Length; i++)
        {
            if (videoInfos[i].AudioBitrate > 0 && videoInfos[i].Resolution > maxResolution)
            {
                maxResolution = videoInfos[i].Resolution;
                resultIndex = i;
            }
        }

        return videoInfos[resultIndex];
    }

    public VideoHostingInfo GetBestQualityVideo(VideoHostingInfo[] videoInfos, int maxQuality)
    {
        VideoHostingInfo videoWithBestResolution = new VideoHostingInfo(0);
        foreach (var info in videoInfos)
        {
            if (info.Resolution < maxQuality && info.Resolution > videoWithBestResolution.Resolution)
                videoWithBestResolution = info;
        }

        return videoWithBestResolution;
    }

    public void DecryptVideoUrl(VideoHostingInfo videoInfo, Action<VideoHostingInfo> completeCallback)
    {
        foreach (var cachedUrl in _cachedVideoUrls)
        {
            for (int i = 0; i < cachedUrl.Value.Length; i++)
            {
                if (cachedUrl.Value[i].DownloadUrl == videoInfo.DownloadUrl)
                {
                    if (videoInfo.RequiresDecryption && !videoInfo.IsDecrypted)
                    {
                        if (_videoDecryptEnum != null)
                            _monoObject.StopCoroutine(_videoDecryptEnum);

                        _decryptionDoneAction = completeCallback;
                        _videoDecryptEnum = _currentVideoParser.DecryptDownloadUrl(videoInfo, OnDecryptDone);
                        _monoObject.StartCoroutine(_videoDecryptEnum);
                        videoInfo.IsDecrypted = true;
                    }
                    cachedUrl.Value[i] = videoInfo;
                }
            }
        }
    }

    private void OnDecryptDone(VideoHostingInfo videoInfo)
    {
        _decryptionDoneAction(videoInfo);
    }

    public void ParseVideoInfos(string url, Action<VideoHostingInfo[]> completeCallback, Action<string> errorCallback)
    {
        if (GetCachedVideoInfos(url) != null)
            completeCallback(_cachedVideoUrls[url]);

        if (IsVideoHostingUrl(url))
        {
            _inProcess = true;
            if (_videoParserEnum != null)
                _monoObject.StopCoroutine(_videoParserEnum);

            _parsingDoneAction = completeCallback;
            _parsingErrorAction = errorCallback;
            _videoParserEnum = _currentVideoParser.Parse(url, OnParsingDone, _parsingErrorAction);
            _monoObject.StartCoroutine(_videoParserEnum);
        }
        else
        {
            if (errorCallback != null)
                errorCallback("VideoHostingsParser error: Incorrect URL - " + url);
            else
                Debug.LogError("VideoHostingsParser error: Incorrect URL - " + url);
        }
    }

    private void OnParsingDone(string url, VideoHostingInfo[] videoInfos)
    {
        _cachedVideoUrls.Add(url, videoInfos);

        _inProcess = false;

        if (_parsingDoneAction != null)
            _parsingDoneAction(videoInfos);
    }

    private void OnParsingError(string error)
    {
        _inProcess = false;

        if (_parsingErrorAction != null)
            _parsingErrorAction(error);
    }

    public void Release()
    {
        _cachedVideoUrls.Clear();
    }
}
