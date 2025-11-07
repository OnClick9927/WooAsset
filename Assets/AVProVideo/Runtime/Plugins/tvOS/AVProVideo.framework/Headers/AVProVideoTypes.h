//
//  AVProVideoTypes.h
//  AVProVideo
//
//  Created by Morris Butler on 02/10/2020.
//  Copyright © 2020 RenderHeads. All rights reserved.
//

#ifndef AVProVideoTypes_h
#define AVProVideoTypes_h

#import <Foundation/Foundation.h>
#import <simd/simd.h>

typedef void * AVPPlayerRef;

// Video settings

/// Supported video pixel format types.
/// @const AVPPlayerVideoPixelFormatInvalid
/// @const AVPPlayerVideoPixelFormatBgra Generic Planar RGBA pixel format, includes BGRA32, DXTn, etc.
/// @const AVPPlayerVideoPixelFormatYCbCr420 Bi-planar Y'CbCr pixel format.
typedef NS_ENUM(int, AVPPlayerVideoPixelFormat)
{
	AVPPlayerVideoPixelFormatInvalid,
	AVPPlayerVideoPixelFormatBgra,
	AVPPlayerVideoPixelFormatYCbCr420
};

typedef NS_OPTIONS(int, AVPPlayerVideoOutputSettingsFlags)
{
	AVPPlayerVideoOutputSettingsFlagsNone            = 0,
	AVPPlayerVideoOutputSettingsFlagsLinear          = 1 << 0,
	AVPPlayerVideoOutputSettingsFlagsGenerateMipMaps = 1 << 1,
};

typedef struct
{
	float width;
	float height;
} AVPPlayerDimensions;

typedef struct
{
	AVPPlayerVideoPixelFormat pixelFormat;
	AVPPlayerVideoOutputSettingsFlags flags;
	AVPPlayerDimensions preferredMaximumResolution;
	float maximumPlaybackRate;
} AVPPlayerVideoOutputSettings;

// Audio settings

typedef NS_ENUM(int, AVPPlayerAudioOutputMode)
{
	AVPPlayerAudioOutputModeSystemDirect,
	AVPPlayerAudioOutputModeCapture,
};

typedef NS_OPTIONS(int, AVPPlayerAudioOutputSettingsFlags)
{
	AVPPlayerAudioOutputSettingsFlagsNone            = 0,
};

typedef struct
{
	AVPPlayerAudioOutputMode mode;
	int sampleRate;
	int bufferLength;
	AVPPlayerAudioOutputSettingsFlags flags;
} AVPPlayerAudioOutputSettings;


typedef NS_OPTIONS(int, AVPPlayerNetworkSettingsFlags)
{
	AVPPlayerNetworkSettingsFlagsNone = 0,
	AVPPlayerNetworkSettingsFlagsPlayWithoutBuffering = 1 << 0,
	AVPPlayerNetworkSettingsFlagsUseSinglePlayerItem  = 1 << 1,
};

typedef struct
{
	double preferredPeakBitRate;
	double preferredForwardBufferDuration;
	AVPPlayerNetworkSettingsFlags flags;
} AVPPlayerNetworkSettings;

typedef struct
{
	AVPPlayerVideoOutputSettings videoOutputSettings;
	AVPPlayerAudioOutputSettings audioOutputSettings;
	AVPPlayerNetworkSettings networkSettings;
} AVPPlayerSettings;

// Player state

///
typedef NS_OPTIONS(int, AVPPlayerStatus)
{
	AVPPlayerStatusUnknown                   = 0,
	
	AVPPlayerStatusReadyToPlay               = 1 <<  0,
	AVPPlayerStatusPlaying                   = 1 <<  1,
	AVPPlayerStatusPaused                    = 1 <<  2,
	AVPPlayerStatusFinished                  = 1 <<  3,
	AVPPlayerStatusSeeking                   = 1 <<  4,
	AVPPlayerStatusBuffering                 = 1 <<  5,
	AVPPlayerStatusStalled                   = 1 <<  6,
	AVPPlayerStatusExternalPlaybackActive    = 1 <<  7,
	AVPPlayerStatusCached                    = 1 <<  8,
	AVPPlayerStatusFinishedSeeking           = 1 <<  9,

	AVPPlayerStatusUpdatedAssetInfo          = 1 << 16,
	AVPPlayerStatusUpdatedTexture            = 1 << 17,
	AVPPlayerStatusUpdatedBufferedTimeRanges = 1 << 18,
	AVPPlayerStatusUpdatedSeekableTimeRanges = 1 << 19,
	AVPPlayerStatusUpdatedText               = 1 << 20,
	
	AVPPlayerStatusHasVideo                  = 1 << 24,
	AVPPlayerStatusHasAudio                  = 1 << 25,
	AVPPlayerStatusHasText                   = 1 << 26,
	AVPPlayerStatusHasMetadata               = 1 << 27,

	AVPPlayerStatusFailed                    = 1 << 31
};

