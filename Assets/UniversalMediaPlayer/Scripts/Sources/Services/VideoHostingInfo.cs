using System.Collections.Generic;

public class VideoHostingInfo
{
    public enum ParsedAudioType
    {
        Aac,
        Mp3,
        Vorbis,

        /// <summary>
        /// The audio type is unknown. This can occur if YoutubeExtractor is not up-to-date.
        /// </summary>
        Unknown
    }

    public enum ParsedAdaptiveType
    {
        None,
        Audio,
        Video
    }

    /// <summary>
    /// The video type. Also known as video container.
    /// </summary>
    public enum ParsedVideoType
    {
        /// <summary>
        /// Video for mobile devices (3GP).
        /// </summary>
        Mobile,

        Flash,
        Mp4,
        WebM,

        /// <summary>
        /// The video type is unknown. This can occur if YoutubeExtractor is not up-to-date.
        /// </summary>
        Unknown
    }

    internal static IEnumerable<VideoHostingInfo> Defaults = new List<VideoHostingInfo>
        {
            /* Non-adaptive */
            new VideoHostingInfo(5, ParsedVideoType.Flash, 240, false, ParsedAudioType.Mp3, 64, ParsedAdaptiveType.None),
            new VideoHostingInfo(6, ParsedVideoType.Flash, 270, false, ParsedAudioType.Mp3, 64, ParsedAdaptiveType.None),
            new VideoHostingInfo(13, ParsedVideoType.Mobile, 0, false, ParsedAudioType.Aac, 0, ParsedAdaptiveType.None),
            new VideoHostingInfo(17, ParsedVideoType.Mobile, 144, false, ParsedAudioType.Aac, 24, ParsedAdaptiveType.None),
            new VideoHostingInfo(18, ParsedVideoType.Mp4, 360, false, ParsedAudioType.Aac, 96, ParsedAdaptiveType.None),
            new VideoHostingInfo(22, ParsedVideoType.Mp4, 720, false, ParsedAudioType.Aac, 192, ParsedAdaptiveType.None),
            new VideoHostingInfo(34, ParsedVideoType.Flash, 360, false, ParsedAudioType.Aac, 128, ParsedAdaptiveType.None),
            new VideoHostingInfo(35, ParsedVideoType.Flash, 480, false, ParsedAudioType.Aac, 128, ParsedAdaptiveType.None),
            new VideoHostingInfo(36, ParsedVideoType.Mobile, 240, false, ParsedAudioType.Aac, 38, ParsedAdaptiveType.None),
            new VideoHostingInfo(37, ParsedVideoType.Mp4, 1080, false, ParsedAudioType.Aac, 192, ParsedAdaptiveType.None),
            new VideoHostingInfo(38, ParsedVideoType.Mp4, 3072, false, ParsedAudioType.Aac, 192, ParsedAdaptiveType.None),
            new VideoHostingInfo(43, ParsedVideoType.WebM, 360, false, ParsedAudioType.Vorbis, 128, ParsedAdaptiveType.None),
            new VideoHostingInfo(44, ParsedVideoType.WebM, 480, false, ParsedAudioType.Vorbis, 128, ParsedAdaptiveType.None),
            new VideoHostingInfo(45, ParsedVideoType.WebM, 720, false, ParsedAudioType.Vorbis, 192, ParsedAdaptiveType.None),
            new VideoHostingInfo(46, ParsedVideoType.WebM, 1080, false, ParsedAudioType.Vorbis, 192, ParsedAdaptiveType.None),

            /* 3d */
            new VideoHostingInfo(82, ParsedVideoType.Mp4, 360, true, ParsedAudioType.Aac, 96, ParsedAdaptiveType.None),
            new VideoHostingInfo(83, ParsedVideoType.Mp4, 240, true, ParsedAudioType.Aac, 96, ParsedAdaptiveType.None),
            new VideoHostingInfo(84, ParsedVideoType.Mp4, 720, true, ParsedAudioType.Aac, 152, ParsedAdaptiveType.None),
            new VideoHostingInfo(85, ParsedVideoType.Mp4, 520, true, ParsedAudioType.Aac, 152, ParsedAdaptiveType.None),
            new VideoHostingInfo(100, ParsedVideoType.WebM, 360, true, ParsedAudioType.Vorbis, 128, ParsedAdaptiveType.None),
            new VideoHostingInfo(101, ParsedVideoType.WebM, 360, true, ParsedAudioType.Vorbis, 192, ParsedAdaptiveType.None),
            new VideoHostingInfo(102, ParsedVideoType.WebM, 720, true, ParsedAudioType.Vorbis, 192, ParsedAdaptiveType.None),

            /* Adaptive (aka DASH) - Video */
            new VideoHostingInfo(133, ParsedVideoType.Mp4, 240, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(134, ParsedVideoType.Mp4, 360, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(135, ParsedVideoType.Mp4, 480, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(136, ParsedVideoType.Mp4, 720, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(137, ParsedVideoType.Mp4, 1080, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(138, ParsedVideoType.Mp4, 2160, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(160, ParsedVideoType.Mp4, 144, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(242, ParsedVideoType.WebM, 240, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(243, ParsedVideoType.WebM, 360, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(244, ParsedVideoType.WebM, 480, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(247, ParsedVideoType.WebM, 720, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(248, ParsedVideoType.WebM, 1080, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(264, ParsedVideoType.Mp4, 1440, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(266, ParsedVideoType.Mp4, 2160, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(271, ParsedVideoType.WebM, 1440, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(272, ParsedVideoType.WebM, 2160, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(278, ParsedVideoType.WebM, 144, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(298, ParsedVideoType.Mp4, 720, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(299, ParsedVideoType.Mp4, 1080, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(302, ParsedVideoType.WebM, 720, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(303, ParsedVideoType.WebM, 1080, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(308, ParsedVideoType.WebM, 1440, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(313, ParsedVideoType.WebM, 2160, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),
            new VideoHostingInfo(315, ParsedVideoType.WebM, 2160, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.Video),

            /* Adaptive (aka DASH) - Audio */
            new VideoHostingInfo(139, ParsedVideoType.Mp4, 0, false, ParsedAudioType.Aac, 48, ParsedAdaptiveType.Audio),
            new VideoHostingInfo(140, ParsedVideoType.Mp4, 0, false, ParsedAudioType.Aac, 128, ParsedAdaptiveType.Audio),
            new VideoHostingInfo(141, ParsedVideoType.Mp4, 0, false, ParsedAudioType.Aac, 256, ParsedAdaptiveType.Audio),
            new VideoHostingInfo(171, ParsedVideoType.WebM, 0, false, ParsedAudioType.Vorbis, 128, ParsedAdaptiveType.Audio),
            new VideoHostingInfo(172, ParsedVideoType.WebM, 0, false, ParsedAudioType.Vorbis, 192, ParsedAdaptiveType.Audio)
        };

    internal VideoHostingInfo(int formatCode)
        : this(formatCode, ParsedVideoType.Unknown, 0, false, ParsedAudioType.Unknown, 0, ParsedAdaptiveType.None)
    { }

    internal VideoHostingInfo(VideoHostingInfo info)
        : this(info.FormatCode, info.VideoType, info.Resolution, info.Is3D, info.AudioType, info.AudioBitrate, info.AdaptiveType)
    { }

    private VideoHostingInfo(int formatCode, ParsedVideoType videoType, int resolution, bool is3D, ParsedAudioType audioType, int audioBitrate, ParsedAdaptiveType adaptiveType)
    {
        this.FormatCode = formatCode;
        this.VideoType = videoType;
        this.Resolution = resolution;
        this.Is3D = is3D;
        this.AudioType = audioType;
        this.AudioBitrate = audioBitrate;
        this.AdaptiveType = adaptiveType;
    }

    /// <summary>
    /// Gets an enum indicating whether the format is adaptive or not.
    /// </summary>
    /// <value>
    /// <c>AdaptiveType.Audio</c> or <c>AdaptiveType.Video</c> if the format is adaptive;
    /// otherwise, <c>AdaptiveType.None</c>.
    /// </value>
    public ParsedAdaptiveType AdaptiveType { get; private set; }

    /// <summary>
    /// The approximate audio bitrate in kbit/s.
    /// </summary>
    /// <value>The approximate audio bitrate in kbit/s, or 0 if the bitrate is unknown.</value>
    public int AudioBitrate { get; private set; }

    /// <summary>
    /// Gets the audio extension.
    /// </summary>
    /// <value>The audio extension, or <c>null</c> if the audio extension is unknown.</value>
    public string AudioExtension
    {
        get
        {
            switch (this.AudioType)
            {
                case ParsedAudioType.Aac:
                    return ".aac";

                case ParsedAudioType.Mp3:
                    return ".mp3";

                case ParsedAudioType.Vorbis:
                    return ".ogg";
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the audio type (encoding).
    /// </summary>
    public ParsedAudioType AudioType { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the audio of this video can be extracted by YoutubeExtractor.
    /// </summary>
    /// <value>
    /// <c>true</c> if the audio of this video can be extracted by YoutubeExtractor; otherwise, <c>false</c>.
    /// </value>
    public bool CanExtractAudio
    {
        get { return this.VideoType == ParsedVideoType.Flash; }
    }

    /// <summary>
    /// Gets the download URL.
    /// </summary>
    public string DownloadUrl { get; internal set; }

    /// <summary>
    /// Gets the format code, that is used by YouTube internally to differentiate between
    /// quality profiles.
    /// </summary>
    public int FormatCode { get; private set; }

    public bool Is3D { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this video info requires a signature decryption before
    /// the download URL can be used.
    ///
    /// This can be achieved with the <see cref="DownloadUrlResolver.DecryptDownloadUrl"/>
    /// </summary>
    public bool RequiresDecryption { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this video info has been decrypted.
    /// </summary>
    public bool IsDecrypted { get; internal set; }

    /// <summary>
    /// Gets the resolution of the video.
    /// </summary>
    /// <value>The resolution of the video, or 0 if the resolution is unkown.</value>
    public int Resolution { get; private set; }

    /// <summary>
    /// Gets the video title.
    /// </summary>
    public string Title { get; internal set; }

    /// <summary>
    /// Gets the video extension.
    /// </summary>
    /// <value>The video extension, or <c>null</c> if the video extension is unknown.</value>
    public string VideoExtension
    {
        get
        {
            switch (this.VideoType)
            {
                case ParsedVideoType.Mp4:
                    return ".mp4";

                case ParsedVideoType.Mobile:
                    return ".3gp";

                case ParsedVideoType.Flash:
                    return ".flv";

                case ParsedVideoType.WebM:
                    return ".webm";
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the video type (container).
    /// </summary>
    public ParsedVideoType VideoType { get; private set; }

    /// <summary>
    /// We use this in the <see cref="DownloadUrlResolver.DecryptDownloadUrl" /> method to
    /// decrypt the signature
    /// </summary>
    /// <returns></returns>
    internal string HtmlPlayerVersion { get; set; }

    public override string ToString()
    {
        return string.Format("Full Title: {0}, Type: {1}, Resolution: {2}p, AudioType: {3}, AudioBitrate: {4}", this.Title + this.VideoExtension, this.VideoType, this.Resolution, this.AudioType, this.AudioBitrate);
    }
}
