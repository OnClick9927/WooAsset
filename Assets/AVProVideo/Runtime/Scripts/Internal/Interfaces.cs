using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
    public interface IMediaPlayer
    {
        void OnEnable();
        void Update();
        void EndUpdate();
        void Render();
        IntPtr GetNativePlayerHandle();
    }

    /// <summary>
    /// Interface for side loading of subtitles in SRT format
    /// </summary>
    public interface IMediaSubtitles
    {
        bool LoadSubtitlesSRT(string data);
        int GetSubtitleIndex();
        string GetSubtitleText();
    }

    public enum BufferedFrameSelectionMode : int
    {
        // No buffering, just selects the latest decoded frame
        None = 0,

        // Selects the newest buffered frame, and displays it until a newer frame is available
        NewestFrame = 10,

        // Selects the oldest buffered frame, and displays it until a newer frame is available
        OldestFrame = 11,

        // Selects the next buffered frame, and displays it until the number of buffered frames changes
        MediaClock = 20,

        // Uses Time.deltaTime to keep a clock which is used to select the buffered frame
        ElapsedTime = 30,

        // Uses VSync delta time to keep a clock which is used to select the buffered frame
        // Time.deltaTime is used to calculate the number of vsyncs that have elapsed
        ElapsedTimeVsynced = 40,

        // Selects the buffered frame corresponding to the external timeStamp (useful for frame-syncing players)
        FromExternalTime = 50,

        // Selects the closest buffered frame corresponding to the external timeStamp (useful for frame-syncing players)
        FromExternalTimeClosest = 51,
    }

    /// <summary>
    /// Interface for buffering frames for more control over the timing of their display
    /// </summary>
    public interface IBufferedDisplay
    {
        /// <summary>
        /// We need to manually call UpdateBufferedDisplay() in the case of master-slave synced playback so that master is updated before slaves
        /// </summary>
        long UpdateBufferedDisplay();

        BufferedFramesState GetBufferedFramesState();

        void SetSlaves(IBufferedDisplay[] slaves);

        void SetBufferedDisplayMode(BufferedFrameSelectionMode mode, IBufferedDisplay master = null);

        void SetBufferedDisplayOptions(bool pauseOnPrerollComplete);
    }

    public interface IMediaControl
    {
        /// <summary>
        /// Be careful using this method directly.  It is best to instead use the OpenMedia() method in the MediaPlayer component as this will set up the events correctly and also perform other checks
        /// customHttpHeaders is in the format "key1:value1\r\nkey2:value2\r\n"=
        /// </summary>
        bool OpenMedia(string path, long offset, string customHttpHeaders, MediaHints mediahints, int forceFileFormat = 0, bool startWithHighestBitrate = false);
        bool OpenMediaFromBuffer(byte[] buffer);
        bool StartOpenMediaFromBuffer(ulong length);
        bool AddChunkToMediaBuffer(byte[] chunk, ulong offset, ulong length);
        bool EndOpenMediaFromBuffer();

#if NETFX_CORE
		bool	OpenMedia(Windows.Storage.Streams.IRandomAccessStream ras, string path, long offset, string customHttpHeaders);
#endif

        void CloseMedia();

        void SetLooping(bool bLooping);
        bool IsLooping();

        bool HasMetaData();
        bool CanPlay();
        bool IsPlaying();
        bool IsSeeking();
        bool IsPaused();
        bool IsFinished();
        bool IsBuffering();

        void Play();
        void Pause();
        void Stop();
        void Rewind();

        /// <summary>
        /// The time in seconds seeked will be to the exact time
        /// This can take a long time is the keyframes are far apart
        /// Some platforms don't support this and instead seek to the closest keyframe
        /// </summary>
        void Seek(double time);

        /// <summary>
        /// The time in seconds seeked will be to the closest keyframe
        /// </summary>
        void SeekFast(double time);

        /// <summary>
        /// The time in seconds seeked to will be within the range [time-timeDeltaBefore, time+timeDeltaAfter] for efficiency.
        /// Only supported on macOS, iOS and tvOS.
        /// Other platforms will automatically pass through to Seek()
        /// </summary>
        void SeekWithTolerance(double time, double timeDeltaBefore, double timeDeltaAfter);

        /// <summary>
        /// Seek to a specific frame, range is [0, GetMaxFrameNumber()]
        /// NOTE: For best results the video should be encoded as keyframes only
        /// and have no audio track, or an audio track with the same length as the video track
        /// </summary>
        void SeekToFrame(int frame, float overrideFrameRate = 0f);

        /// <summary>
        /// Seek forwards or backwards relative to the current frame
        /// NOTE: For best results the video should be encoded as keyframes only
        /// and have no audio track, or an audio track with the same length as the video track
        /// </summary>
        void SeekToFrameRelative(int frameOffset, float overrideFrameRate = 0f);

        /// <summary>
        /// Returns the current video time in seconds
        /// </summary>
        double GetCurrentTime();

        /// <summary>
        /// Returns the current video time in frames, range is [0, GetMaxFrameNumber()]
        /// NOTE: For best results the video should be encoded as keyframes only
        /// and have no audio track, or an audio track with the same length as the video track
        /// </summary>
        int GetCurrentTimeFrames(float overrideFrameRate = 0f);

        /// <summary>
        /// Returns the current video date and time usually from the
        /// EXT-X-PROGRAM-DATE-TIME tag on HLS streams
        /// Only supported on macOS, iOS, tvOS and Android (using ExoPlayer API)
        /// And Windows 10 using WinRT API
        /// </summary>
        System.DateTime GetProgramDateTime();

        float GetPlaybackRate();
        void SetPlaybackRate(float rate);

        void MuteAudio(bool bMute);
        bool IsMuted();
        void SetVolume(float volume);
        void SetBalance(float balance);
        float GetVolume();
        float GetBalance();

        /*int		GetCurrentVideoTrack();
		void	SetVideoTrack(int index);

		int		GetCurrentAudioTrack();
		void	SetAudioTrack(int index);*/

        /// <summary>
        /// Returns a range of time values that can be seeked in seconds
        /// </summary>
        TimeRanges GetSeekableTimes();

        /// <summary>
        /// Returns a range of time values that contain fully downloaded segments,
        /// which can be seeked to immediately without requiring additional downloading
        /// </summary>
        TimeRanges GetBufferedTimes();

        ErrorCode GetLastError();
        long GetLastExtendedErrorCode();

        void SetTextureProperties(FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp, int anisoLevel = 1);

        // Audio Grabbing

        /// <summary>
        /// Copies the specified amount of audio into the buffer
        /// If the specified amount is not yet available then nothing no samples are copied
        /// The number of audio samples grabbed are returned
        /// </summary>
        int GrabAudio(float[] buffer, int sampleCount, int channelCount);
        int GetAudioBufferedSampleCount();
        int GetAudioChannelCount();
        AudioChannelMaskFlags GetAudioChannelMask();

        // Audio 360

        void SetAudioChannelMode(Audio360ChannelMode channelMode);
        void SetAudioHeadRotation(Quaternion q);
        void ResetAudioHeadRotation();
        void SetAudioFocusEnabled(bool enabled);
        void SetAudioFocusProperties(float offFocusLevel, float widthDegrees);
        void SetAudioFocusRotation(Quaternion q);
        void ResetAudioFocus();

        bool WaitForNextFrame(Camera dummyCamera, int previousFrameCount);

        [Obsolete("SetPlayWithoutBuffering has been deprecated, see platform specific options for how to enable playback without buffering (if supported).")]
        void SetPlayWithoutBuffering(bool playWithoutBuffering);

        // Encrypted stream support
        //void	SetKeyServerURL(string url);
        void SetKeyServerAuthToken(string token);
        void SetOverrideDecryptionKey(byte[] key);

        // External playback support.

        /// <summary>
        /// Check to see if external playback is currently active on the player.
        /// </summary>
        bool IsExternalPlaybackActive();

        /// <summary>
        /// Set whether the player is allowed to switch to external playback, e.g. AirPlay.
        /// </summary>
        void SetAllowsExternalPlayback(bool enable);

        /// <summary>
        /// Sets the video gravity of the player for external playback only.
        /// </summary>
        void SetExternalPlaybackVideoGravity(ExternalPlaybackVideoGravity gravity);
    }

    public interface IMediaInfo
    {
        /// <summary>
        /// Returns media duration in seconds
        /// </summary>
        double GetDuration();

        /// <summary>
        /// Returns media duration in frames
        /// NOTE: For best results the video should be encoded as keyframes only
        /// and have no audio track, or an audio track with the same length as the video track
        /// </summary>
        int GetDurationFrames(float overrideFrameRate = 0f);

        /// <summary>
        /// Returns highest frame number that can be seeked to
        /// NOTE: For best results the video should be encoded as keyframes only
        /// and have no audio track, or an audio track with the same length as the video track
        /// </summary>
        int GetMaxFrameNumber(float overrideFrameRate = 0f);

        /// <summary>
        /// Returns video width in pixels
        /// </summary>
        int GetVideoWidth();

        /// <summary>
        /// Returns video height in pixels
        /// </summary>
        int GetVideoHeight();

        /// <summary>
        /// Returns the frame rate of the media.
        /// </summary>
        float GetVideoFrameRate();

        /// <summary>
        /// Returns the current achieved display rate in frames per second
        /// </summary>
        float GetVideoDisplayRate();

        /// <summary>
        /// Returns true if the media has a visual track
        /// </summary>
        bool HasVideo();

        /// <summary>
        /// Returns true if the media has a audio track
        /// </summary>
        bool HasAudio();

        /// <summary>
        /// Returns the a description of which playback path is used internally.
        /// This can for example expose whether CPU or GPU decoding is being performed
        /// For Windows the available player descriptions are:
        ///		"DirectShow" - legacy Microsoft API but still very useful especially with modern filters such as LAV
        ///		"MF-MediaEngine-Software" - uses the Windows 8.1 features of the Microsoft Media Foundation API, but software decoding
        ///		"MF-MediaEngine-Hardware" - uses the Windows 8.1 features of the Microsoft Media Foundation API, but GPU decoding
        ///	Android has "MediaPlayer" and "ExoPlayer"
        ///	macOS / tvOS / iOS just has "AVFoundation"
        /// </summary>
        string GetPlayerDescription();

#if !AVPRO_NEW_GAMMA
        /// <summary>
        /// Whether this MediaPlayer instance supports linear color space
        /// If it doesn't then a correction may have to be made in the shader
        /// </summary>
        bool PlayerSupportsLinearColorSpace();
#endif

        /// <summary>
        /// Checks if the playback is in a stalled state
        /// </summary>
        bool IsPlaybackStalled();

        /// <summary>
        /// The affine transform of the texture as an array of six floats: a, b, c, d, tx, ty.
        /// </summary>
        float[] GetTextureTransform();

        /// <summary>
        /// Gets the estimated bandwidth used by all video players (in bits per second)
        /// Currently only supported on Android when using ExoPlayer API
        /// </summary>
        long GetEstimatedTotalBandwidthUsed();

        /*
		string GetMediaDescription();
		string GetVideoDescription();
		string GetAudioDescription();*/

        /// <summary>
        /// Checks if the media is compatible with external playback, for instance via AirPlay.
        /// </summary>
        bool IsExternalPlaybackSupported();

        // Internal method
        bool GetDecoderPerformance(ref int activeDecodeThreadCount, ref int decodedFrameCount, ref int droppedFrameCount);

        // Internal method
        PlaybackQualityStats GetPlaybackQualityStats();
    }

    #region MediaCaching

    /// <summary>Options for configuring media caching.</summary>
    public class MediaCachingOptions
    {
        /// <summary>The minimum bitrate of the media to cache in bits per second.</summary>
        public double minimumRequiredBitRate;

        /// <summary>The minimum resolution of the media to cache.</summary>
        /// <remark>Only supported on iOS 14 and later.</remark>
        public Vector2 minimumRequiredResolution;

        /// <summary>Human readable title for the cached media.</summary>
        /// <remark>iOS: This value will be displayed in the usage pane of the settings app.</remark>
        public string title;

        /// <summary>Optional artwork for the cached media in PNG format.</summary>
        /// <remark>iOS: This value will be displayed in the usage pane of the settings app.</remark>
        public byte[] artwork;
    }

    /// <summary>Status of the media item in the cache.</summary>
    public enum CachedMediaStatus : int
    {
        /// <summary>The media has not been cached.</summary>
        NotCached,
        /// <summary>The media is being cached.</summary>
        Caching,
        /// <summary>The media is cached.</summary>
        Cached,
        /// <summary>The media is not cached, something went wrong - check the log.</summary>
        Failed
    }

    /// <summary>Interface for the media cache.</summary>
    public interface IMediaCache
    {
        /// <summary>Test to see if the player can cache media.</summary>
        /// <returns>True if media caching is supported.</returns>
        bool IsMediaCachingSupported();

        /// <summary>Cache the media specified by url.</summary>
        /// <param name="url">The url of the media.</param>
        /// <param name="headers"></param>
        /// <param name="options"></param>
        void AddMediaToCache(string url, string headers = null, MediaCachingOptions options = null);

        /// <summary>Cancels the download of the media specified by url.</summary>
        /// <param name="url">The url of the media.</param>
        void CancelDownloadOfMediaToCache(string url);

        /// <summary>Remove the cached media specified by url.</summary>
        /// <param name="url">The url of the media.</param>
        void RemoveMediaFromCache(string url);

        /// <summary>Get the cached status for the media specified.</summary>
        /// <param name="url">The url of the media.</param>
        /// <param name="progress">The amount of the media that has been cached in the range [0...1].</param>
        /// <returns>The status of the media.</returns>
        CachedMediaStatus GetCachedMediaStatus(string url, ref float progress);

        /// <summary>Test if the currently open media is cached.</summary>
        /// <returns>True if the media is cached, false otherwise.</returns>
        bool IsMediaCached();
    }

    #endregion

    public interface ITextureProducer
    {
        /// <summary>
        /// Gets the number of textures produced by the media player.
        /// </summary>
        int GetTextureCount();

        /// <summary>
        /// Returns the Unity texture containing the current frame image.
        /// The texture pointer will return null while the video is loading
        /// This texture usually remains the same for the duration of the video.
        /// There are cases when this texture can change, for instance: if the graphics device is recreated,
        /// a new video is loaded, or if an adaptive stream (eg HLS) is used and it switches video streams.
        /// </summary>
        Texture GetTexture(int index = 0);

        /// <summary>
        /// Returns a count of how many times the texture has been updated
        /// </summary>
        int GetTextureFrameCount();

        /// <summary>
        /// Returns whether this platform supports counting the number of times the texture has been updated
        /// </summary>
        bool SupportsTextureFrameCount();

        /// <summary>
        /// Returns the presentation time stamp of the current texture
        /// </summary>
        long GetTextureTimeStamp();

        /// <summary>
        /// Returns true if the image on the texture is upside-down
        /// </summary>
        bool RequiresVerticalFlip();

        /// <summary>
        /// Returns the type of packing used for stereo content
        /// </summary>
        StereoPacking GetTextureStereoPacking();

        /// <summary>
        /// Returns the whether the texture has transparency
        /// </summary>
        TransparencyMode GetTextureTransparency();

        /// <summary>
        /// Returns the type of packing used for alpha content
        /// </summary>
        AlphaPacking GetTextureAlphaPacking();

        /// <summary>
        /// Returns the current transformation required to convert from YpCbCr to RGB colorspaces.
        /// </summary>
        Matrix4x4 GetYpCbCrTransform();

#if AVPRO_NEW_GAMMA
		/// <summary>
		/// Returns the gamma type of a sampled pixel
		/// Is the texture returns samples in linear gamma then no conversion is need when using Unity's linear color space mode
		/// If it doesn't then a correction may have to be made in the shader
		/// </summary>
		TextureGamma GetTextureSampleGamma();

		bool TextureRequiresGammaConversion();
#endif
    }

    public enum Platform
    {
        Windows,
        MacOSX,
        iOS,
        tvOS,
        Android,
        WindowsUWP,
        WebGL,
        Count = 7,
        Unknown = 100,
    }

    public enum MediaSource
    {
        Reference,
        Path,
    }

    public enum MediaPathType
    {
        AbsolutePathOrURL,
        RelativeToProjectFolder,
        RelativeToStreamingAssetsFolder,
        RelativeToDataFolder,
        RelativeToPersistentDataFolder,
    }

    [System.Serializable]
    public class MediaPath
    {
        [SerializeField] MediaPathType _pathType = MediaPathType.RelativeToStreamingAssetsFolder;
        public MediaPathType PathType { get { return _pathType; } set { _pathType = value; } }

        [SerializeField] string _path = string.Empty;
        public string Path { get { return _path; } set { _path = value; } }

        public MediaPath()
        {
            _pathType = MediaPathType.RelativeToStreamingAssetsFolder;
            _path = string.Empty;
        }
        public MediaPath(MediaPath copy)
        {
            _pathType = copy.PathType;
            _path = copy.Path;
        }
        public MediaPath(string path, MediaPathType pathType)
        {
            _pathType = pathType;
            _path = path;
        }

        public string GetResolvedFullPath()
        {
            string result = Helper.GetFilePath(_path, _pathType);

#if (UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN))
            if (result.Length > 200 && !result.Contains("://"))
            {
                result = Helper.ConvertLongPathToShortDOS83Path(result);
            }
#endif

            return result;
        }

        public static bool operator ==(MediaPath a, MediaPath b)
        {
            if ((object)a == null)
                return (object)b == null;

            return a.Equals(b);
        }
        public static bool operator !=(MediaPath a, MediaPath b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var a = (MediaPath)obj;
            return (_pathType == a._pathType && _path == a._path);
        }

        public override int GetHashCode()
        {
            return _pathType.GetHashCode() ^ _path.GetHashCode();
        }
    }

    public enum OverrideMode
    {
        None,                       // No overide, just use internal logic
        Override,                   // Manually override
    }

    public enum TextureGamma
    {
        SRGB,
        Linear,

        // Future HDR support
        // PQ,
        // HLG,
    }

    public enum StereoPacking : int
    {
        None = 0,                   // Monoscopic
        TopBottom = 1,              // Top is the left eye, bottom is the right eye
        LeftRight = 2,              // Left is the left eye, right is the right eye
        CustomUV = 3,               // Use the mesh UV to unpack, uv0=left eye, uv1=right eye
        TwoTextures = 4,            // First texture left eye, second texture is right eye
        Unknown = 10,
    }

    [System.Serializable]
    public struct MediaHints
    {
        public TransparencyMode transparency;
        public AlphaPacking alphaPacking;
        public StereoPacking stereoPacking;

        private static MediaHints defaultHints = new MediaHints();
        public static MediaHints Default { get { return defaultHints; } }
    }

    [System.Serializable]
    public struct VideoResolveOptions
    {
        [SerializeField] public bool applyHSBC;
        [SerializeField, Range(0f, 1f)] public float hue;
        [SerializeField, Range(0f, 1f)] public float saturation;
        [SerializeField, Range(0f, 1f)] public float brightness;
        [SerializeField, Range(0f, 1f)] public float contrast;
        [SerializeField, Range(0.0001f, 10f)] public float gamma;
        [SerializeField] public Color tint;
        [SerializeField] public bool generateMipmaps;

        bool IsIdentityColourAdjust()
        {
            return (applyHSBC && hue != 0.0f && saturation != 0.5f && brightness != 0.5f && contrast != 0.5f && gamma != 1.0f);
        }

        void ResetColourAdjust()
        {
            hue = 0.0f;
            saturation = 0.5f;
            brightness = 0.5f;
            contrast = 0.5f;
            gamma = 1.0f;
        }

        public static VideoResolveOptions Create()
        {
            VideoResolveOptions result = new VideoResolveOptions()
            {
                tint = Color.white,
            };
            result.ResetColourAdjust();

            return result;
        }
    }

    /// Transparency Mode
    public enum TransparencyMode
    {
        Opaque,
        Transparent,
    }

    public enum StereoEye
    {
        Both,
        Left,
        Right,
    }

    public enum AlphaPacking
    {
        None,
        TopBottom,
        LeftRight,
    }

    public enum ErrorCode
    {
        None = 0,
        LoadFailed = 100,
        DecodeFailed = 200,
    }

    public enum Orientation
    {
        Landscape,              // Landscape Right (0 degrees)
        LandscapeFlipped,       // Landscape Left (180 degrees)
        Portrait,               // Portrait Up (90 degrees)
        PortraitFlipped,        // Portrait Down (-90 degrees)
        PortraitHorizontalMirror,   // Portrait that is mirrored horizontally
    }

    public enum VideoMapping
    {
        Unknown,
        Normal,
        EquiRectangular360,
        EquiRectangular180,
        CubeMap3x2,
    }

    public enum FileFormat
    {
        Unknown,
        HLS,
        DASH,
        SmoothStreaming,
    }

    public static class Windows
    {
        public enum VideoApi
        {
            MediaFoundation,            // Windows 8.1 and above
            DirectShow,                 // Legacy API
            WinRT,                      // Windows 10 and above
        };

        public enum AudioOutput
        {
            System,                     // Default
            Unity,                      // Media Foundation API only
            FacebookAudio360,           // Media Foundation API only
            None,                       // Media Foundation API only
        }

        // WIP: Experimental feature to allow overriding audio device for VR headsets
        public const string AudioDeviceOutputName_Vive = "HTC VIVE USB Audio";
        public const string AudioDeviceOutputName_Rift = "Headphones (Rift Audio)";
    }

    public static class WindowsUWP
    {
        public enum VideoApi
        {
            MediaFoundation,            // UWP 8.1 and above
            WinRT,                      // UWP 10 and above
        };

        public enum AudioOutput
        {
            System,                     // Default
            Unity,                      // Media Foundation API only
            FacebookAudio360,           // Media Foundation API only
            None,                       // Media Foundation API only
        }
    }

    public static class Android
    {
        public enum VideoApi
        {
            MediaPlayer = 1,
            ExoPlayer,
        }

        public enum AudioOutput
        {
            System,                     // Default
            Unity,                      // ExoPlayer API only
            FacebookAudio360,           // ExoPlayer API only
        }

        public const int Default_MinBufferTimeMs = 50000;   // Only valid when using ExoPlayer (default comes from DefaultLoadControl.DEFAULT_MIN_BUFFER_MS)
        public const int Default_MaxBufferTimeMs = 50000;   // Only valid when using ExoPlayer (default comes from DefaultLoadControl.DEFAULT_MAX_BUFFER_MS)
        public const int Default_BufferForPlaybackMs = 2500;        // Only valid when using ExoPlayer (default comes from DefaultLoadControl.DEFAULT_BUFFER_FOR_PLAYBACK_MS)
        public const int Default_BufferForPlaybackAfterRebufferMs = 5000;       // Only valid when using ExoPlayer (default comes from DefaultLoadControl.DEFAULT_BUFFER_FOR_PLAYBACK_AFTER_REBUFFER_MS)
    }

    public static class WebGL
    {
        public enum ExternalLibrary
        {
            None,
            DashJs,
            HlsJs,
            Custom,
        }
    }

    // Facebook Audio 360 channel mapping
    public enum Audio360ChannelMode
    {
        TBE_8_2 = 0,         /// 8 channels of hybrid TBE ambisonics and 2 channels of head-locked stereo audio
		TBE_8,               /// 8 channels of hybrid TBE ambisonics. NO head-locked stereo audio
		TBE_6_2,             /// 6 channels of hybrid TBE ambisonics and 2 channels of head-locked stereo audio
		TBE_6,               /// 6 channels of hybrid TBE ambisonics. NO head-locked stereo audio
		TBE_4_2,             /// 4 channels of hybrid TBE ambisonics and 2 channels of head-locked stereo audio
		TBE_4,               /// 4 channels of hybrid TBE ambisonics. NO head-locked stereo audio
		TBE_8_PAIR0,         /// Channels 1 and 2 of TBE hybrid ambisonics
		TBE_8_PAIR1,         /// Channels 3 and 4 of TBE hybrid ambisonics
		TBE_8_PAIR2,         /// Channels 5 and 6 of TBE hybrid ambisonics
		TBE_8_PAIR3,         /// Channels 7 and 8 of TBE hybrid ambisonics
		TBE_CHANNEL0,        /// Channels 1 of TBE hybrid ambisonics
		TBE_CHANNEL1,        /// Channels 2 of TBE hybrid ambisonics
		TBE_CHANNEL2,        /// Channels 3 of TBE hybrid ambisonics
		TBE_CHANNEL3,        /// Channels 4 of TBE hybrid ambisonics
		TBE_CHANNEL4,        /// Channels 5 of TBE hybrid ambisonics
		TBE_CHANNEL5,        /// Channels 6 of TBE hybrid ambisonics
		TBE_CHANNEL6,        /// Channels 7 of TBE hybrid ambisonics
		TBE_CHANNEL7,        /// Channels 8 of TBE hybrid ambisonics
		HEADLOCKED_STEREO,   /// Head-locked stereo audio
		HEADLOCKED_CHANNEL0, /// Channels 1 or left of head-locked stereo audio
		HEADLOCKED_CHANNEL1, /// Channels 2 or right of head-locked stereo audio
		AMBIX_4,             /// 4 channels of first order ambiX
		AMBIX_4_2,           /// 4 channels of first order ambiX with 2 channels of head-locked audio
		AMBIX_9,             /// 9 channels of second order ambiX
		AMBIX_9_2,           /// 9 channels of second order ambiX with 2 channels of head-locked audio
		AMBIX_16,            /// 16 channels of third order ambiX
		AMBIX_16_2,          /// 16 channels of third order ambiX with 2 channels of head-locked audio
		MONO,                /// Mono audio
		STEREO,              /// Stereo audio
		UNKNOWN,             /// Unknown channel map
		INVALID,             /// Invalid/unknown map. This must always be last.
	}

    [System.Flags]
    public enum AudioChannelMaskFlags : int
    {
        Unspecified = 0x0,
        FrontLeft = 0x1,
        FrontRight = 0x2,
        FrontCenter = 0x4,
        LowFrequency = 0x8,
        BackLeft = 0x10,
        BackRight = 0x20,
        FrontLeftOfCenter = 0x40,
        FrontRightOfCenter = 0x80,
        BackCenter = 0x100,
        SideLeft = 0x200,
        SideRight = 0x400,
        TopCenter = 0x800,
        TopFrontLeft = 0x1000,
        TopFrontCenter = 0x2000,
        TopFrontRight = 0x4000,
        TopBackLeft = 0x8000,
        TopBackCenter = 0x10000,
        TopBackRight = 0x20000,
    }

    public enum TextureFlags : int
    {
        Unknown = 0,
        TopDown = 1 << 0,
        SamplingIsLinear = 1 << 1,
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct BufferedFramesState
    {
        public System.Int32 freeFrameCount;
        public System.Int32 bufferedFrameCount;
        public System.Int64 minTimeStamp;
        public System.Int64 maxTimeStamp;
        public System.Int32 prerolledCount;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct TextureFrame
    {
        internal System.IntPtr texturePointer;
        internal System.IntPtr auxTexturePointer;
        internal System.Int64 timeStamp;
        internal System.UInt32 frameCounter;
        internal System.UInt32 writtenFrameCount;
        internal TextureFlags flags;
        internal System.IntPtr internalNativePointer;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct TimeRange
    {
        public TimeRange(double startTime, double duration)
        {
            this.startTime = startTime;
            this.duration = duration;
        }

        public double startTime, duration;

        public double StartTime { get { return startTime; } }
        public double EndTime { get { return startTime + duration; } }
        public double Duration { get { return duration; } }
    }

    public class TimeRanges : IEnumerable
    {
        internal TimeRanges() { }

        public IEnumerator GetEnumerator()
        {
            return _ranges.GetEnumerator();
        }

        public TimeRange this[int index]
        {
            get
            {
                return _ranges[index];
            }
        }

        internal TimeRanges(TimeRange[] ranges)
        {
            _ranges = ranges;
            CalculateRange();
        }

        internal void CalculateRange()
        {
            _minTime = _maxTime = 0.0;
            if (_ranges != null && _ranges.Length > 0)
            {
                double maxTime = 0.0;
                double minTime = double.MaxValue;
                for (int i = 0; i < _ranges.Length; i++)
                {
                    minTime = System.Math.Min(minTime, _ranges[i].startTime);
                    maxTime = System.Math.Max(maxTime, _ranges[i].startTime + _ranges[i].duration);
                }
                _minTime = minTime;
                _maxTime = maxTime;
            }
        }

        public int Count { get { return _ranges.Length; } }
        public double MinTime { get { return _minTime; } }
        public double MaxTime { get { return _maxTime; } }
        public double Duration { get { return (_maxTime - _minTime); } }

        internal TimeRange[] _ranges = new TimeRange[0];
        internal double _minTime = 0.0;
        internal double _maxTime = 0.0;
    }

    /// <summary>
    /// Video gravity to use with external playback.
    /// </summary>
    public enum ExternalPlaybackVideoGravity
    {
        /// <summary>Resizes the video to fit the display, may cause stretching.</summary>
        Resize,
        /// <summary>Resizes the video whilst preserving the video's aspect ratio to fit the display bounds.</summary>
        ResizeAspect,
        /// <summary>Resizes the video whilst preserving aspect to fill the display bounds.</summary>
        ResizeAspectFill,
    };

}