typedef NS_OPTIONS(int, AVPPlayerFlags)
{
	AVPPlayerFlagsNone                  = 0,
	AVPPlayerFlagsLooping               = 1 <<  0,
	AVPPlayerFlagsMuted                 = 1 <<  1,
	AVPPlayerFlagsAllowExternalPlayback = 1 <<  2,
	AVPPlayerFlagsResumePlayback        = 1 << 16,
//	AVPPlayerFlagsCacheAsset            = 1 << 17,
	AVPPlayerFlagsDirty                 = 1 << 31,
};

typedef NS_ENUM(int, AVPPlayerExternalPlaybackVideoGravity)
{
	AVPPlayerExternalPlaybackVideoGravityResize,
	AVPPlayerExternalPlaybackVideoGravityResizeAspect,
	AVPPlayerExternalPlaybackVideoGravityResizeAspectFill
};

typedef struct
{
	AVPPlayerStatus status;
	double currentTime;
	double currentDate;
	int selectedVideoTrack;
	int selectedAudioTrack;
	int selectedTextTrack;
	int bufferedTimeRangesCount;
	int seekableTimeRangesCount;
	int audioCaptureBufferedSamplesCount;
} AVPPlayerState;

typedef NS_OPTIONS(int, AVPPlayerAssetFlags)
{
	AVPPlayerAssetFlagsNone = 0,
	AVPPlayerAssetFlagsCompatibleWithAirPlay = 1 << 0,
};

typedef struct
{
	double duration;
	AVPPlayerDimensions dimensions;
	float frameRate;
	int videoTrackCount;
	int audioTrackCount;
	int textTrackCount;
	AVPPlayerAssetFlags flags;
} AVPPlayerAssetInfo;

typedef NS_OPTIONS(int, AVPPlayerTrackFlags)
{
	AVPPlayerTrackFlagsNone    = 0,
	AVPPlayerTrackFlagsDefault = 1 << 0,
};

typedef NS_ENUM(int, AVPPlayerVideoTrackStereoMode)
{
	AVPPlayerVideoTrackStereoModeUnknown,
	AVPPlayerVideoTrackStereoModeMonoscopic,
	AVPPlayerVideoTrackStereoModeStereoscopicTopBottom,
	AVPPlayerVideoTrackStereoModeStereoscopicLeftRight,
	AVPPlayerVideoTrackStereoModeStereoscopicCustom,
	AVPPlayerVideoTrackStereoModeStereoscopicRightLeft,
};

typedef struct
{
	float a;
	float b;
	float c;
	float d;
	float tx;
	float ty;
} AVPAffineTransform;

typedef NS_OPTIONS(int, AVPPlayerVideoTrackFlags)
{
	AVPPlayerVideoTrackFlagsNone     = 0,
	AVPPlayerVideoTrackFlagsHasAlpha = 1 << 0,
};

// Version of simd_float4x4 with relaxed alignment requirements. Allows for passing Matrix4x4 through from
// Unity. Probably not remotely useful otherwise.
typedef struct { simd_packed_float4 columns[4]; } simd_packed_float4x4;

#ifdef __x86_64__
typedef simd_packed_float4x4 Matrix4x4;
#else
typedef simd_float4x4 Matrix4x4;
#endif

typedef struct
{
	unichar * _Nullable name;
	unichar * _Nullable language;
	int trackID;
	float estimatedDataRate;
	uint32_t codecSubtype;
	AVPPlayerTrackFlags flags;

	AVPPlayerDimensions dimensions;
	float frameRate;
	AVPAffineTransform transform;
	AVPPlayerVideoTrackStereoMode stereoMode;
	int bitsPerComponent;
	AVPPlayerVideoTrackFlags videoTrackFlags;

	Matrix4x4 yCbCrTransform;
} AVPPlayerVideoTrackInfo;

