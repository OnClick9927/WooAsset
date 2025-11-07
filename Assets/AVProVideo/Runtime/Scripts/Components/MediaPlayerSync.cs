#if AVPROVIDEO_SUPPORT_BUFFERED_DISPLAY
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Experimental
{
	/// <summary>
	/// Syncronise multiple MediaPlayer components (currently Windows ONLY using Media Foundation ONLY)
	/// This feature requires Ultra Edition
	/// </summary>
	[AddComponentMenu("AVPro Video/Media Player Sync (BETA)", -90)]
	[HelpURL("http://renderheads.com/products/avpro-video/")]
	public class MediaPlayerSync : MonoBehaviour
	{
		[SerializeField] MediaPlayer _masterPlayer = null;
		[SerializeField] MediaPlayer[] _slavePlayers = null;
		[SerializeField] bool _playOnStart = true;
		[SerializeField] bool _waitAfterPreroll = false;
		[SerializeField] bool _logSyncErrors = false;

		public MediaPlayer MasterPlayer { get { return _masterPlayer; } set { _masterPlayer = value; } }
		public MediaPlayer[] SlavePlayers { get { return _slavePlayers; } set { _slavePlayers = value; } }
		public bool PlayOnStart { get { return _playOnStart; } set { _playOnStart = value; } }
		public bool WaitAfterPreroll { get { return _waitAfterPreroll; } set { _waitAfterPreroll = value; } }
		public bool LogSyncErrors { get { return _logSyncErrors; } set { _logSyncErrors = value; } }

		private enum State
		{
			Idle,
			Loading,
			Prerolling,
			Prerolled,
			Playing,
			Finished,
		}

		private State _state = State.Idle;

		void Awake()
		{
#if (UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN))
			SetupPlayers();
#else
			Debug.LogError("[AVProVideo] This component only works on the Windows platform");
			this.enabled = false;
#endif
		}

		void Start()
		{
			if (_playOnStart)
			{
				StartPlayback();
				_state = State.Loading;
				_playOnStart = false;
			}
		}

		public void OpenMedia(string[] mediaPaths)
		{
			Debug.Assert(mediaPaths.Length == (_slavePlayers.Length + 1));

			_masterPlayer.MediaSource = MediaSource.Path;
			_masterPlayer.MediaPath = new MediaPath(mediaPaths[0], MediaPathType.AbsolutePathOrURL);

			for (int i = 0; i < _slavePlayers.Length; i++)
			{
				_slavePlayers[i].MediaSource = MediaSource.Path;
				_slavePlayers[i].MediaPath = new MediaPath(mediaPaths[i+1], MediaPathType.AbsolutePathOrURL);
			}

			StartPlayback();
		}

		/// <summary>
		/// This is called when _autoPlay is false and once the MediaPlayers have had their source media set
		/// </summary>
		[ContextMenu("StartPlayback")]
		public void StartPlayback()
		{
			SetupPlayers();

			if (!IsPrerolled())
			{
				OpenMediaAll();
				_state = State.Loading;
			}
			else
			{
				PlayAll();
				_state = State.Playing;
			}
		}

		public void Seek(double time, bool approximate = true)
		{
			if (approximate)
			{
				SeekFastAll(time);
			}
			else
			{
				SeekAll(time);
			}

			_state = State.Prerolling;
		}

		public bool IsPrerolled()
		{
			return (_state == State.Prerolled);
		}

		void SetupPlayers()
		{
			SetupPlayer(_masterPlayer);
			for (int i = 0; i < _slavePlayers.Length; i++)
			{
				SetupPlayer(_slavePlayers[i]);
			}
		}

		void SetupPlayer(MediaPlayer player)
		{
			bool isMaster = (player == _masterPlayer);
			player.AutoOpen = false;
			player.AutoStart = false;
			player.AudioMuted = !isMaster;
			player.PlatformOptionsWindows.videoApi = Windows.VideoApi.MediaFoundation;
			player.PlatformOptionsWindows.useLowLatency = true;
			player.PlatformOptionsWindows.pauseOnPrerollComplete = true;
			player.PlatformOptionsWindows.bufferedFrameSelection = isMaster ? BufferedFrameSelectionMode.ElapsedTimeVsynced : BufferedFrameSelectionMode.FromExternalTime;
		}

		// NOTE: We check on LateUpdate() as MediaPlayer uses Update() to update state and we want to make sure all players have been updated
		void LateUpdate()
		{
			if (_state == State.Idle)
			{
			}
			if (_state == State.Loading)
			{
				UpdateLoading();
			}
			if (_state == State.Prerolling)
			{
				UpdatePrerolling();
			}
			if (_state == State.Prerolled)
			{
				/*if (Input.GetKeyDown(KeyCode.Alpha0))
				{
					StartPlayback();
				}*/
			}
			if (_state == State.Playing)
			{
				UpdatePlaying();
			}
			if (_state == State.Finished)
			{
			}

#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				Debug.Log("sleep");
				System.Threading.Thread.Sleep(16);
			}
			
			/*if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				double time = Random.Range(0f, (float)_masterPlayer.Info.GetDuration());
				Seek(time);
			}

			long gcMemory = System.GC.GetTotalMemory(false);
			//Debug.Log("GC: " + (gcMemory / 1024) + " " + (gcMemory - lastGcMemory));
			if ((gcMemory - lastGcMemory) < 0)
			{
				Debug.LogWarning("COLLECTION!!! " + (lastGcMemory - gcMemory));
			}
			lastGcMemory = gcMemory;*/
#endif
		}

		//long lastGcMemory = 0;

		void UpdateLoading()
		{
			// Finished loading?
			if (IsAllVideosLoaded())
			{
				// Assign the master and slaves
				_masterPlayer.BufferedDisplay.SetBufferedDisplayMode(BufferedFrameSelectionMode.ElapsedTimeVsynced);

				IBufferedDisplay[] slaves = new IBufferedDisplay[_slavePlayers.Length];
				for (int i = 0; i < _slavePlayers.Length; i++)
				{
					slaves[i] = _slavePlayers[i].BufferedDisplay;
				}
				_masterPlayer.BufferedDisplay.SetSlaves(slaves);

				//System.Threading.Thread.Sleep(1250);

				// Begin preroll
				PlayAll();

				_state = State.Prerolling;
			}
		}

		void UpdatePrerolling()
		{
			if (IsAllVideosPaused())
			{
				//System.Threading.Thread.Sleep(250);

				if (_waitAfterPreroll)
				{
					_state = State.Prerolled;
				}
				else
				{
					PlayAll();
					_state = State.Playing;
				}
			}
		}

		void UpdatePlaying()
		{
			if (_masterPlayer.Control.IsPlaying())
			{
				if (_logSyncErrors)
				{
					CheckSync();
					CheckSmoothness();
				}

				BufferedFramesState state = _masterPlayer.BufferedDisplay.GetBufferedFramesState();
				if (state.bufferedFrameCount < 3)
				{
					//Debug.LogWarning("FORCE SLEEP");
					System.Threading.Thread.Sleep(16);
				}
			}
			else
			{
				// Pause slaves
				for (int i = 0; i < _slavePlayers.Length; i++)
				{
					MediaPlayer slave = _slavePlayers[i];
					slave.Pause();
				}
			}

			// Finished?
			if (IsPlaybackFinished(_masterPlayer))
			{
				_state = State.Finished;
			}
		}

		private long _lastTimeStamp;
		private int _sameFrameCount;

		void CheckSmoothness()
		{
			long timeStamp = _masterPlayer.TextureProducer.GetTextureTimeStamp();
			//int frameCount = _masterPlayer.TextureProducer.GetTextureFrameCount();
			long frameDuration = (long)(10000000f / _masterPlayer.Info.GetVideoFrameRate());

			long vsyncDuration = (long)((QualitySettings.vSyncCount * 10000000f) / (float)Screen.currentResolution.refreshRate);
			float vsyncFrames = (float)vsyncDuration / frameDuration;

			float fractionalFrames = vsyncFrames - Mathf.FloorToInt(vsyncFrames);

			if (fractionalFrames == 0f)
			{
				if (QualitySettings.vSyncCount != 0)
				{
					if (!Mathf.Approximately(_sameFrameCount, vsyncFrames))
					{
						Debug.LogWarning("Frame " + timeStamp + " was shown for " + _sameFrameCount + " frames instead of expected " + vsyncFrames);
					}
				}
			}

			long d = (timeStamp - _lastTimeStamp);
			if (d != 0)
			{
				long threshold = 10000;
				if (d > frameDuration + threshold ||
					d < frameDuration - threshold)
				{
					Debug.LogWarning("Possible frame skip, " + timeStamp + " " + d);
				}

				_sameFrameCount = 1;
			}
			else
			{
				_sameFrameCount++;
			}

			_lastTimeStamp = timeStamp;
			//Debug.Log(frameDuration);
		}

		void CheckSync()
		{
			long timeStamp = _masterPlayer.TextureProducer.GetTextureTimeStamp();

			bool inSync = true;
			foreach (MediaPlayer slavePlayer in _slavePlayers)
			{
				if (slavePlayer.TextureProducer.GetTextureTimeStamp() != timeStamp)
				{
					inSync = false;
					break;
				}
			}

			if (!inSync)
			{
				LogSyncState();
				Debug.LogWarning("OUT OF SYNC!!!!!!!");
				//Debug.Break();
			}
			else
			{
				//LogSyncState();
			}
		}

		void LogSyncState()
		{
			string text = "Time - Full,Free\t\tRange\n";
			text += LogSyncState(_masterPlayer) + "\n";
			foreach (MediaPlayer slavePlayer in _slavePlayers)
			{
				text += LogSyncState(slavePlayer) + "\n";
			}
			Debug.Log(text);
		}

		string LogSyncState(MediaPlayer player)
		{
			BufferedFramesState state = player.BufferedDisplay.GetBufferedFramesState();
			long timeStamp = player.TextureProducer.GetTextureTimeStamp();
			string result = string.Format("{4} - {2},{3}\t\t{0}-{1}   ({5})", state.minTimeStamp, state.maxTimeStamp, state.bufferedFrameCount, state.freeFrameCount, timeStamp, Time.deltaTime);
			return result;
		}

		void OpenMediaAll()
		{
			_masterPlayer.OpenMedia(autoPlay:false);
			for (int i = 0; i < _slavePlayers.Length; i++)
			{
				_slavePlayers[i].OpenMedia(autoPlay:false);
			}
		}

		void PauseAll()
		{
			_masterPlayer.Pause();
			for (int i = 0; i < _slavePlayers.Length; i++)
			{
				_slavePlayers[i].Pause();
			}
		}

		void PlayAll()
		{
			_masterPlayer.Play();
			for (int i = 0; i < _slavePlayers.Length; i++)
			{
				_slavePlayers[i].Play();
			}
		}

		void SeekAll(double time)
		{
			 _masterPlayer.Control.Seek(time);
			foreach (MediaPlayer player in _slavePlayers)
			{
				player.Control.Seek(time);
			}
		}

		void SeekFastAll(double time)
		{
			_masterPlayer.Control.SeekFast(time);
			foreach (MediaPlayer player in _slavePlayers)
			{
				player.Control.SeekFast(time);
			}
		}

		bool IsAllVideosLoaded()
		{
			bool result = false;
			if (IsVideoLoaded(_masterPlayer))
			{
				result = true;
				for (int i = 0; i < _slavePlayers.Length; i++)
				{
					if (!IsVideoLoaded(_slavePlayers[i]))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		bool IsAllVideosPaused()
		{
			bool result = false;
			if (IsVideoPaused(_masterPlayer))
			{
				result = true;
				for (int i = 0; i < _slavePlayers.Length; i++)
				{
					if (!IsVideoPaused(_slavePlayers[i]))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}
		static bool IsPlaybackFinished(MediaPlayer player)
		{
			bool result = false;
			if (player != null && player.Control != null)
			{
				if (player.Control.IsFinished())
				{
					BufferedFramesState state = player.BufferedDisplay.GetBufferedFramesState();
					if (state.bufferedFrameCount == 0)
					{
						result = true;
					}
				}
			}
			return result;
		}

		static bool IsVideoLoaded(MediaPlayer player)
		{
			return (player != null && player.Control != null && player.Control.HasMetaData() && player.Control.CanPlay());
		}
		static bool IsVideoPaused(MediaPlayer player)
		{
			return (player != null && player.Control != null && player.Control.IsPaused());
		}
	}
}
#endif