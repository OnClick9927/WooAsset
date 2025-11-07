// NOTE: We only allow this script to compile in editor so we can easily check for compilation issues
#if (UNITY_EDITOR || (UNITY_STANDALONE_WIN || UNITY_WSA_10_0))

#define AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
#if UNITY_WSA_10 || ENABLE_IL2CPP
	#define AVPROVIDEO_MARSHAL_RETURN_BOOL
#endif
#if UNITY_2019_3_OR_NEWER && !UNITY_2020_1_OR_NEWER
	#define AVPROVIDEO_FIX_UPDATEEXTERNALTEXTURE_LEAK
#endif

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using System.Text;

#if NETFX_CORE
using Windows.Storage.Streams;
#endif

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Windows desktop and UWP implementation of BaseMediaPlayer
	/// </summary>
	public /*sealed*/ partial class WindowsMediaPlayer : BaseMediaPlayer
	{

#region IBufferedDisplay Implementation

		private BufferedFrameSelectionMode _frameSelectionMode = BufferedFrameSelectionMode.None;
		private bool _pauseOnPrerollComplete = false;
		private IBufferedDisplay _masterDisplay;
		private IBufferedDisplay[] _slaveDisplays;
		private double _displayClockTime = 0.0;
		private double _timeAccumulation = 0.0;
		private bool _needsInitialFrame = true;

		private void FlushFrameBuffering(bool releaseTexture)
		{
			if (_frameSelectionMode == BufferedFrameSelectionMode.None) return;

			if (releaseTexture && _textureFrame.internalNativePointer != System.IntPtr.Zero)
			{
				Native.UnlockTextureFrame(_instance, ref _textureFrame);
				_textureFrame.internalNativePointer = System.IntPtr.Zero;
				_textureFrame.texturePointer = System.IntPtr.Zero;
			}

			Native.FlushFrameBuffering(_instance);

			// Native _pauseOnPrerollComplete needs to be reset
			//Native.SetFrameBufferingEnabled(_instance, (_frameSelectionMode != BufferedFrameSelectionMode.None), _pauseOnPrerollComplete);

			_needsInitialFrame = true;
			_timeAccumulation = 0.0;
		}

		internal override long InternalUpdateBufferedDisplay()
		{
			BufferedFramesState state = GetBufferedFramesState();
			if (state.bufferedFrameCount > 0)
			{ 
				if (_frameSelectionMode == BufferedFrameSelectionMode.NewestFrame)
				{
					SetBufferedDisplayTime(_frameSelectionMode, -1, false);
				}
				else if (_frameSelectionMode == BufferedFrameSelectionMode.OldestFrame)
				{
					SetBufferedDisplayTime(_frameSelectionMode, -1, false);
				}
				else if (_frameSelectionMode == BufferedFrameSelectionMode.ElapsedTime ||
						_frameSelectionMode == BufferedFrameSelectionMode.ElapsedTimeVsynced)
				{
					// Only start consuming frames on these conditions
					bool needsInitialFrame = (_textureFrame.texturePointer == System.IntPtr.Zero || _needsInitialFrame);
					bool playingBufferedFrames = (IsPrerollComplete() && (IsPlaying() || (!IsPlaying() && IsFinished())));
					if (needsInitialFrame || playingBufferedFrames)
					{
						if (needsInitialFrame)
						{
							if (SetBufferedDisplayTime(BufferedFrameSelectionMode.OldestFrame, -1, true))
							{
								_displayClockTime = _textureFrame.timeStamp;
								_needsInitialFrame = false;
							}
						}
						else
						{
							// TODO: run without vsync, just show next frame (use media clock for present?)
							// use our own clock...
							const double SecondsToHNS = 10000000.0;
							double videoFrameDuration = SecondsToHNS / (double)GetVideoFrameRate();
							long videoDuration = (long)Math.Floor(SecondsToHNS * GetDuration());
							long lastFrameTime = Math.Max(videoDuration, state.maxTimeStamp);
							double delta = SecondsToHNS * Time.deltaTime;

							if (_frameSelectionMode == BufferedFrameSelectionMode.ElapsedTimeVsynced && QualitySettings.vSyncCount > 0)
							{
								// Since we're running with vsync enabled, the MINIMUM elapsed time will be 1 monitor refresh (multiplied by QualitySettings.vSyncCount)
								double monitorDuration = (QualitySettings.vSyncCount * SecondsToHNS) / (double)Screen.currentResolution.refreshRate;

								int wholeFrames = (int)System.Math.Floor(_timeAccumulation / monitorDuration);
								wholeFrames = System.Math.Max(1, wholeFrames);
								delta = monitorDuration * wholeFrames;

								//LogBufferState();
								if (wholeFrames > 1)
								{
									//Debug.Log(Time.frameCount + "] " + Time.deltaTime + " " + wholeFrames + " " + _timeAccumulation + " " + _timeAccumulation / SecondsToHNS);
									//LogBufferState();
								}

								_timeAccumulation += (Time.deltaTime * SecondsToHNS) - delta;

								//delta = monitorDuration;

								/*double actualFrameDuration = Time.deltaTime * SecondsToHNS;
								double idealFrameTimeDifference = (actualFrameDuration);// - minMonitorDuration);
								if (idealFrameTimeDifference > (minMonitorDuration / 2))
								{
									int droppedFrames = (int)Math.Round(idealFrameTimeDifference / minMonitorDuration);
									//Debug.Log(Time.maximumDeltaTime + " " + Time.deltaTime + " " + actualFrameDuration + " " + idealFrameTimeDifference + " = " + droppedFrames);
									delta += minMonitorDuration * droppedFrames;
									
									//LogBufferState();
								}
								else
								{
									//Debug.Log(Time.deltaTime);
								}
								// If we're running slower than this or there is a frame drop, the elapsed time will be a multiple
								// of the monitor refresh rate
								*/
							}

							_displayClockTime += delta;

							int multiple = (int)videoFrameDuration;
							long snappedFrameTime = (long)Math.Floor(_displayClockTime / multiple) * multiple;
							if (_isLooping && snappedFrameTime > lastFrameTime)
							{
								snappedFrameTime %= lastFrameTime;
								_needsInitialFrame = true;
							}
							else
							{
								snappedFrameTime = Math.Min(snappedFrameTime, lastFrameTime);
							}

							if (System.Math.Abs(snappedFrameTime - _textureFrame.timeStamp) > 1000)
							{
								//Debug.Log("1 " + _displayClockTime + " > " + snappedFrameTime + " d:" + delta);
								//LogBufferState();

								if (_needsInitialFrame)
								{
									if (SetBufferedDisplayTime(BufferedFrameSelectionMode.OldestFrame, -1, true))
									{
										_displayClockTime = _textureFrame.timeStamp;
										//Debug.Log("initial: " + _displayClockTime);
										_needsInitialFrame = false;
									}
								}
								else if (!SetBufferedDisplayTime(_frameSelectionMode, snappedFrameTime, false))
								{
									//Debug.LogWarning("[AVProVideo] failed to set time at " + snappedFrameTime);
									//LogBufferState();

									// Try to snap to oldest buffered time
									_displayClockTime = (state.minTimeStamp + state.maxTimeStamp) / 2.0;
									snappedFrameTime = (long)Math.Floor(_displayClockTime / multiple) * multiple;
									if (_isLooping && snappedFrameTime > lastFrameTime)
									{
										snappedFrameTime %= lastFrameTime;
									}
									else
									{
										snappedFrameTime = Math.Min(snappedFrameTime, lastFrameTime);
									}

									if (SetBufferedDisplayTime(BufferedFrameSelectionMode.FromExternalTimeClosest, snappedFrameTime, false))
									{
										_displayClockTime = _textureFrame.timeStamp;
										//Debug.LogWarning("[AVProVideo] Good set: " + _displayClockTime);
									}
									else
									{
										//Debug.LogWarning("[AVProVideo] Failed to display frame time " + snappedFrameTime);
										//LogBufferState();
									}
								}
							}
						}
					}
				}
				else if (_frameSelectionMode == BufferedFrameSelectionMode.FromExternalTime)
				{
					if (_masterDisplay != null)
					{
						// Use the time from the master
						long timeStamp = _masterDisplay.UpdateBufferedDisplay();
						if (timeStamp != GetTextureTimeStamp())
						{
							if (!SetBufferedDisplayTime(BufferedFrameSelectionMode.FromExternalTimeClosest, timeStamp, false))
							{
								Debug.LogWarning("[AVProVideo] Failed to display frame using external clock at time " + timeStamp);
							}
						}
					}
				}
			}

			return GetTextureTimeStamp();
		}

		private void LogBufferState()
		{
			BufferedFramesState state = GetBufferedFramesState();
			long timeStamp = GetTextureTimeStamp();
			string result = string.Format("[AVProVideo] {4} - {2},{3}\t\t{0}-{1}   ({5})", state.minTimeStamp, state.maxTimeStamp, state.bufferedFrameCount, state.freeFrameCount, timeStamp, Time.deltaTime);
			Debug.Log(result);
		}

		private bool SetBufferedDisplayTime(BufferedFrameSelectionMode mode, long timeOfDesiredFrameToDisplay, bool ignorePreroll)
		{
			bool result = false;
			//if (!_isPaused)
			{
				result = Native.LockTextureFrame(_instance, mode, timeOfDesiredFrameToDisplay, ref _textureFrame, ignorePreroll);
			}
			return result;
		}

		public override BufferedFramesState GetBufferedFramesState()
		{
			BufferedFramesState state = new BufferedFramesState();
			Native.GetBufferedFramesState(_instance, ref state);
			return state;
		}

		public override void SetBufferedDisplayMode(BufferedFrameSelectionMode mode, IBufferedDisplay master = null)
		{
			_frameSelectionMode = mode;
			_masterDisplay = master;
			UpdateBufferedDisplay();
		}

		public override void SetBufferedDisplayOptions(bool pauseOnPrerollComplete)
		{
			_pauseOnPrerollComplete = pauseOnPrerollComplete;
			Native.SetFrameBufferingEnabled(_instance, (_frameSelectionMode != BufferedFrameSelectionMode.None), _pauseOnPrerollComplete);
		}

		public override void SetSlaves(IBufferedDisplay[] slaves)
		{
			foreach (IBufferedDisplay slave in slaves)
			{
				slave.SetBufferedDisplayMode(BufferedFrameSelectionMode.FromExternalTime, this);
			}
			_slaveDisplays = slaves;
		}

		private bool IsPrerollComplete()
		{
			bool result = true;
			
			if (GetBufferedFramesState().prerolledCount <= 0)
			{
				result = false;
			}
			if (_slaveDisplays != null && result)
			{
				foreach (IBufferedDisplay slave in _slaveDisplays)
				{
					if (slave.GetBufferedFramesState().prerolledCount <= 0)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		private partial struct Native
		{
			[DllImport("AVProVideo")]
			public static extern bool GetBufferedFramesState(System.IntPtr playerInstance, ref BufferedFramesState state);

			[DllImport("AVProVideo")]
#if AVPROVIDEO_MARSHAL_RETURN_BOOL
			[return: MarshalAs(UnmanagedType.I1)]
#endif
			public static extern bool LockTextureFrame(System.IntPtr instance, BufferedFrameSelectionMode mode, long time, ref TextureFrame textureFrame, bool ignorePreroll);

			[DllImport("AVProVideo")]
			public static extern void UnlockTextureFrame(System.IntPtr instance, ref TextureFrame textureFrame);

			[DllImport("AVProVideo")]
			public static extern void ReleaseTextureFrame(System.IntPtr instance, ref TextureFrame textureFrame);

			[DllImport("AVProVideo")]
			public static extern void FlushFrameBuffering(System.IntPtr instance);
		}

#endregion // IBufferedDisplay Implementation

	}
}
#endif