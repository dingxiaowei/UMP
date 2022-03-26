using System;
using System.Collections;

internal interface IVideoHosting
{
    string GetVideoParserName();
    string[] GetVideoParserSignatures();
    bool TryNormalizeUrl(string url, out string normalizedUrl);
    IEnumerator Parse(string url, Action<string, VideoHostingInfo[]> doneAction, Action<string> errorAction);
    IEnumerator DecryptDownloadUrl(VideoHostingInfo videoInfo, Action<VideoHostingInfo> completeCallback);
}
