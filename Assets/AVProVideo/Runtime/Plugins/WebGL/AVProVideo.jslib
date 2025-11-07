var AVProVideoWebGL =
{
	/*isNumber: function (item) {
		return typeof(item) === "number" && !isNaN(item);
	},
	assert: function (equality, message) {
		if (!equality)
			console.log(message);
	},*/
	count: 0,
	players: [],

	hasPlayer__deps: ["players"],
	hasPlayer: function (videoIndex)
	{
		if (videoIndex)
		{
			if (videoIndex == -1)
			{
				return false;
			}

			if (_players)
			{
				if (_players[videoIndex])
				{
					return true;
				}
			}
		}
		else
		{
			if (_players)
			{
				if (_players.length > 0) 
				{
					return true;
				}
			}
		}
		return false;
	},

	AVPPlayerInsertVideoElement__deps: ["count", "players"],
	AVPPlayerInsertVideoElement: function (path, idValues, externalLibrary)
	{
		if (!path) { return false; }

		// NOTE: When loading from the indexedDB (Application.persistentDataPath), 
		//       URL.createObjectURL() must be used get a valid URL.  See:
		//       http://www.misfitgeek.com/html5-off-line-storing-and-retrieving-videos-with-indexeddb/
		path = Pointer_stringify(path);
		_count++;

		var vid = document.createElement("video");
		var useNativeSrcPath = true;
		var hls = null;

		if (externalLibrary == 1)
		{
			useNativeSrcPath = false;
			var player = dashjs.MediaPlayer().create();
			player.initialize(vid, path, true);
		}
		else if (externalLibrary == 2)
		{
			useNativeSrcPath = false;
			hls = new Hls();
			hls.loadSource(path);
			hls.attachMedia(vid);
			hls.on(Hls.Events.MANIFEST_PARSED, function()
			{
				//video.play();
			});
		}
		else if (externalLibrary == 3)
		{
			//useNativeSrcPath = false;
		}

		// Some sources say that this is the proper way to catch errors...
		/*vid.addEventListener('error', function(event) {
			console.log("Error: " + event);
		}, true);*/

		var hasSetCanPlay = false;
		var playerIndex;
		var id = _count;
		
		var vidData = {
			id: id,
			video: vid,
			ready: false,
			hasMetadata: false,
			isStalled: false,
			buffering: false,
			lastErrorCode: 0,
			hlsjs: hls
		};

		_players.push(vidData);
		playerIndex = (_players.length > 0) ? _players.length - 1 : 0;

		/*const frameCounterCallback = function (timeNow, metadata) {
			console.log("got a frame! " + metadata.presentedFrames + " " +  metadata.presentationTime);
			vid.requestVideoFrameCallback(frameCounterCallback);
		};

		if (HTMLVideoElement.prototype.requestVideoFrameCallback)
		{
			console.log("has frame callback support");
			vid.requestVideoFrameCallback(frameCounterCallback);
		}*/

		vid.oncanplay = function()
		{
			if (!hasSetCanPlay) 
			{
				hasSetCanPlay = true;
				vidData.ready = true;
			}
		};

		vid.onloadedmetadata = function()
		{
			vidData.hasMetadata = true;
		};

		vid.oncanplaythrough = function()
		{
			vidData.buffering = false;
		};

		vid.onplaying = function()
		{
			vidData.buffering = false;
			vidData.isStalled = false;
			//console.log("PLAYING");
		};

		vid.onwaiting = function()
		{
			vidData.buffering = true;
			//console.log("WAITING");
		};

		vid.onstalled = function()
		{
			vidData.isStalled = true;
			//console.log("STALLED");
		}

		/*vid.onpause = function() {
		};*/

		vid.onended = function()
		{
			vidData.buffering = false;
			vidData.isStalled = false;
			//console.log("ENDED");
		};

		vid.ontimeupdate = function()
		{
			vidData.buffering = false;
			vidData.isStalled = false;
			//console.log("vid current time: ", this.currentTime);
		};

		vid.onerror = function(texture)
		{
			var err = "unknown error";

			switch (vid.error.code) {
				case 1:
					err = "video loading aborted";
					break;
				case 2:
					err = "network loading error";
					break;
				case 3:
					err = "video decoding failed / corrupted data or unsupported codec";
					break;
				case 4:
					err = "video not supported";
					break;
			}

			vidData.lastErrorCode = vid.error.code;

			console.log("Error: " + err + " (errorcode=" + vid.error.code + ")", "color:red;");
		};

		vid.crossOrigin = "anonymous";
		vid.preload = 'auto';
		vid.autoplay = false;
		if (useNativeSrcPath)
		{
			vid.src = path;
		}

		HEAP32[(idValues>>2)] = playerIndex;
		HEAP32[(idValues>>2)+1] = id;

		return true;
	},

	AVPPlayerGetLastError__deps: ["players", "hasPlayer"],
	AVPPlayerGetLastError: function(playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0; }

		var ret = _players[playerIndex].lastErrorCode
		_players[playerIndex].lastErrorCode = 0;

		return ret;
	},
	
	AVPPlayerCreateVideoTexture__deps: ["players", "hasPlayer"],
	AVPPlayerCreateVideoTexture: function (textureId)
	{
		const texture = GLctx.createTexture();
		GL.textures[textureId] = texture;

		//console.log("creating textureId " +textureId + " : " + GL.textures[textureId]);
		GLctx.bindTexture(GLctx.TEXTURE_2D, texture);
	},

	AVPPlayerDestroyVideoTexture__deps: ["players", "hasPlayer"],
	AVPPlayerDestroyVideoTexture: function (textureId)
	{
		GLctx.deleteTexture(GL.textures[textureId]);
	},

	AVPPlayerFetchVideoTexture__deps: ["players", "hasPlayer"],
	AVPPlayerFetchVideoTexture: function (playerIndex, textureId, init)
	{
		if (!_hasPlayer(playerIndex)) {	return;	}

		//console.log("updating textureId " +textureId + " : " + GL.textures[textureId]);
		GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[textureId]);

		//GLctx.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
		if (!init)
		{
			//GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, _players[playerIndex].video);
			GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, _players[playerIndex].video);
		}
		else
		{
			GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, _players[playerIndex].video);
		}
		
		//NB: This line causes the texture to not show unless something else is rendered (not sure why)
		//GLctx.bindTexture(GLctx.TEXTURE_2D, null);
		GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
		GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
		GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);
	},

	AVPPlayerUpdatePlayerIndex__deps: ["players", "hasPlayer"],
	AVPPlayerUpdatePlayerIndex: function (id)
	{
		var result = -1;

		if (!_hasPlayer()) { return result;	}

		_players.forEach(function (currentVid, index)
		{
			if (currentVid != null && currentVid.id == id)
			{
				result = index;
			}
		});

		return result;
	},

	AVPPlayerWidth__deps: ["players", "hasPlayer"],
	AVPPlayerWidth: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0; }

		return _players[playerIndex].video.videoWidth;
	},

	AVPPlayerHeight__deps: ["players", "hasPlayer"],
	AVPPlayerHeight: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0; }

		return _players[playerIndex].video.videoHeight;
	},

	AVPPlayerReady__deps: ["players", "hasPlayer"],
	AVPPlayerReady: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		if (_players)
		{
			if (_players.length > 0)
			{
				if (_players[playerIndex])
				{
					return _players[playerIndex].ready;
				}
			}
		}
		else
		{
			return false;
		}

		//return _players[playerIndex].video.readyState >= _players[playerIndex].video.HAVE_CURRENT_DATA;
	},

	AVPPlayerClose__deps: ["players", "hasPlayer"],
	AVPPlayerClose: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return;	}

		var vid = _players[playerIndex].video;

		// Setting 'src' to an empty string results in the onerror handler being invoked and producing log noise on Chrome.
		// Removing the src attribute and invoking load is a recommended best practice in the HTML Standard.
		// See https://html.spec.whatwg.org/multipage/media.html#best-practices-for-authors-using-media-elements
		vid.pause();
		vid.removeAttribute("src"); // Previous: vid.src = "";
		vid.load();

		if (_players[playerIndex].hlsjs != null)
		{
			_players[playerIndex].hlsjs.destroy();
			_players[playerIndex].hlsjs = null;
		}

		_players[playerIndex].video = null;
		_players[playerIndex] = null;

		var allEmpty = true;
		for (i = 0; i < _players.length; i++) {
			if (_players[i] != null) {
				allEmpty = false;
				break;
			}
		}
		if (allEmpty)
		{
			_players = [];
		}
		//_players = _players.splice(playerIndex, 1);

		// Remove from DOM
		//vid.parentNode.removeChild(vid);
	},

	AVPPlayerSetLooping__deps: ["players", "hasPlayer"],
	AVPPlayerSetLooping: function (playerIndex, loop)
	{
		if (!_hasPlayer(playerIndex)) {	return; }

		_players[playerIndex].video.loop = loop;
	},

	AVPPlayerIsLooping__deps: ["players", "hasPlayer"],
	AVPPlayerIsLooping: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		return _players[playerIndex].video.loop;
	},

	AVPPlayerHasMetadata__deps: ["players", "hasPlayer"],
	AVPPlayerHasMetadata: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		return (_players[playerIndex].video.readyState >= 1);
	},

	AVPPlayerIsPlaying__deps: ["players", "hasPlayer"],
	AVPPlayerIsPlaying: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var video = _players[playerIndex].video;

		return (!video.paused && !video.ended);// || video.seeking || video.readyState < video.HAVE_FUTURE_DATA);
	},

	AVPPlayerIsSeeking__deps: ["players", "hasPlayer"],
	AVPPlayerIsSeeking: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		return _players[playerIndex].video.seeking;
	},

	AVPPlayerIsPaused__deps: ["players", "hasPlayer"],
	AVPPlayerIsPaused: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		return _players[playerIndex].video.paused;
	},

	AVPPlayerIsFinished__deps: ["players", "hasPlayer"],
	AVPPlayerIsFinished: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		return _players[playerIndex].video.ended;
	},

	AVPPlayerIsBuffering__deps: ["players", "hasPlayer"],
	AVPPlayerIsBuffering: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		return _players[playerIndex].buffering;
	},

	AVPPlayerIsPlaybackStalled__deps: ["players", "hasPlayer"],
	AVPPlayerIsPlaybackStalled: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		return _players[playerIndex].isStalled;
	},

	AVPPlayerPlay__deps: ["players", "hasPlayer"],
	AVPPlayerPlay: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		// https://webkit.org/blog/7734/auto-play-policy-changes-for-macos/
		// https://developers.google.com/web/updates/2017/06/play-request-was-interrupted
		var playPromise = _players[playerIndex].video.play();
		if (playPromise !== undefined)
		{
			playPromise.then(function()
			{
				// Automatic playback started!
				// Show playing UI.
			})
			.catch(function(error) 
			{
				// Auto-play was prevented
				// Show paused UI.
				return false;
			});
		}
		return true;
	},

	AVPPlayerPause__deps: ["players", "hasPlayer"],
	AVPPlayerPause: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return;	}

		_players[playerIndex].video.pause();
	},

	AVPPlayerSeekToTime__deps: ["players", "hasPlayer"],
	AVPPlayerSeekToTime: function (playerIndex, timeSec, fast)
	{
		if (!_hasPlayer(playerIndex)) {	return;	}

		var vid = _players[playerIndex].video;

		if (vid.seekable && vid.seekable.length > 0)
		{
			var timeNorm = 0.0;
			if (vid.duration > 0.0)
			{
				timeNorm = timeSec / vid.duration;
			}
			for (i = 0; i < vid.seekable.length; i++)
			{
				if (timeNorm >= vid.seekable.start(i) && timeNorm <= vid.seekable.end(i)) 
				{
					if (fast && vid.fastSeek)
					{
						vid.fastSeek(timeNorm);
					}
					else
					{
						vid.currentTime = timeSec;
					}
					return;
				}
			}
		}
		else
		{
			if (timeSec == 0.0) 
			{
				vid.load();
			}
			else
			{
				vid.currentTime = timeSec;
			}
		}
	},

	AVPPlayerGetCurrentTime__deps: ["players", "hasPlayer"],
	AVPPlayerGetCurrentTime: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return 0.0;	}

		return _players[playerIndex].video.currentTime;
	},

	AVPPlayerGetDuration__deps: ["players", "hasPlayer"],
	AVPPlayerGetDuration: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return 0.0;	}

		return _players[playerIndex].video.duration;
	},

	AVPPlayerGetPlaybackRate__deps: ["players", "hasPlayer"],
	AVPPlayerGetPlaybackRate: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return 0.0;	}

		return _players[playerIndex].video.playbackRate;
	},

	AVPPlayerSetPlaybackRate__deps: ["players", "hasPlayer"],
	AVPPlayerSetPlaybackRate: function (playerIndex, rate)
	{
		if (!_hasPlayer(playerIndex)) {	return;	}

		_players[playerIndex].video.playbackRate = rate;
	},

	AVPPlayerSetMuted__deps: ["players", "hasPlayer"],
	AVPPlayerSetMuted: function (playerIndex, mute)
	{
		if (!_hasPlayer(playerIndex)) {	return;	}

		_players[playerIndex].video.muted = mute;
	},

	AVPPlayerIsMuted__deps: ["players", "hasPlayer"],
	AVPPlayerIsMuted: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		return _players[playerIndex].video.muted;
	},

	AVPPlayerSetVolume__deps: ["players", "hasPlayer"],
	AVPPlayerSetVolume: function (playerIndex, volume)
	{
		if (!_hasPlayer(playerIndex)) { return;	}

		_players[playerIndex].video.volume = volume;
	},

	AVPPlayerGetVolume__deps: ["players", "hasPlayer"],
	AVPPlayerGetVolume: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return 0.0;	}

		return _players[playerIndex].video.volume;
	},

	AVPPlayerHasVideo__deps: ["players", "hasPlayer"],
	AVPPlayerHasVideo: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var isChrome = !!window.chrome && !!window.chrome.webstore;

		if (isChrome)
		{
			return Boolean(_players[playerIndex].video.webkitVideoDecodedByteCount > 0);
		}
		
		if (_players[playerIndex].video.videoTracks)
		{
			return Boolean(_players[playerIndex].video.videoTracks.length > 0);
		}
		
		return true;
	},

	AVPPlayerHasAudio__deps: ["players", "hasPlayer"],
	AVPPlayerHasAudio: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		return _players[playerIndex].video.mozHasAudio || Boolean(_players[playerIndex].video.webkitAudioDecodedByteCount) ||
				Boolean(_players[playerIndex].video.audioTracks && _players[playerIndex].video.audioTracks.length);
	},

	AVPPlayerGetDecodedFrameCount__deps: ["players", "hasPlayer"],
	AVPPlayerGetDecodedFrameCount: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0; }

		var vid = _players[playerIndex].video;
		if (vid.readyState <= HTMLMediaElement.HAVE_CURRENT_DATA) { return 0; }

		var frameCount = 0;

		if (vid.mozPresentedFrames)
		{
			frameCount = vid.mozPresentedFrames;
		}
		else if (vid.mozDecodedFrames)
		{
			frameCount = vid.mozDecodedFrames;
		}
		else if (vid.webkitDecodedFrameCount)
		{
			frameCount = vid.webkitDecodedFrameCount;
		}

		/*var q = vid.getVideoPlaybackQuality();
		if (q)
		{
			console.log("frames: " + q.totalVideoFrames + " " + q.droppedVideoFrames);
		}*/

		return frameCount;
	},

	AVPPlayerSupportedDecodedFrameCount__deps: ["players", "hasPlayer"],
	AVPPlayerSupportedDecodedFrameCount: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) {	return false; }

		var vid = _players[playerIndex].video;

		if (vid.mozPresentedFrames)
		{
			return true;
		}		
		else if (vid.mozDecodedFrames)
		{
			return true;
		}
		else if (vid.webkitDecodedFrameCount)
		{
			return true;
		}

		return false;
	},

	AVPPlayerGetNumBufferedTimeRanges__deps: ["players", "hasPlayer"],
	AVPPlayerGetNumBufferedTimeRanges: function(playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0; }

		if (_players[playerIndex].video.buffered)
		{
			return _players[playerIndex].video.buffered.length;
		}
		return 0;
	},

	AVPPlayerGetTimeRangeStart__deps: ["players", "hasPlayer"],
	AVPPlayerGetTimeRangeStart: function(playerIndex, rangeIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0.0;	}

		if (_players[playerIndex].video.buffered)
		{
			if(rangeIndex >= _players[playerIndex].video.buffered.length)
			{
				return 0.0;
			}
			return _players[playerIndex].video.buffered.start(rangeIndex);
		}
		return 0.0;
	},

	AVPPlayerGetTimeRangeEnd__deps: ["players", "hasPlayer"],
	AVPPlayerGetTimeRangeEnd: function(playerIndex, rangeIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0.0;	}

		if (_players[playerIndex].video.buffered)
		{
			if(rangeIndex >= _players[playerIndex].video.buffered.length)
			{
				return 0.0;
			}
			return _players[playerIndex].video.buffered.end(rangeIndex);
		}
		return 0.0;
	},

	AVPPlayerGetVideoTrackCount__deps: ["players", "hasPlayer", "AVPPlayerHasVideo"],
	AVPPlayerGetVideoTrackCount: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0; }

		var result = 0;
		var tracks = _players[playerIndex].video.videoTracks;
		if (tracks)
		{
			result = tracks.length;
		}
		else
		{
			if (_AVPPlayerHasVideo(playerIndex))
			{
				result = 1;
			}
		}
		return result;
	},

	AVPPlayerGetAudioTrackCount__deps: ["players", "hasPlayer", "AVPPlayerHasAudio"],
	AVPPlayerGetAudioTrackCount: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0; }

		var result = 0;
		var tracks = _players[playerIndex].video.audioTracks;
		if (tracks)
		{
			result = tracks.length;
		}
		else
		{
			if (_AVPPlayerHasAudio(playerIndex))
			{
				result = 1;
			}
		}
		return result;
	},

	AVPPlayerGetTextTrackCount__deps: ["players", "hasPlayer"],
	AVPPlayerGetTextTrackCount: function (playerIndex)
	{
		if (!_hasPlayer(playerIndex)) { return 0; }

		var result = 0;
		var tracks = _players[playerIndex].video.textTracks;
		if (tracks)
		{
			result = tracks.length;
		}
		return result;
	},

	AVPPlayerSetActiveVideoTrack__deps: ["players", "hasPlayer"],
	AVPPlayerSetActiveVideoTrack: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = false;
		if (_players[playerIndex].video.videoTracks)
		{
			var tracks = _players[playerIndex].video.videoTracks;
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				tracks[trackIndex].selected = true;
				result = true;
			}
		}
		return result;
	},

	AVPPlayerSetActiveAudioTrack: ["players", "hasPlayer"],
	AVPPlayerSetActiveAudioTrack: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = false;
		if (_players[playerIndex].video.audioTracks)
		{
			var tracks = _players[playerIndex].video.audioTracks;
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				for (i = 0; i < tracks.length; i++)
				{
					tracks[i].enabled = (i === trackIndex);
				}
				result = true;
			}
		}
		return result;
	},

	AVPPlayerSetActiveTextTrack: ["players", "hasPlayer"],
	AVPPlayerSetActiveTextTrack: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = false;
		if (_players[playerIndex].video.textTracks)
		{
			var tracks = _players[playerIndex].video.textTracks;
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				for (i = 0; i < tracks.length; i++)
				{
					tracks[i].mode = (i === trackIndex)?"showing":"disabled";
				}
				result = true;
			}
		}
		return result;
	},

	AVPPlayerStringToBuffer: [],
	AVPPlayerStringToBuffer: function (text)
	{
		// Get size of the string
		var bufferSize = lengthBytesUTF8(text) + 1;
		// Allocate memory space
		var buffer = _malloc(bufferSize);
		// Copy old data to the new one then return it
		stringToUTF8(text, buffer, bufferSize);
		return buffer;
	},

	AVPPlayerGetVideoTrackName: ["players", "hasPlayer", "AVPPlayerStringToBuffer"],
	AVPPlayerGetVideoTrackName: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = null;
		var tracks = _players[playerIndex].video.videoTracks;
		if (tracks)
		{
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				result = _AVPPlayerStringToBuffer(tracks[trackIndex].label);
			}
		}
		return result;
	},

	AVPPlayerGetAudioTrackName: ["players", "hasPlayer", "AVPPlayerStringToBuffer"],
	AVPPlayerGetAudioTrackName: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = null;
		var tracks = _players[playerIndex].video.audioTracks;
		if (tracks)
		{
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				result = _AVPPlayerStringToBuffer(tracks[trackIndex].label);
			}
		}
		return result;
	},

	AVPPlayerGetTextTrackName: ["players", "hasPlayer", "AVPPlayerStringToBuffer"],
	AVPPlayerGetTextTrackName: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = null;
		var tracks = _players[playerIndex].video.textTracks;
		if (tracks)
		{
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				result = _AVPPlayerStringToBuffer(tracks[trackIndex].label);
			}
		}
		return result;
	},

	AVPPlayerGetVideoTrackLanguage: ["players", "hasPlayer", "AVPPlayerStringToBuffer"],
	AVPPlayerGetVideoTrackLanguage: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = null;
		var tracks = _players[playerIndex].video.videoTracks;
		if (tracks)
		{
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				result = _AVPPlayerStringToBuffer(tracks[trackIndex].language);
			}
		}
		return result;
	},

	AVPPlayerGetAudioTrackLanguage: ["players", "hasPlayer", "AVPPlayerStringToBuffer"],
	AVPPlayerGetAudioTrackLanguage: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = null;
		var tracks = _players[playerIndex].video.audioTracks;
		if (tracks)
		{
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				result = _AVPPlayerStringToBuffer(tracks[trackIndex].language);
			}
		}
		return result;
	},

	AVPPlayerGetTextTrackLanguage: ["players", "hasPlayer", "AVPPlayerStringToBuffer"],
	AVPPlayerGetTextTrackLanguage: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = null;
		var tracks = _players[playerIndex].video.textTracks;
		if (tracks)
		{
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				result = _AVPPlayerStringToBuffer(tracks[trackIndex].language);
			}
		}
		return result;
	},

	AVPPlayerIsVideoTrackActive: ["players", "hasPlayer", "AVPPlayerHasVideo"],
	AVPPlayerIsVideoTrackActive: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = false;
		var tracks = _players[playerIndex].video.videoTracks;
		if (tracks)
		{
			result = (tracks.selectedIndex === trackIndex);
		}
		else
		{
			result = _AVPPlayerHasVideo(playerIndex);
		}
		return result;
	},

	AVPPlayerIsAudioTrackActive: ["players", "hasPlayer", "AVPPlayerHasAudio"],
	AVPPlayerIsAudioTrackActive: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = false;
		var tracks = _players[playerIndex].video.audioTracks;
		if (tracks)
		{
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				result = tracks[trackIndex].enabled;
			}
		}
		else
		{
			result = _AVPPlayerHasAudio(playerIndex);
		}
		return result;
	},

	AVPPlayerIsTextTrackActive: ["players", "hasPlayer"],
	AVPPlayerIsTextTrackActive: function (playerIndex, trackIndex)
	{
		if (!_hasPlayer(playerIndex)) { return false; }

		var result = false;
		var tracks = _players[playerIndex].video.textTracks;
		if (tracks)
		{
			if (trackIndex >=0 && trackIndex < tracks.length)
			{
				result = (tracks[trackIndex].mode === "showing");
			}
		}
		return result;
	}
};

autoAddDeps(AVProVideoWebGL, 'count');
autoAddDeps(AVProVideoWebGL, 'players');
autoAddDeps(AVProVideoWebGL, 'hasPlayer');
autoAddDeps(AVProVideoWebGL, 'AVPPlayerHasVideo');
autoAddDeps(AVProVideoWebGL, 'AVPPlayerHasAudio');
autoAddDeps(AVProVideoWebGL, 'AVPPlayerStringToBuffer');
mergeInto(LibraryManager.library, AVProVideoWebGL);