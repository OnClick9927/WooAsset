//
//  AVProVideo.h
//  AVProVideo
//
//  Created by Morris Butler on 21/05/2020.
//  Copyright © 2020 RenderHeads. All rights reserved.
//

#import "AVProVideoTypes.h"

//! Project version number for AVPro Video.
FOUNDATION_EXPORT double AVProVideoVersionNumber;

//! Project version string for AVPro Video.
FOUNDATION_EXPORT const unsigned char AVProVideoVersionString[];

#if __cplusplus
extern "C" {
#endif

NS_ASSUME_NONNULL_BEGIN

void *       AVPPluginGetVersionStringPointer(void);
AVPPlayerRef AVPPluginMakePlayer(AVPPlayerSettings settings);

void         AVPPlayerRelease(AVPPlayerRef player);

bool         AVPPlayerOpenURL(AVPPlayerRef player, const char *url, const char *headers);
void         AVPPlayerClose(AVPPlayerRef player);

void         AVPPlayerGetState(AVPPlayerRef player, AVPPlayerState *state);
void         AVPPlayerGetAssetInfo(AVPPlayerRef player, AVPPlayerAssetInfo *info);
void         AVPPlayerGetBufferedTimeRanges(AVPPlayerRef player, AVPPlayerTimeRange *ranges, int count);
void         AVPPlayerGetSeekableTimeRanges(AVPPlayerRef player, AVPPlayerTimeRange *ranges, int count);
void         AVPPlayerGetTexture(AVPPlayerRef player, AVPPlayerTexture *texture);
void         AVPPlayerGetText(AVPPlayerRef player, AVPPlayerText *text);
int          AVPPlayerGetAudio(AVPPlayerRef player, float *buffer, int length);

void         AVPPlayerGetVideoTrackInfo(AVPPlayerRef player, int index, AVPPlayerVideoTrackInfo *info);
void         AVPPlayerGetAudioTrackInfo(AVPPlayerRef player, int index, AVPPlayerAudioTrackInfo *info);
void         AVPPlayerGetTextTrackInfo(AVPPlayerRef player, int index, AVPPlayerTextTrackInfo *info);

void         AVPPlayerSetFlags(AVPPlayerRef player, int flags);
void         AVPPlayerSetRate(AVPPlayerRef player, float rate);
void         AVPPlayerSetVolume(AVPPlayerRef player, float volume);
void         AVPPlayerSetExternalPlaybackVideoGravity(AVPPlayerRef player, AVPPlayerExternalPlaybackVideoGravity externalPlaybackVideoGravity);
bool         AVPPlayerSetTrack(AVPPlayerRef player, AVPPlayerTrackType type, int index);
void         AVPPlayerSetPlayerSettings(AVPPlayerRef player, AVPPlayerSettings settings);

void         AVPPlayerSeek(AVPPlayerRef player, double toTime, double toleranceBefore, double toleranceAfter);

void         AVPPlayerSetKeyServerURL(AVPPlayerRef player, const char *url);
void         AVPPlayerSetKeyServerAuthToken(AVPPlayerRef player, const char *token);
void         AVPPlayerSetDecryptionKey(AVPPlayerRef player, const char *key, int length);

void         AVPPluginUnityRegisterRenderingPlugin(void *registerRenderingPluginFunction);

void         AVPPluginCacheMediaForURL(const char *url, const char *headers, AVPMediaCachingOptions options);
void         AVPPluginCancelDownloadOfMediaForURL(const char *url);
void         AVPPluginRemoveCachedMediaForURL(const char *url);
int          AVPPluginGetCachedMediaStatusForURL(const char *url, float *progress);

NS_ASSUME_NONNULL_END

#if __cplusplus
}
#endif
