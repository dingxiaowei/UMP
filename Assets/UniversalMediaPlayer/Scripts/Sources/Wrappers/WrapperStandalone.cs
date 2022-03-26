using System;
using System.Runtime.InteropServices;

namespace UMP.Wrappers
{
    internal class WrapperStandalone : IWrapperNative, IWrapperPlayer, IWrapperSpu, IWrapperAudio
    {   
        private IntPtr _libVLCHandler = IntPtr.Zero;
        private IntPtr _libUMPHandler = IntPtr.Zero;

        private int _nativeIndex;

        private delegate void ManageLogCallback(string msg);
        private ManageLogCallback _manageLogCallback;

        #region Native Imports
        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern int UMPNativeInit();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativeUpdateIndex(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativeSetTexture(int index, IntPtr texture);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativeSetPixelsBuffer(int index, IntPtr buffer, int width, int height);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetPixelsBuffer(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern int UMPNativeGetPixelsBufferWidth(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern int UMPNativeGetPixelsBufferHeight(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativePixelsBufferRelease(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetLogMessage(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern int UMPNativeGetLogLevel(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern int UMPNativeGetState(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern float UMPNativeGetStateFloatValue(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern long UMPNativeGetStateLongValue(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetStateStringValue(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern int UMPNativeSetPixelsVerticalFlip(int index, bool flip);

        [DllImport(UMPSettings.ASSET_NAME)]
        internal static extern void UMPNativeSetAudioParams(int index, int numChannels, int sampleRate);

        [DllImport(UMPSettings.ASSET_NAME)]
        internal static extern IntPtr UMPNativeGetAudioParams(int index, char separator);

        [DllImport(UMPSettings.ASSET_NAME)]
        internal static extern int UMPNativeGetAudioSamples(int index, IntPtr decodedSamples, int samplesLength, AudioOutput.AudioChannels audioChannel);

        [DllImport(UMPSettings.ASSET_NAME)]
        internal static extern bool UMPNativeClearAudioSamples(int index, int samplesLength);

        [DllImport(UMPSettings.ASSET_NAME)]
        internal static extern void UMPNativeDirectRender(int index);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetUnityRenderCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetVideoLockCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetVideoDisplayCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetVideoFormatCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetVideoCleanupCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetAudioSetupCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetAudioPlayCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeMediaPlayerEventCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern IntPtr UMPNativeGetLogMessageCallback();

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativeSetUnityLogMessageCallback(IntPtr callback);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern void UMPNativeUpdateFramesCounter(int index, int counter);

        [DllImport(UMPSettings.ASSET_NAME)]
        private static extern int UMPNativeGetFramesCounter(int index);

        [InteropFunction("UMPNativeInit")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UMPNativeInitDel();

        [InteropFunction("UMPNativeUpdateIndex")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UMPNativeUpdateIndexDel(int index);

        [InteropFunction("UMPNativeSetTexture")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UMPNativeSetTextureDel(int index, IntPtr texture);

        [InteropFunction("UMPNativeSetPixelsBuffer")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UMPNativeSetPixelsBufferDel(int index, IntPtr buffer, int width, int height);

        [InteropFunction("UMPNativeGetPixeslBuffer")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetPixelsBufferDel(int index);

        [InteropFunction("UMPNativeGetPixelsBufferWidth")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UMPNativeGetPixelsBufferWidthDel(int index);

        [InteropFunction("UMPNativeGetPixelsBufferHeight")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UMPNativeGetPixelsBufferHeightDel(int index);

        [InteropFunction("UMPNativePixelsBufferRelease")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UMPNativePixelsBufferReleaseDel(int index);

        [InteropFunction("UMPNativeGetLogMessage")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetLogMessageDel(int index);

        [InteropFunction("UMPNativeGetLogLevel")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UMPNativeGetLogLevelDel(int index);

        [InteropFunction("UMPNativeGetState")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UMPNativeGetStateDel(int index);

        [InteropFunction("UMPNativeGetStateFloatValue")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float UMPNativeGetStateFloatValueDel(int index);

        [InteropFunction("UMPNativeGetStateLongValue")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long UMPNativeGetStateLongValueDel(int index);

        [InteropFunction("UMPNativeGetStateStringValue")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetStateStringValueDel(int index);

        [InteropFunction("UMPNativeSetPixelsVerticalFlip")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UMPNativeSetPixelsVerticalFlipDel(int index, bool flip);

        [InteropFunction("UMPNativeSetAudioParams")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UMPNativeSetAudioParamsDel(int index, int numChannels, int sampleRate);

        [InteropFunction("UMPNativeGetAudioParams")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetAudioParamsDel(int index, char separator);

        [InteropFunction("UMPNativeGetAudioChannels")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UMPNativeGetAudioChannelsDel(int index);

        [InteropFunction("UMPNativeGetAudioSamples")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UMPNativeGetAudioSamplesDel(int index, IntPtr decodedSamples, int samplesLength, AudioOutput.AudioChannels audioChannel);

        [InteropFunction("UMPNativeClearAudioSamples")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool UMPNativeClearAudioSamplesDel(int index, int samplesLength);

        [InteropFunction("UMPNativeDirectRender")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UMPNativeDirectRenderDel(int index);

        [InteropFunction("UMPNativeGetUnityRenderCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetUnityRenderCallbackDel();

        [InteropFunction("UMPNativeGetVideoLockCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetVideoLockCallbackDel();

        [InteropFunction("UMPNativeGetVideoDisplayCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetVideoDisplayCallbackDel();

        [InteropFunction("UMPNativeGetVideoFormatCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetVideoFormatCallbackDel();

        [InteropFunction("UMPNativeGetVideoCleanupCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetVideoCleanupCallbackDel();

        [InteropFunction("UMPNativeGetAudioSetupCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetAudioSetupCallbackDel();

        [InteropFunction("UMPNativeGetAudioPlayCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetAudioPlayCallbackDel();

        [InteropFunction("UMPNativeMediaPlayerEventCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeMediaPlayerEventCallbackDel();

        [InteropFunction("UMPNativeGetLogMessageCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr UMPNativeGetLogMessageCallbackDel();

        [InteropFunction("UMPNativeSetUnityLogMessageCallback")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UMPNativeSetUnityLogMessageCallbackDel(IntPtr callback);

        [InteropFunction("UMPNativeUpdateFramesCounter")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UMPNativeUpdateFramesCounterCallbackDel(int index, int counter);

        [InteropFunction("UMPNativeGetFramesCounter")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UMPNativeGetFramesCounterCallbackDel(int index);
        #endregion

        #region LibVLC Imports
        /// <summary>
        /// Set current crop filter geometry.
        /// </summary>
        [InteropFunction("libvlc_video_set_aspect_ratio")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetVideoAspectRatio(IntPtr playerObject, string cropGeometry);

        /// <summary>
        /// Unset libvlc log instance.
        /// </summary>
        [InteropFunction("libvlc_log_unset")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LogUnset(IntPtr instance);

        /// <summary>
        /// Unset libvlc log instance.
        /// </summary>
        [InteropFunction("libvlc_log_set")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LogSet(IntPtr instance, IntPtr callback, IntPtr data);

        /// <summary>
        /// Create and initialize a libvlc instance.
        /// </summary>
        /// <returns>Return the libvlc instance or NULL in case of error.</returns>
        [InteropFunction("libvlc_new")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CreateNewInstance(int argc, string[] argv);

        /// <summary>
        /// Destroy libvlc instance.
        /// </summary>
        [InteropFunction("libvlc_release")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ReleaseInstance(IntPtr instance);

        /// <summary>
        /// Frees an heap allocation returned by a LibVLC function.
        /// </summary>
        /// <returns>Return the libvlc instance or NULL in case of error.</returns>
        [InteropFunction("libvlc_free")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void Free(IntPtr ptr);

        /// <summary>
        /// CCallback function notification.
        /// </summary>
        [InteropFunction("libvlc_callback_t")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EventCallback(IntPtr args);

        /// <summary>
        /// Register for an event notification.
        /// </summary>
        /// <param name="eventManagerInstance">Event manager to which you want to attach to.</param>
        /// <param name="eventType">The desired event to which we want to listen.</param>
        /// <param name="callback">The function to call when i_event_type occurs.</param>
        /// <param name="userData">User provided data to carry with the event.</param>
        /// <returns>Return 0 on success, ENOMEM on error.</returns>
        [InteropFunction("libvlc_event_attach")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int AttachEvent(IntPtr eventManagerInstance, EventTypes eventType, IntPtr callback, IntPtr userData);

        /// <summary>
        /// Unregister an event notification.
        /// </summary>
        /// <param name="eventManagerInstance">Event manager to which you want to attach to.</param>
        /// <param name="eventType">The desired event to which we want to listen.</param>
        /// <param name="callback">The function to call when i_event_type occurs.</param>
        /// <param name="userData">User provided data to carry with the event.</param>
        /// <returns>Return 0 on success, ENOMEM on error.</returns>
        [InteropFunction("libvlc_event_detach")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int DetachEvent(IntPtr eventManagerInstance, EventTypes eventType, IntPtr callback, IntPtr userData);

        /// <summary>
        /// Create a media with a certain given media resource location, for instance a valid URL.
        /// </summary>
        /// <returns>Return the newly created media or NULL on error.</returns>
        [InteropFunction("libvlc_media_new_location")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CreateNewMediaFromLocation(IntPtr instance, [MarshalAs(UnmanagedType.LPStr)] string path);

        /// <summary>
        /// Create a media for a certain file path.
        /// </summary>
        /// <returns>Return the newly created media or NULL on error.</returns>
        [InteropFunction("libvlc_media_new_path")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CreateNewMediaFromPath(IntPtr instance, [MarshalAs(UnmanagedType.LPStr)] string path);

        /// <summary>
        /// Add an option to the media.
        /// </summary>
        [InteropFunction("libvlc_media_add_option")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void AddOptionToMedia(IntPtr mediaInstance, [MarshalAs(UnmanagedType.LPStr)] string option);

        /// <summary>
        /// It will release the media descriptor object. It will send out an libvlc_MediaFreed event to all listeners. If the media descriptor object has been released it should not be used again.
        /// </summary>
        [InteropFunction("libvlc_media_release")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ReleaseMedia(IntPtr mediaInstance);

        /// <summary>
        /// Get the media resource locator (mrl) from a media descriptor object.
        /// </summary>
        [InteropFunction("libvlc_media_get_mrl")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetMediaMrl(IntPtr mediaInstance);

        /// <summary>
        /// Get Parsed status for media descriptor object.
        /// </summary>
        [InteropFunction("libvlc_media_get_parsed_status")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate ParsedStatus GetMediaParsedStatus(IntPtr mediaInstance);

        /// <summary>
        /// Read a meta of the media.
        /// </summary>
        [InteropFunction("libvlc_media_get_meta")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetMediaMetadata(IntPtr mediaInstance, MediaMetadatas meta);

        /// <summary>
        /// Get the current statistics about the media
        /// </summary>
        /// <param name="mediaInstance">Media descriptor object</param>
        /// <param name="statsInformationsPointer">Structure that contain
        /// the statistics about the media 
        /// (this structure must be allocated by the caller)</param>
        /// <returns></returns>
        [InteropFunction("libvlc_media_get_stats")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetMediaStats(IntPtr mediaInstance, out MediaStats statsInformationsPointer);

        /// <summary>
        /// Parse a media meta data and tracks information. 
        /// </summary>
        [InteropFunction("libvlc_media_parse")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ParseMedia(IntPtr mediaInstance);

        /// <summary>
        /// Parse a media meta data and tracks information async. 
        /// </summary>
        [InteropFunction("libvlc_media_parse_async")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ParseMediaAsync(IntPtr mediaInstance);

        /// <summary>
        /// Get Parsed status for media descriptor object.
        /// </summary>
        [InteropFunction("libvlc_media_is_parsed")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int IsParsedMedia(IntPtr mediaInstance);

        /// <summary>
        /// Get media descriptor's elementary streams description.
        /// </summary>
        [InteropFunction("libvlc_media_get_tracks_info")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetMediaTracksInformations(IntPtr mediaInstance, out IntPtr tracksInformationsPointer);

        /// <summary>
        /// Create an empty Media Player object.
        /// </summary>
        /// <returns>Return a new media player object, or NULL on error.</returns>
        [InteropFunction("libvlc_media_player_new")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CreateMediaPlayer(IntPtr instance);

        /// <summary>
        /// It will release the media player object. If the media player object has been released, then it should not be used again.
        /// </summary>
        [InteropFunction("libvlc_media_player_release")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ReleaseMediaPlayer(IntPtr playerObject);

        /// <summary>
        /// Set the media that will be used by the media_player. If any, previous media will be released.
        /// </summary>
        /// <returns>Return a new media player object, or NULL on error.</returns>
        [InteropFunction("libvlc_media_player_set_media")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetMediaToMediaPlayer(IntPtr playerObject, IntPtr mediaInstance);

        /// <summary>
        /// Get the Event Manager from which the media player send event.
        /// </summary>
        /// <returns>Return the event manager associated with media player.</returns>
        [InteropFunction("libvlc_media_player_event_manager")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetMediaPlayerEventManager(IntPtr playerObject);

        /// <summary>
        /// Check if media player is playing.
        /// </summary>
        [InteropFunction("libvlc_media_player_is_playing")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int IsPlaying(IntPtr playerObject);

        /// <summary>
        /// Play.
        /// </summary>
        /// <returns>Return 0 if playback started (and was already started), or -1 on error.</returns>
        [InteropFunction("libvlc_media_player_play")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int Play(IntPtr playerObject);

        /// <summary>
        /// Toggle pause (no effect if there is no media).
        /// </summary>
        [InteropFunction("libvlc_media_player_pause")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void Pause(IntPtr playerObject);

        /// <summary>
        /// Stop.
        /// </summary>
        [InteropFunction("libvlc_media_player_stop")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void Stop(IntPtr playerObject);

        /// <summary>
        /// Set video callbacks.
        /// </summary>
        [InteropFunction("libvlc_video_set_callbacks")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetVideoCallbacks(IntPtr playerObject, IntPtr @lock, IntPtr unlock, IntPtr display, IntPtr opaque);

        /// <summary>
        /// Set video format.
        /// </summary>
        [InteropFunction("libvlc_video_set_format")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetVideoFormat(IntPtr playerObject, [MarshalAs(UnmanagedType.LPStr)] string chroma, uint width, uint height, uint pitch);

        /// <summary>
        /// Set video format callbacks.
        /// </summary>
        [InteropFunction("libvlc_video_set_format_callbacks")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetVideoFormatCallbacks(IntPtr playerObject, IntPtr setup, IntPtr cleanup);

        /// <summary>
        /// Get length of movie playing
        /// </summary>
        /// <returns>Get the requested movie play rate.</returns>
        [InteropFunction("libvlc_media_player_get_length")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long GetLength(IntPtr playerObject);

        /// <summary>
        /// Get Rate at which movie is playing.
        /// </summary>
        /// <returns>Get the requested movie play rate.</returns>
        [InteropFunction("libvlc_media_player_get_time")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long GetTime(IntPtr playerObject);

        /// <summary>
        /// Get time at which movie is playing.
        /// </summary>
        /// <returns>Get the requested movie play time.</returns>
        [InteropFunction("libvlc_media_player_set_time")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetTime(IntPtr playerObject, long time);

        /// <summary>
        /// Get media position.
        /// </summary>
        [InteropFunction("libvlc_media_player_get_position")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float GetMediaPosition(IntPtr playerObject);

        /// <summary>
        /// Get media position.
        /// </summary>
        [InteropFunction("libvlc_media_player_set_position")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]

        private delegate void SetMediaPosition(IntPtr playerObject, float position);

        /// <summary>
        /// Is the player able to play.
        /// </summary>
        [InteropFunction("libvlc_media_player_will_play")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int CouldPlay(IntPtr playerObject);

        /// <summary>
        /// Get the requested media play rate.
        /// </summary>
        [InteropFunction("libvlc_media_player_get_rate")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float GetRate(IntPtr playerObject);

        /// <summary>
        /// Set the requested media play rate.
        /// </summary>
        [InteropFunction("libvlc_media_player_set_rate")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetRate(IntPtr playerObject, float rate);

        /// <summary>
        /// Get the media state.
        /// </summary>
        [InteropFunction("libvlc_media_player_get_state")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate MediaStates GetMediaPlayerState(IntPtr playerObject);

        /// <summary>
        /// Get media fps rate.
        /// </summary>
        [InteropFunction("libvlc_media_player_get_fps")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float GetFramesPerSecond(IntPtr playerObject);

        /// <summary>
        /// Get video size in pixels.
        /// </summary>
        [InteropFunction("libvlc_video_get_size")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetVideoSize(IntPtr playerObject, uint num, out uint px, out uint py);

        /// <summary>
        /// Get video scale.
        /// </summary>
        [InteropFunction("libvlc_video_get_scale")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float GetVideoScale(IntPtr playerObject);

        /// <summary>
        /// Set video scale.
        /// </summary>
        [InteropFunction("libvlc_video_set_scale")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float SetVideoScale(IntPtr playerObject, float f_factor);

        /// <summary>
        /// Take a snapshot of the current video window.
        /// </summary>
        /// <returns>Return 0 on success, -1 if the video was not found.</returns>
        [InteropFunction("libvlc_video_take_snapshot")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int TakeSnapshot(IntPtr playerObject, uint num, string fileName, uint width, uint height);

        #region Spu setup
        /// <summary>
        /// Get the number of available video subtitles.
        /// </summary>
        [InteropFunction("libvlc_video_get_spu_count")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetVideoSpuCount(IntPtr playerObject);

        /// <summary>
        /// Get the description of available video subtitles.
        /// </summary>
        [InteropFunction("libvlc_video_get_spu_description")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetVideoSpuDescription(IntPtr playerObject);

        /// <summary>
        /// Get current video subtitle.
        /// </summary>
        [InteropFunction("libvlc_video_get_spu")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]

        private delegate int GetVideoSpu(IntPtr playerObject);

        /// <summary>
        /// Set new video subtitle.
        /// </summary>
        [InteropFunction("libvlc_video_set_spu")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetVideoSpu(IntPtr playerObject, int spu);

        /// <summary>
        /// Set new video subtitle file.
        /// </summary>
        [InteropFunction("libvlc_video_set_subtitle_file")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetVideoSubtitleFile(IntPtr playerObject, [MarshalAs(UnmanagedType.LPStr)] string path);
        #endregion

        #region Audio setup
        /// <summary>
        /// Set audio format.
        /// </summary>
        [InteropFunction("libvlc_audio_set_format")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetAudioFormat(IntPtr playerObject, [MarshalAs(UnmanagedType.LPStr)] string format, int rate, int channels);

        /// <summary>
        /// Set audio callbacks.
        /// </summary>
        [InteropFunction("libvlc_audio_set_callbacks")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetAudioCallbacks(IntPtr playerObject, IntPtr play, IntPtr pause, IntPtr resume, IntPtr flush, IntPtr drain, IntPtr opaque);

        /// <summary>
        /// Set audio format callbacks.
        /// </summary>
        [InteropFunction("libvlc_audio_set_format_callbacks")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetAudioFormatCallbacks(IntPtr playerObject, IntPtr setup, IntPtr cleanup);

        /// <summary>
        /// Get number of available audio tracks.
        /// </summary>
        [InteropFunction("libvlc_audio_get_track_count")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetAudioTracksCount(IntPtr playerObject);

        /// <summary>
        /// Get the description of available audio tracks.
        /// </summary>
        [InteropFunction("libvlc_audio_get_track_description")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetAudioTracksDescriptions(IntPtr playerObject);

        /// <summary>
        /// Relase the description of available track.
        /// </summary>
        [InteropFunction("libvlc_track_description_release")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr ReleaseTracksDescriptions(IntPtr trackDescription);

        /// <summary>
        /// Get current audio track.
        /// </summary>
        [InteropFunction("libvlc_audio_get_track")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetAudioTrack(IntPtr playerObject);

        /// <summary>
        /// Set audio track.
        /// </summary>
        [InteropFunction("libvlc_audio_set_track")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetAudioTrack(IntPtr playerObject, int trackId);

        /// <summary>
        /// Get current audio delay.
        /// </summary>
        [InteropFunction("libvlc_audio_get_delay")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long GetAudioDelay(IntPtr playerObject);

        /// <summary>
        /// Set current audio delay. The audio delay will be reset to zero each time the media changes.
        /// </summary>
        [InteropFunction("libvlc_audio_set_delay")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetAudioDelay(IntPtr playerObject, long channel);

        /// <summary>
        /// Get current software audio volume.
        /// </summary>
        [InteropFunction("libvlc_audio_get_volume")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetVolume(IntPtr playerObject);

        /// <summary>
        /// Set current software audio volume.
        /// </summary>
        [InteropFunction("libvlc_audio_set_volume")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetVolume(IntPtr playerObject, int volume);

        /// <summary>
        /// Set current mute status.
        /// </summary>
        [InteropFunction("libvlc_audio_set_mute")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetMute(IntPtr playerObject, int status);

        /// <summary>
        /// Get current mute status.
        /// </summary>
        [InteropFunction("libvlc_audio_get_mute")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int IsMute(IntPtr playerObject);

        /// <summary>
        /// Get the list of available audio outputs.
        /// </summary>
        [InteropFunction("libvlc_audio_output_list_get")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetAudioOutputsDescriptions(IntPtr instance);

        /// <summary>
        /// It will release the list of available audio outputs.
        /// </summary>
        [InteropFunction("libvlc_audio_output_list_release")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ReleaseAudioOutputDescription(IntPtr audioOutput);

        /// <summary>
        /// Set the audio output. Change will be applied after stop and play.
        /// </summary>
        [InteropFunction("libvlc_audio_output_set")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetAudioOutput(IntPtr playerObject, IntPtr audioOutputName);

        /// <summary>
        ///  Set audio output device. Changes are only effective after stop and play.
        /// </summary>
        [InteropFunction("libvlc_audio_output_device_set")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetAudioOutputDevice(IntPtr playerObject, [MarshalAs(UnmanagedType.LPStr)] string audioOutputName, [MarshalAs(UnmanagedType.LPStr)] string deviceName);

        /// <summary>
        ///  Get audio output devices list.
        /// </summary>
        [InteropFunction("libvlc_audio_output_device_list_get")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetAudioOutputDeviceList(IntPtr playerObject, [MarshalAs(UnmanagedType.LPStr)] string aout);

        /// <summary>
        ///  Release audio output devices list.
        /// </summary>
        [InteropFunction("libvlc_audio_output_device_list_release")]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr ReleaseAudioOutputDeviceList(IntPtr p_list);
        #endregion
        #endregion

        public WrapperStandalone(PlayerOptionsStandalone options)
        {
            var settings = UMPSettings.GetSettings();

            // Additional loading of 'libVLCCore' library for correct work of main library
            InteropLibraryLoader.Load(UMPSettings.LIB_VLC_CORE_NAME, settings.UseExternalLibs, settings.AdditionalLibsPath);

            _libVLCHandler = InteropLibraryLoader.Load(UMPSettings.LIB_VLC_NAME, settings.UseExternalLibs, settings.AdditionalLibsPath);

            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                _libUMPHandler = InteropLibraryLoader.Load(UMPSettings.ASSET_NAME, false, string.Empty);

            _manageLogCallback = DebugLogHandler;
            NativeSetUnityLogMessageCallback(Marshal.GetFunctionPointerForDelegate(_manageLogCallback));

            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                _nativeIndex = InteropLibraryLoader.GetInteropDelegate<UMPNativeInitDel>(_libUMPHandler).Invoke();
            else
                _nativeIndex = UMPNativeInit();
        }

        private void DebugLogHandler(string msg)
        {
            UnityEngine.Debug.LogError(msg);
        }

        #region Native
        public int NativeIndex
        {
            get
            {
                return _nativeIndex;
            }
        }

        public void NativeUpdateIndex()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                InteropLibraryLoader.GetInteropDelegate<UMPNativeUpdateIndexDel>(_libUMPHandler).Invoke(_nativeIndex);
            else
                UMPNativeUpdateIndex(_nativeIndex);
        }

        public void NativeSetTexture(IntPtr texture)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                InteropLibraryLoader.GetInteropDelegate<UMPNativeSetTextureDel>(_libUMPHandler).Invoke(_nativeIndex, texture);
            else
                UMPNativeSetTexture(_nativeIndex, texture);
        }

        public void NativeSetPixelsBuffer(IntPtr buffer, int width, int height)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                InteropLibraryLoader.GetInteropDelegate<UMPNativeSetPixelsBufferDel>(_libUMPHandler).Invoke(_nativeIndex, buffer, width, height);
            else
                UMPNativeSetPixelsBuffer(_nativeIndex, buffer, width, height);
        }

        public IntPtr NativeGetPixelsBuffer()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetPixelsBufferDel>(_libUMPHandler).Invoke(_nativeIndex);

            return UMPNativeGetPixelsBuffer(_nativeIndex);
        }

        public int NativeGetPixelsBufferWidth()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetPixelsBufferWidthDel>(_libUMPHandler).Invoke(_nativeIndex);

            return UMPNativeGetPixelsBufferWidth(_nativeIndex);
        }

        public int NativeGetPixelsBufferHeight()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetPixelsBufferHeightDel>(_libUMPHandler).Invoke(_nativeIndex);

            return UMPNativeGetPixelsBufferHeight(_nativeIndex);
        }

        public void NativePixelsBufferRelease()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                InteropLibraryLoader.GetInteropDelegate<UMPNativePixelsBufferReleaseDel>(_libUMPHandler).Invoke(_nativeIndex);
            else
                UMPNativePixelsBufferRelease(_nativeIndex);
        }

        public string NativeGetLogMessage()
        {
            IntPtr value = IntPtr.Zero;

            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                value = InteropLibraryLoader.GetInteropDelegate<UMPNativeGetLogMessageDel>(_libUMPHandler).Invoke(_nativeIndex);
            else
                value = UMPNativeGetLogMessage(_nativeIndex);

            return value != IntPtr.Zero ? Marshal.PtrToStringAnsi(value) : null;
        }

        public int NativeGetLogLevel()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetLogLevelDel>(_libUMPHandler).Invoke(_nativeIndex);

            return UMPNativeGetLogLevel(_nativeIndex);
        }

        public int NativeGetState()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetStateDel>(_libUMPHandler).Invoke(_nativeIndex);

            return UMPNativeGetState(_nativeIndex);
        }

        private float NativeGetStateFloatValue()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetStateFloatValueDel>(_libUMPHandler).Invoke(_nativeIndex);

            return UMPNativeGetStateFloatValue(_nativeIndex);
        }

        private long NativeGetStateLongValue()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetStateLongValueDel>(_libUMPHandler).Invoke(_nativeIndex);

            return UMPNativeGetStateLongValue(_nativeIndex);
        }

        private string NativeGetStateStringValue()
        {
            IntPtr value = IntPtr.Zero;

            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                value = InteropLibraryLoader.GetInteropDelegate<UMPNativeGetStateStringValueDel>(_libUMPHandler).Invoke(_nativeIndex);
            else
                value = UMPNativeGetStateStringValue(_nativeIndex);

            return value != IntPtr.Zero ? Marshal.PtrToStringAnsi(value) : null;
        }

        public void NativeSetPixelsVerticalFlip(bool flip)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                InteropLibraryLoader.GetInteropDelegate<UMPNativeSetPixelsVerticalFlipDel>(_libUMPHandler).Invoke(_nativeIndex, flip);
            else
                UMPNativeSetPixelsVerticalFlip(_nativeIndex, flip);
        }

        public void NativeSetAudioParams(int numChannels, int sampleRate)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                InteropLibraryLoader.GetInteropDelegate<UMPNativeSetAudioParamsDel>(_libUMPHandler).Invoke(_nativeIndex, numChannels, sampleRate);
            else
                UMPNativeSetAudioParams(_nativeIndex, numChannels, sampleRate);
        }

        public string NativeGetAudioParams(char separator)
        {
            IntPtr value = IntPtr.Zero;

            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                value = InteropLibraryLoader.GetInteropDelegate<UMPNativeGetAudioParamsDel>(_libUMPHandler).Invoke(_nativeIndex, separator);
            else
                value = UMPNativeGetAudioParams(_nativeIndex, separator);

            return value != IntPtr.Zero ? Marshal.PtrToStringAnsi(value) : null;
        }

        public int NativeGetAudioSamples(IntPtr decodedSamples, int samplesLength, AudioOutput.AudioChannels audioChannel)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetAudioSamplesDel>(_libUMPHandler).Invoke(_nativeIndex, decodedSamples, samplesLength, audioChannel);

            return UMPNativeGetAudioSamples(_nativeIndex, decodedSamples, samplesLength, audioChannel);
        }

        public bool NativeClearAudioSamples(int samplesLength)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeClearAudioSamplesDel>(_libUMPHandler).Invoke(_nativeIndex, samplesLength);

            return UMPNativeClearAudioSamples(_nativeIndex, samplesLength);
        }

        public IntPtr NativeGetUnityRenderCallback()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetUnityRenderCallbackDel>(_libUMPHandler).Invoke();

            return UMPNativeGetUnityRenderCallback();
        }

        public IntPtr NativeGetVideoLockCallback()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetVideoLockCallbackDel>(_libUMPHandler).Invoke();

            return UMPNativeGetVideoLockCallback();
        }

        public IntPtr NativeGetVideoDisplayCallback()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetVideoDisplayCallbackDel>(_libUMPHandler).Invoke();

            return UMPNativeGetVideoDisplayCallback();
        }

        public IntPtr NativeGetVideoFormatCallback()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetVideoFormatCallbackDel>(_libUMPHandler).Invoke();

            return UMPNativeGetVideoFormatCallback();
        }

        public IntPtr NativeGetVideoCleanupCallback()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetVideoCleanupCallbackDel>(_libUMPHandler).Invoke();

            return UMPNativeGetVideoCleanupCallback();
        }

        public IntPtr NativeGetAudioSetupCallback()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetAudioSetupCallbackDel>(_libUMPHandler).Invoke();

            return UMPNativeGetAudioSetupCallback();
        }

        public IntPtr NativeGetAudioPlayCallback()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetAudioPlayCallbackDel>(_libUMPHandler).Invoke();

            return UMPNativeGetAudioPlayCallback();
        }

        public IntPtr NativeMediaPlayerEventCallback()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeMediaPlayerEventCallbackDel>(_libUMPHandler).Invoke();

            return UMPNativeMediaPlayerEventCallback();
        }

        public IntPtr NativeGetLogMessageCallback()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetLogMessageCallbackDel>(_libUMPHandler).Invoke();

            return UMPNativeGetLogMessageCallback();
        }

        public void NativeSetUnityLogMessageCallback(IntPtr callback)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                InteropLibraryLoader.GetInteropDelegate<UMPNativeSetUnityLogMessageCallbackDel>(_libUMPHandler).Invoke(callback);
            else
                UMPNativeSetUnityLogMessageCallback(callback);
        }

        public void NativeUpdateFramesCounter(int counter)
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                InteropLibraryLoader.GetInteropDelegate<UMPNativeUpdateFramesCounterCallbackDel>(_libUMPHandler).Invoke(_nativeIndex, counter);
            else
                UMPNativeUpdateFramesCounter(_nativeIndex, counter);
        }

        public int NativeGetFramesCounter()
        {
            if (UMPSettings.SupportedPlatform == UMPSettings.Platforms.Linux)
                return InteropLibraryLoader.GetInteropDelegate<UMPNativeGetFramesCounterCallbackDel>(_libUMPHandler).Invoke(_nativeIndex);

            return UMPNativeGetFramesCounter(_nativeIndex);
        }
        #endregion

        #region Player
        public void PlayerSetDataSource(string path, object playerObject = null)
        {
        }

        public bool PlayerPlay(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<Play>(_libVLCHandler).Invoke((IntPtr)playerObject) == 0;
        }

        public void PlayerPause(object playerObject)
        {
            InteropLibraryLoader.GetInteropDelegate<Pause>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public void PlayerStop(object playerObject)
        {
            InteropLibraryLoader.GetInteropDelegate<Stop>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public void PlayerRelease(object playerObject)
        {
            InteropLibraryLoader.GetInteropDelegate<ReleaseMediaPlayer>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public bool PlayerIsPlaying(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<IsPlaying>(_libVLCHandler).Invoke((IntPtr)playerObject) == 1;
        }

        public bool PlayerWillPlay(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<CouldPlay>(_libVLCHandler).Invoke((IntPtr)playerObject) == 1;
        }

        public long PlayerGetLength(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetLength>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public float PlayerGetBufferingPercentage(object playerObject)
        {
            return 0;
        }

        public long PlayerGetTime(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetTime>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public void PlayerSetTime(long time, object playerObject)
        {
            InteropLibraryLoader.GetInteropDelegate<SetTime>(_libVLCHandler).Invoke((IntPtr)playerObject, time);
        }

        public float PlayerGetPosition(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetMediaPosition>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public void PlayerSetPosition(float pos, object playerObject)
        {
            InteropLibraryLoader.GetInteropDelegate<SetMediaPosition>(_libVLCHandler).Invoke((IntPtr)playerObject, pos);
        }

        public float PlayerGetRate(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetRate>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public bool PlayerSetRate(float rate, object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<SetRate>(_libVLCHandler).Invoke((IntPtr)playerObject, rate) == 0;
        }

        public int PlayerGetVolume(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetVolume>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public int PlayerSetVolume(int volume, object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<SetVolume>(_libVLCHandler).Invoke((IntPtr)playerObject, volume);
        }

        public bool PlayerGetMute(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<IsMute>(_libVLCHandler).Invoke((IntPtr)playerObject) == 1;
        }

        public void PlayerSetMute(bool mute, object playerObject)
        {
            InteropLibraryLoader.GetInteropDelegate<SetMute>(_libVLCHandler).Invoke((IntPtr)playerObject, mute ? 1 : 0);
        }

        public int PlayerVideoWidth(object playerObject)
        {
            uint width = 0, height = 0;
            InteropLibraryLoader.GetInteropDelegate<GetVideoSize>(_libVLCHandler).Invoke((IntPtr)playerObject, 0, out width, out height);
            return (int)width;
        }

        public int PlayerVideoHeight(object playerObject)
        {
            uint width = 0, height = 0;
            InteropLibraryLoader.GetInteropDelegate<GetVideoSize>(_libVLCHandler).Invoke((IntPtr)playerObject, 0, out width, out height);
            return (int)height;
        }

        public int PlayerVideoFramesCounter(object playerObject)
        {
            return 0;
        }
        
        public PlayerState PlayerGetState()
        {
            return (PlayerState)NativeGetState();
        }

        public object PlayerGetStateValue()
        {
            object value = NativeGetStateFloatValue();

            if ((float)value < 0)
            {
                value = NativeGetStateLongValue();

                if ((long)value < 0)
                {
                    value = NativeGetStateStringValue();
                }
            }

            return value;
        }
        #endregion

        #region Player Spu
        public MediaTrackInfo[] PlayerSpuGetTracks(object playerObject)
        {
            var tracks = new System.Collections.Generic.List<MediaTrackInfo>();
            var tracksCount = InteropLibraryLoader.GetInteropDelegate<GetVideoSpuCount>(_libVLCHandler).Invoke((IntPtr)playerObject);
            var tracksHandler = InteropLibraryLoader.GetInteropDelegate<GetVideoSpuDescription>(_libVLCHandler).Invoke((IntPtr)playerObject);

            for (int i = 0; i < tracksCount; i++)
            {
                if (tracksHandler != IntPtr.Zero)
                {
                    var track = (TrackDescription)Marshal.PtrToStructure(tracksHandler, typeof(TrackDescription));
                    tracks.Add(new MediaTrackInfo(track.Id, track.Name));
                    tracksHandler = track.NextDescription;
                }
            }

            return tracks.ToArray();
        }

        public int PlayerSpuGetTrack(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetVideoSpu>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public int PlayerSpuSetTrack(int spuIndex, object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<SetVideoSpu>(_libVLCHandler).Invoke((IntPtr)playerObject, spuIndex);
        }
        #endregion

        #region Player Audio
        public MediaTrackInfo[] PlayerAudioGetTracks(object playerObject)
        {
            var tracks = new System.Collections.Generic.List<MediaTrackInfo>();
            var tracksCount = InteropLibraryLoader.GetInteropDelegate<GetAudioTracksCount>(_libVLCHandler).Invoke((IntPtr)playerObject);
            var tracksHandler = InteropLibraryLoader.GetInteropDelegate<GetAudioTracksDescriptions>(_libVLCHandler).Invoke((IntPtr)playerObject);

            for (int i = 0; i < tracksCount; i++)
            {
                if (tracksHandler != IntPtr.Zero)
                {
                    var track = (TrackDescription)Marshal.PtrToStructure(tracksHandler, typeof(TrackDescription));
                    tracks.Add(new MediaTrackInfo(track.Id, track.Name));
                    tracksHandler = track.NextDescription;
                }
            }

            //InteropLibraryLoader.GetInteropDelegate<ReleaseTracksDescriptions>(_libVLCHandler).Invoke(tracksHandler);

            return tracks.ToArray();
        }

        public int PlayerAudioGetTrack(object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetAudioTrack>(_libVLCHandler).Invoke((IntPtr)playerObject);
        }

        public int PlayerAudioSetTrack(int audioIndex, object playerObject)
        {
            return InteropLibraryLoader.GetInteropDelegate<SetAudioTrack>(_libVLCHandler).Invoke((IntPtr)playerObject, audioIndex);
        }
        #endregion

        #region Platform dependent functionality
        public IntPtr ExpandedLibVLCNew(string[] args)
        {
            if (args == null)
                args = new string[0];

            return InteropLibraryLoader.GetInteropDelegate<CreateNewInstance>(_libVLCHandler).Invoke(args.Length, args);
        }

        public void ExpandedLibVLCRelease(IntPtr libVLCInst)
        {
            InteropLibraryLoader.GetInteropDelegate<ReleaseInstance>(_libVLCHandler).Invoke(libVLCInst);
        }

        public IntPtr ExpandedMediaNewLocation(IntPtr libVLCInst, string path)
        {
            return InteropLibraryLoader.GetInteropDelegate<CreateNewMediaFromLocation>(_libVLCHandler).Invoke(libVLCInst, path);
        }

        public void ExpandedSetMedia(IntPtr mpInstance, IntPtr libVLCMediaInst)
        {
            InteropLibraryLoader.GetInteropDelegate<SetMediaToMediaPlayer>(_libVLCHandler).Invoke(mpInstance, libVLCMediaInst);
        }

        public void ExpandedAddOption(IntPtr libVLCMediaInst, string option)
        {
            InteropLibraryLoader.GetInteropDelegate<AddOptionToMedia>(_libVLCHandler).Invoke(libVLCMediaInst, option);
        }

        // TODO Move to IPlayerWrapper
        public void ExpandedMediaGetStats(IntPtr mpInstance, out MediaStats mediaStats)
        {
            InteropLibraryLoader.GetInteropDelegate<GetMediaStats>(_libVLCHandler).Invoke(mpInstance, out mediaStats);
        }

        // TODO Move to IPlayerWrapper
        public TrackInfo[] ExpandedMediaGetTracksInfo(IntPtr mpInstance)
        {
            IntPtr result_buffer = new IntPtr();
            int trackCount = InteropLibraryLoader.GetInteropDelegate<GetMediaTracksInformations>(_libVLCHandler).Invoke(mpInstance, out result_buffer);

            if (trackCount < 0)
                return null;

            IntPtr buffer = result_buffer;
            var tracks = new TrackInfo[trackCount];

            for (int i = 0; i < tracks.Length; i++)
            {
                tracks[i] = (TrackInfo)Marshal.PtrToStructure(buffer, typeof(TrackInfo));
                buffer = new IntPtr(buffer.ToInt64() + Marshal.SizeOf(typeof(TrackInfo)));
            }
            ExpandedFree(result_buffer);
            return tracks;
        }

        public void ExpandedMediaRelease(IntPtr libVLCMediaInst)
        {
            InteropLibraryLoader.GetInteropDelegate<ReleaseMedia>(_libVLCHandler).Invoke(libVLCMediaInst);
        }

        public IntPtr ExpandedMediaPlayerNew(IntPtr libVLCInst)
        {
            return InteropLibraryLoader.GetInteropDelegate<CreateMediaPlayer>(_libVLCHandler).Invoke(libVLCInst);
        }

        public void ExpandedFree(IntPtr instance)
        {
            InteropLibraryLoader.GetInteropDelegate<Free>(_libVLCHandler).Invoke(instance);
        }

        public IntPtr ExpandedEventManager(IntPtr mpInstance)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetMediaPlayerEventManager>(_libVLCHandler).Invoke(mpInstance);
        }

        public int ExpandedEventAttach(IntPtr eventManagerInst, EventTypes eventType, IntPtr callback, IntPtr userData)
        {
            return InteropLibraryLoader.GetInteropDelegate<AttachEvent>(_libVLCHandler).Invoke(eventManagerInst, eventType, callback, userData);
        }

        public void ExpandedEventDetach(IntPtr eventManagerInst, EventTypes eventType, IntPtr callback, IntPtr userData)
        {
            InteropLibraryLoader.GetInteropDelegate<DetachEvent>(_libVLCHandler).Invoke(eventManagerInst, eventType, callback, userData);
        }

        public void ExpandedLogSet(IntPtr libVLC, IntPtr callback, IntPtr data)
        {
            InteropLibraryLoader.GetInteropDelegate<LogSet>(_libVLCHandler).Invoke(libVLC, callback, data);
        }

        public void ExpandedLogUnset(IntPtr libVLC)
        {
            InteropLibraryLoader.GetInteropDelegate<LogUnset>(_libVLCHandler).Invoke(libVLC);
        }

        public void ExpandedVideoSetCallbacks(IntPtr mpInstance, IntPtr @lock, IntPtr unlock, IntPtr display, IntPtr opaque)
        {
            InteropLibraryLoader.GetInteropDelegate<SetVideoCallbacks>(_libVLCHandler).Invoke(mpInstance, @lock, unlock, display, opaque);
        }

        public void ExpandedVideoSetFormatCallbacks(IntPtr mpInstance, IntPtr setup, IntPtr cleanup)
        {
            InteropLibraryLoader.GetInteropDelegate<SetVideoFormatCallbacks>(_libVLCHandler).Invoke(mpInstance, setup, cleanup);
        }

        public void ExpandedVideoSetFormat(IntPtr mpInstance, string chroma, uint width, uint height, uint pitch)
        {
            InteropLibraryLoader.GetInteropDelegate<SetVideoFormat>(_libVLCHandler).Invoke(mpInstance, chroma, width, height, pitch);
        }

        public void ExpandedAudioSetCallbacks(IntPtr mpInstance, IntPtr play, IntPtr pause, IntPtr resume, IntPtr flush, IntPtr drain, IntPtr opaque)
        {
            InteropLibraryLoader.GetInteropDelegate<SetAudioCallbacks>(_libVLCHandler).Invoke(mpInstance, play, pause, resume, flush, drain, opaque);
        }

        public void ExpandedAudioSetFormatCallbacks(IntPtr mpInstance, IntPtr setup, IntPtr cleanup)
        {
            InteropLibraryLoader.GetInteropDelegate<SetAudioFormatCallbacks>(_libVLCHandler).Invoke(mpInstance, setup, cleanup);
        }

        public void ExpandedAudioSetFormat(IntPtr mpInstance, string format, int rate, int channels)
        {
            InteropLibraryLoader.GetInteropDelegate<SetAudioFormat>(_libVLCHandler).Invoke(mpInstance, format, rate, channels);
        }

        public long ExpandedGetAudioDelay(IntPtr mpInstance)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetAudioDelay>(_libVLCHandler).Invoke(mpInstance);
        }

        public void ExpandedSetAudioDelay(IntPtr mpInstance, long channel)
        {
            InteropLibraryLoader.GetInteropDelegate<SetAudioDelay>(_libVLCHandler).Invoke(mpInstance, channel);
        }

        public int ExpandedAudioOutputSet(IntPtr mpInstance, string psz_name)
        {
            return InteropLibraryLoader.GetInteropDelegate<SetAudioOutput>(_libVLCHandler).Invoke(mpInstance, Marshal.StringToHGlobalAnsi(psz_name));
        }

        public IntPtr ExpandedAudioOutputListGet(IntPtr mpInstance)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetAudioOutputsDescriptions>(_libVLCHandler).Invoke(mpInstance);
        }

        public void ExpandedAudioOutputListRelease(IntPtr outputListInst)
        {
            InteropLibraryLoader.GetInteropDelegate<ReleaseAudioOutputDescription>(_libVLCHandler).Invoke(outputListInst);
        }

        public void ExpandedAudioOutputDeviceSet(IntPtr mpInstance, string psz_audio_output, string psz_device_id)
        {
            InteropLibraryLoader.GetInteropDelegate<SetAudioOutputDevice>(_libVLCHandler).Invoke(mpInstance, psz_audio_output, psz_device_id);
        }

        public IntPtr ExpandedAudioOutputDeviceListGet(IntPtr mpInstance, string aout)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetAudioOutputDeviceList>(_libVLCHandler).Invoke(mpInstance, aout);
        }

        public void ExpandedAudioOutputDeviceListRelease(IntPtr deviceListInst)
        {
            InteropLibraryLoader.GetInteropDelegate<ReleaseAudioOutputDeviceList>(_libVLCHandler).Invoke(deviceListInst);
        }

        public int ExpandedSpuSetFile(IntPtr mpInstance, string path)
        {
            return InteropLibraryLoader.GetInteropDelegate<SetVideoSubtitleFile>(_libVLCHandler).Invoke(mpInstance, path);
        }

        public float ExpandedVideoGetScale(IntPtr mpInstance)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetVideoScale>(_libVLCHandler).Invoke(mpInstance);
        }

        public void ExpandedVideoSetScale(IntPtr mpInstance, float factor)
        {
            InteropLibraryLoader.GetInteropDelegate<SetVideoScale>(_libVLCHandler).Invoke(mpInstance, factor);
        }

        public void ExpandedVideoTakeSnapshot(IntPtr mpInstance, uint stream, string filePath, uint width, uint height)
        {
            InteropLibraryLoader.GetInteropDelegate<TakeSnapshot>(_libVLCHandler).Invoke(mpInstance, stream, filePath, width, height);
        }

        public float ExpandedVideoFrameRate(IntPtr mpInstance)
        {
            return InteropLibraryLoader.GetInteropDelegate<GetFramesPerSecond>(_libVLCHandler).Invoke(mpInstance);
        }
        #endregion
    }
}