/// Audio channel bitmap
/// @const AVPPlayerAudioTrackChannelBitmapUnspecified No channels specified.
typedef NS_OPTIONS(uint32_t, AVPPlayerAudioTrackChannelBitmap)
{
	AVPPlayerAudioTrackChannelBitmapUnspecified 		= 0,
	AVPPlayerAudioTrackChannelBitmapFrontLeft 			= 1 <<  0,
	AVPPlayerAudioTrackChannelBitmapFrontRight 			= 1 <<  1,
	AVPPlayerAudioTrackChannelBitmapFrontCenter 		= 1 <<  2,
	AVPPlayerAudioTrackChannelBitmapLowFrequency 		= 1 <<  3,
	AVPPlayerAudioTrackChannelBitmapBackLeft 			= 1 <<  4,
	AVPPlayerAudioTrackChannelBitmapBackRight 			= 1 <<  5,
	AVPPlayerAudioTrackChannelBitmapFrontLeftOfCenter 	= 1 <<  6,
	AVPPlayerAudioTrackChannelBitmapFrontRightOfCenter 	= 1 <<  7,
	AVPPlayerAudioTrackChannelBitmapBackCenter 			= 1 <<  8,
	AVPPlayerAudioTrackChannelBitmapSideLeft 			= 1 <<  9,
	AVPPlayerAudioTrackChannelBitmapSideRight 			= 1 << 10,
	AVPPlayerAudioTrackChannelBitmapTopCenter 			= 1 << 11,
	AVPPlayerAudioTrackChannelBitmapTopFrontLeft 		= 1 << 12,
	AVPPlayerAudioTrackChannelBitmapTopFrontCenter 		= 1 << 13,
	AVPPlayerAudioTrackChannelBitmapTopFrontRight 		= 1 << 14,
	AVPPlayerAudioTrackChannelBitmapTopBackLeft 		= 1 << 15,
	AVPPlayerAudioTrackChannelBitmapTopBackCenter 		= 1 << 16,
	AVPPlayerAudioTrackChannelBitmapTopBackRight 		= 1 << 17,
};

typedef struct
{
	unichar * _Nullable name;
	unichar * _Nullable language;
	int trackID;
	float estimatedDataRate;
	uint32_t codecSubtype;
	AVPPlayerTrackFlags flags;
	
	double sampleRate;
	uint32_t channelCount;
	uint32_t channelLayoutTag;
	AVPPlayerAudioTrackChannelBitmap channelBitmap;

} AVPPlayerAudioTrackInfo;

typedef struct
{
	unichar * _Nullable name;
	unichar * _Nullable language;
	int trackID;
	float estimatedDataRate;
	uint32_t codecSubtype;
	AVPPlayerTrackFlags flags;
} AVPPlayerTextTrackInfo;

typedef NS_ENUM(int, AVPPlayerTrackType)
{
	AVPPlayerTrackTypeVideo,
	AVPPlayerTrackTypeAudio,
	AVPPlayerTrackTypeText
};

typedef struct
{
	double start;
	double duration;
} AVPPlayerTimeRange;

/// Texture flags.
/// @const AVPPlayerTextureFlagsFlipped The texture is flipped in the y-axis.
/// @const AVPlayerTextureFlagsLinear The texture uses the linear color space.
/// @const AVPPlayerTextureFlagsMipmapped The texture has mipmaps.
typedef NS_OPTIONS(int, AVPPlayerTextureFlags)
{
	AVPPlayerTextureFlagsNone      = 0,
	AVPPlayerTextureFlagsFlipped   = 1 << 0,
	AVPPlayerTextureFlagsLinear    = 1 << 1,
	AVPPlayerTextureFlagsMipmapped = 1 << 2,
};

typedef NS_ENUM(int, AVPPlayerTextureFormat)
{
	AVPPlayerTextureFormatInvalid,
	AVPPlayerTextureFormatBgra8Unorm,
	AVPPlayerTextureFormatR8Unorm,
	AVPPlayerTextureFormatRg8Unorm,
	AVPPlayerTextureFormatBc1,
	AVPPlayerTextureFormatBc3,
	AVPPlayerTextureFormatBc4,
	AVPPlayerTextureFormatBc5,
	AVPPlayerTextureFormatBc7,
	AVPPlayerTextureFormatBgr10a2Unorm,
	AVPPlayerTextureFormatR16Unorm,
	AVPPlayerTextureFormatRg16Unorm,
	AVPPlayerTextureFormatBgr10xr,
};

typedef struct
{
	void * _Nullable plane;
	int width;
	int height;
	AVPPlayerTextureFormat pixelFormat;
} AVPPlayerTexturePlane;

#define AVPPlayerTextureMaxPlanes 2

typedef struct
{
	AVPPlayerTexturePlane planes[AVPPlayerTextureMaxPlanes];
	int64_t itemTime;
	int frameCount;
	int planeCount;
	AVPPlayerTextureFlags flags;
} AVPPlayerTexture;

typedef struct
{
	void * _Nullable buffer;
	int64_t itemTime;
	int32_t length;
	int32_t sequence;
} AVPPlayerText;

typedef struct AudioCaptureBuffer *AudioCaptureBufferRef;

typedef struct
{
	double minimumRequiredBitRate;
	struct { float width, height; } minimumRequiredResolution;
	const char * _Nullable title;
	const void * _Nullable artwork;
	int artworkLength;
} AVPMediaCachingOptions;

typedef NS_ENUM(int, AVPCachedMediaStatus)
{
	AVPCachedMediaStatusNotCached,
	AVPCachedMediaStatusCaching,
	AVPCachedMediaStatusCached,
	AVPCachedMediaStatusFailed
};

#endif /* AVProVideoTypes_h */
