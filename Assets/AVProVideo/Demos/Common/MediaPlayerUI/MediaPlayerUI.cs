// UnityEngine.UI was moved to a package in 2019.2.0
// Unfortunately no way to test for this across all Unity versions yet
// You can set up the asmdef to reference the new package, but the package doesn't 
// existing in Unity 2017 etc, and it throws an error due to missing reference
#define AVPRO_PACKAGE_UNITYUI
#if (UNITY_2019_2_OR_NEWER && AVPRO_PACKAGE_UNITYUI) || (!UNITY_2019_2_OR_NEWER)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RenderHeads.Media.AVProVideo;
using RenderHeads.Media.AVProVideo.Demos.UI;

//-----------------------------------------------------------------------------
// Copyright 2018-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Demos
{
	public class MediaPlayerUI : MonoBehaviour
	{
		[SerializeField] MediaPlayer _mediaPlayer = null;

		[Header("Options")]

		[SerializeField] float _keyVolumeDelta = 0.05f;
		[SerializeField] float _jumpDeltaTime = 5f;
		[SerializeField] bool _showOptions = true;
		[SerializeField] bool _autoHide = true;
		[SerializeField] float _userInactiveDuration = 1.5f;
		[SerializeField] bool _useAudioFading = true;

		[Header("Keyboard Controls")]
		[SerializeField] bool _enableKeyboardControls = true;
		[SerializeField] KeyCode KeyVolumeUp = KeyCode.UpArrow;
		[SerializeField] KeyCode KeyVolumeDown = KeyCode.DownArrow;
		[SerializeField] KeyCode KeyTogglePlayPause = KeyCode.Space;
		[SerializeField] KeyCode KeyToggleMute = KeyCode.M;
		[SerializeField] KeyCode KeyJumpForward = KeyCode.RightArrow;
		[SerializeField] KeyCode KeyJumpBack = KeyCode.LeftArrow;

		[Header("Optional Components")]
		[SerializeField] OverlayManager _overlayManager = null;
		[SerializeField] MediaPlayer _thumbnailMediaPlayer = null;
		[SerializeField] RectTransform _timelineTip = null;

		[Header("UI Components")]
		[SerializeField] RectTransform _canvasTransform = null;
		//[SerializeField] Image image = null;
		[SerializeField] Slider _sliderTime = null;
		[SerializeField] EventTrigger _videoTouch = null;
		[SerializeField] CanvasGroup _controlsGroup = null;

		[Header("UI Components (Optional)")]
		[SerializeField] GameObject _liveItem = null;
		[SerializeField] Text _textMediaName = null;
		[SerializeField] Text _textTimeDuration = null;
		[SerializeField] Slider _sliderVolume = null;
		[SerializeField] Button _buttonPlayPause = null;
		[SerializeField] Button _buttonVolume = null;
		[SerializeField] Button _buttonSubtitles = null;
		[SerializeField] Button _buttonOptions = null;
		[SerializeField] Button _buttonTimeBack = null;
		[SerializeField] Button _buttonTimeForward = null;
		[SerializeField] RawImage _imageAudioSpectrum = null;
		[SerializeField] GameObject _optionsMenuRoot = null;
		[SerializeField] HorizontalSegmentsPrimitive _segmentsSeek = null;
		[SerializeField] HorizontalSegmentsPrimitive _segmentsBuffered = null;
		[SerializeField] HorizontalSegmentsPrimitive _segmentsProgress = null;

		private bool _wasPlayingBeforeTimelineDrag;
		private float _controlsFade = 1f;
		private Material _playPauseMaterial;
		private Material _volumeMaterial;
		private Material _subtitlesMaterial;
		private Material _optionsMaterial;
		private Material _audioSpectrumMaterial;
		private float[] _spectrumSamples = new float[128];
		private float[] _spectrumSamplesSmooth = new float[128];
		private float _maxValue = 1f;
		private float _audioVolume = 1f;

		private float _audioFade = 0f;
		private bool _isAudioFadingUpToPlay = true;
		private const float AudioFadeDuration = 0.25f;
		private float _audioFadeTime = 0f;

		private readonly LazyShaderProperty _propMorph = new LazyShaderProperty("_Morph");
		private readonly LazyShaderProperty _propMute = new LazyShaderProperty("_Mute");
		private readonly LazyShaderProperty _propVolume = new LazyShaderProperty("_Volume");
		private readonly LazyShaderProperty _propSpectrum = new LazyShaderProperty("_Spectrum");
		private readonly LazyShaderProperty _propSpectrumRange = new LazyShaderProperty("_SpectrumRange");

		void Awake()
		{
			#if UNITY_IOS
			Application.targetFrameRate = 60;
			#endif
		}

		void Start()
		{
			if (_mediaPlayer)
			{
				_audioVolume = _mediaPlayer.AudioVolume;
			}
			SetupPlayPauseButton();
			SetupTimeBackForwardButtons();
			SetupVolumeButton();
			SetupSubtitlesButton();
			SetupOptionsButton();
			SetupAudioSpectrum();
			CreateTimelineDragEvents();
			CreateVideoTouchEvents();
			CreateVolumeSliderEvents();
			UpdateVolumeSlider();
			BuildOptionsMenu();
		}

		private struct UserInteraction
		{
			public static float InactiveTime;
			private static Vector3 _previousMousePos;
			private static int _lastInputFrame;

			public static bool IsUserInputThisFrame()
			{
				if (Time.frameCount == _lastInputFrame)
				{
					return true;
				}
				#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
				bool touchInput = (Input.touchSupported && Input.touchCount > 0);
				bool mouseInput = (Input.mousePresent && (Input.mousePosition != _previousMousePos || Input.mouseScrollDelta != Vector2.zero || Input.GetMouseButton(0)));

				if (touchInput || mouseInput)
				{
					_previousMousePos = Input.mousePosition;
					_lastInputFrame = Time.frameCount;
					return true;
				}

				return false;
				#else
				return true;
				#endif
			}
		}

		private Material DuplicateMaterialOnImage(Graphic image)
		{
			// Assign a copy of the material so we aren't modifying the material asset file
			image.material = new Material(image.material);
			return image.material;
		}

		private void SetupPlayPauseButton()
		{
			if (_buttonPlayPause)
			{
				_buttonPlayPause.onClick.AddListener(OnPlayPauseButtonPressed);
				_playPauseMaterial = DuplicateMaterialOnImage(_buttonPlayPause.GetComponent<Image>());
			}
		}

		private void SetupTimeBackForwardButtons()
		{
			if (_buttonTimeBack)
			{
				_buttonTimeBack.onClick.AddListener(OnPlayTimeBackButtonPressed);
			}
			if (_buttonTimeForward)
			{
				_buttonTimeForward.onClick.AddListener(OnPlayTimeForwardButtonPressed);
			}
		}

		private void SetupVolumeButton()
		{
			if (_buttonVolume)
			{
				_buttonVolume.onClick.AddListener(OnVolumeButtonPressed);
				_volumeMaterial = DuplicateMaterialOnImage(_buttonVolume.GetComponent<Image>());
			}
		}

		private void SetupSubtitlesButton()
		{
			if (_buttonSubtitles)
			{
				_buttonSubtitles.onClick.AddListener(OnSubtitlesButtonPressed);
				_subtitlesMaterial = DuplicateMaterialOnImage(_buttonSubtitles.GetComponent<Image>());
			}
		}

		private void SetupOptionsButton()
		{
			if (_buttonOptions)
			{
				_buttonOptions.onClick.AddListener(OnOptionsButtonPressed);
				_optionsMaterial = DuplicateMaterialOnImage(_buttonOptions.GetComponent<Image>());
			}
		}

		private void SetupAudioSpectrum()
		{
			if (_imageAudioSpectrum)
			{
				_audioSpectrumMaterial = DuplicateMaterialOnImage(_imageAudioSpectrum);
			}
		}

		private void OnPlayPauseButtonPressed()
		{
			TogglePlayPause();
		}

		private void OnPlayTimeBackButtonPressed()
		{
			SeekRelative(-_jumpDeltaTime);
		}

		private void OnPlayTimeForwardButtonPressed()
		{
			SeekRelative(_jumpDeltaTime);
		}

		private void OnVolumeButtonPressed()
		{
			ToggleMute();
		}

		private void OnSubtitlesButtonPressed()
		{
			ToggleSubtitles();
		}

		private void OnOptionsButtonPressed()
		{
			ToggleOptionsMenu();
		}

		private bool _isHoveringOverTimeline;

		private void OnTimelineBeginHover(PointerEventData eventData)
		{
			if (eventData.pointerCurrentRaycast.gameObject != null)
			{
				_isHoveringOverTimeline = true;
				_sliderTime.transform.localScale = new Vector3(1f, 2.5f, 1f);
			}
		}

		private void OnTimelineEndHover(PointerEventData eventData)
		{
			_isHoveringOverTimeline = false;
			_sliderTime.transform.localScale = new Vector3(1f, 1f, 1f);
		}

		private void CreateVideoTouchEvents()
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerUp;
			entry.callback.AddListener((data) => { OnVideoPointerUp(); });
			_videoTouch.triggers.Add(entry);
		}

		private void OnVideoPointerUp()
		{
			bool controlsMostlyVisible = (_controlsGroup.alpha >= 0.5f && _controlsGroup.gameObject.activeSelf);
			if (controlsMostlyVisible)
			{
				TogglePlayPause();
			}
		}

		void UpdateAudioFading()
		{
			// Increment fade timer
			if (_audioFadeTime < AudioFadeDuration)
			{
				_audioFadeTime = Mathf.Clamp(_audioFadeTime + Time.deltaTime, 0f, AudioFadeDuration);
			}

			// Trigger pause when audio faded down
			if (_audioFadeTime >= AudioFadeDuration)
			{
				if (!_isAudioFadingUpToPlay)
				{
					Pause(skipFeedback:true);
				}
			}

			// Apply audio fade value
			if (_mediaPlayer.Control != null && _mediaPlayer.Control.IsPlaying())
			{
				_audioFade = Mathf.Clamp01(_audioFadeTime / AudioFadeDuration);
				if (!_isAudioFadingUpToPlay)
				{
					_audioFade = (1f - _audioFade);
				}
				ApplyAudioVolume();
			}
		}

		public void TogglePlayPause()
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				if (_useAudioFading && _mediaPlayer.Info.HasAudio())
				{
					if (_mediaPlayer.Control.IsPlaying())
					{
						if (_overlayManager)
						{
							_overlayManager.TriggerFeedback(OverlayManager.Feedback.Pause);
						}
						_isAudioFadingUpToPlay = false;
					}
					else
					{
						_isAudioFadingUpToPlay = true;
						Play();
					}
					_audioFadeTime = 0f;
				}
				else
				{
					if (_mediaPlayer.Control.IsPlaying())
					{
						Pause();
					}
					else
					{
						Play();
					}
				}
			}
		}

		private void Play()
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				if (_overlayManager)
				{
					_overlayManager.TriggerFeedback(OverlayManager.Feedback.Play);
				}
				_mediaPlayer.Play();
			}
		}

		private void Pause(bool skipFeedback = false)
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				if (!skipFeedback)
				{
					if (_overlayManager)
					{
						_overlayManager.TriggerFeedback(OverlayManager.Feedback.Pause);
					}
				}
				_mediaPlayer.Pause();
			}
		}

		public void SeekRelative(float deltaTime)
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				TimeRange timelineRange = GetTimelineRange();
				double time = _mediaPlayer.Control.GetCurrentTime() + deltaTime;
				time = System.Math.Max(time, timelineRange.startTime);
				time = System.Math.Min(time, timelineRange.startTime + timelineRange.duration);
				_mediaPlayer.Control.Seek(time);

				if (_overlayManager)
				{
					_overlayManager.TriggerFeedback(deltaTime > 0f ? OverlayManager.Feedback.SeekForward : OverlayManager.Feedback.SeekBack);
				}
			}
		}

		public void ChangeAudioVolume(float delta)
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				// Change volume
				_audioVolume = Mathf.Clamp01(_audioVolume + delta);

				// Update the UI
				UpdateVolumeSlider();

				// Trigger the overlays
				if (_overlayManager)
				{
					_overlayManager.TriggerFeedback(delta > 0f ? OverlayManager.Feedback.VolumeUp : OverlayManager.Feedback.VolumeDown);
				}
			}
		}

		public void ToggleMute()
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				if (_mediaPlayer.Control.IsMuted())
				{
					MuteAudio(false);
				}
				else
				{
					MuteAudio(true);
				}
			}
		}

		private void MuteAudio(bool mute)
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				// Change mute
				_mediaPlayer.Control.MuteAudio(mute);

				// Update the UI
				// The UI element is constantly updated by the Update() method

				// Trigger the overlays
				if (_overlayManager)
				{
					_overlayManager.TriggerFeedback(mute ? OverlayManager.Feedback.VolumeMute : OverlayManager.Feedback.VolumeUp);
				}
			}
		}

		public void ToggleSubtitles()
		{
			if (_mediaPlayer && _mediaPlayer.TextTracks != null)
			{
				if (_mediaPlayer.TextTracks.GetTextTracks().Count > 0)
				{
					if (_mediaPlayer.TextTracks.GetActiveTextTrack() != null)
					{
						_mediaPlayer.TextTracks.SetActiveTextTrack(null);
					}
					else
					{
						// TODO: instead of activating the first one, base it on the language/track 
						// selection stored in the MediaPlayerUI
						_mediaPlayer.TextTracks.SetActiveTextTrack(_mediaPlayer.TextTracks.GetTextTracks()[0]);
					}
				}
			}
		}

		private void ToggleOptionsMenu()
		{
			_showOptions = !_showOptions;
			BuildOptionsMenu();
		}

		private void BuildOptionsMenu()
		{
			if (_optionsMenuRoot)
			{
				_optionsMenuRoot.SetActive(_showOptions);
			}
			// Temporary code for now disables to touch controls while the debug menu
			// is shown, to stop it consuming mouse input for IMGUI
			_videoTouch.enabled = !_showOptions;
		}

		private void CreateTimelineDragEvents()
		{
			EventTrigger trigger = _sliderTime.gameObject.GetComponent<EventTrigger>();
			if (trigger != null)
			{
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerDown;
				entry.callback.AddListener((data) => { OnTimeSliderBeginDrag(); });
				trigger.triggers.Add(entry);

				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.Drag;
				entry.callback.AddListener((data) => { OnTimeSliderDrag(); });
				trigger.triggers.Add(entry);

				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerUp;
				entry.callback.AddListener((data) => { OnTimeSliderEndDrag(); });
				trigger.triggers.Add(entry);

				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerEnter;
				entry.callback.AddListener((data) => { OnTimelineBeginHover((PointerEventData)data); });
				trigger.triggers.Add(entry);

				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerExit;
				entry.callback.AddListener((data) => { OnTimelineEndHover((PointerEventData)data); });
				trigger.triggers.Add(entry);
			}
		}

		private void CreateVolumeSliderEvents()
		{
			if (_sliderVolume != null)
			{
				EventTrigger trigger = _sliderVolume.gameObject.GetComponent<EventTrigger>();
				if (trigger != null)
				{
					EventTrigger.Entry entry = new EventTrigger.Entry();
					entry.eventID = EventTriggerType.PointerDown;
					entry.callback.AddListener((data) => { OnVolumeSliderDrag(); });
					trigger.triggers.Add(entry);

					entry = new EventTrigger.Entry();
					entry.eventID = EventTriggerType.Drag;
					entry.callback.AddListener((data) => { OnVolumeSliderDrag(); });
					trigger.triggers.Add(entry);
				}
			}
		}

		private void OnVolumeSliderDrag()
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				_audioVolume = _sliderVolume.value;
				ApplyAudioVolume();
			}
		}

		private void ApplyAudioVolume()
		{
			if (_mediaPlayer)
			{
				_mediaPlayer.AudioVolume = (_audioVolume * _audioFade);
			}
		}

		private void UpdateVolumeSlider()
		{
			if (_sliderVolume)
			{
				if (_mediaPlayer)
				{
					// TODO: remove this
					/*if (mp.Control != null)
					{
						_sliderVolume.value = mp.Control.GetVolume();
					}
					else*/
					{
						_sliderVolume.value = _audioVolume;
					}
				}
			}
		}

		private void UpdateAudioSpectrum()
		{
			bool showAudioSpectrum = false;
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				AudioSource audioSource = _mediaPlayer.AudioSource;
				if (audioSource && _audioSpectrumMaterial)
				{
					showAudioSpectrum = true;

					float maxFreq = (Helper.GetUnityAudioSampleRate() / 2);

					// Frequencies over 18Khz generally aren't very interesting to visualise, so clamp the range
					const float clampFreq = 18000f;
					int sampleRange = Mathf.FloorToInt(Mathf.Clamp01(clampFreq / maxFreq) * _spectrumSamples.Length);

					// Add new samples and smooth the samples over time
					audioSource.GetSpectrumData(_spectrumSamples, 0, FFTWindow.BlackmanHarris);

					// Find the maxValue sample for normalising with
					float maxValue = -1.0f;
					for (int i = 0; i < sampleRange; i++)
					{
						if (_spectrumSamples[i] > maxValue)
						{
							maxValue = _spectrumSamples[i];
						}
					}

					// Chase maxValue to zero
					_maxValue = Mathf.Lerp(_maxValue, 0.0f, Mathf.Clamp01(2.0f * Time.deltaTime));

					// Update maxValue
					_maxValue = Mathf.Max(_maxValue, maxValue);
					if (_maxValue <= 0.01f)
					{
						_maxValue = 1f;
					}

					// Copy and smooth the spectrum values
					for (int i = 0; i < sampleRange; i++)
					{
						float newSample = _spectrumSamples[i] / _maxValue;
						_spectrumSamplesSmooth[i] = Mathf.Lerp(_spectrumSamplesSmooth[i], newSample, Mathf.Clamp01(15.0f * Time.deltaTime));
					}

					// Update shader
					_audioSpectrumMaterial.SetFloatArray(_propSpectrum.Id, _spectrumSamplesSmooth);
					_audioSpectrumMaterial.SetFloat(_propSpectrumRange.Id, (float)sampleRange);
				}
			}

			if (_imageAudioSpectrum)
			{
				_imageAudioSpectrum.gameObject.SetActive(showAudioSpectrum);
			}
		}

		private void OnTimeSliderBeginDrag()
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				_wasPlayingBeforeTimelineDrag = _mediaPlayer.Control.IsPlaying();
				if (_wasPlayingBeforeTimelineDrag)
				{
					_mediaPlayer.Pause();
				}
				OnTimeSliderDrag();
			}
		}

		private void OnTimeSliderDrag()
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				TimeRange timelineRange = GetTimelineRange();
				double time = timelineRange.startTime + (_sliderTime.value * timelineRange.duration);
				_mediaPlayer.Control.Seek(time);
				_isHoveringOverTimeline = true;
			}
		}

		private void OnTimeSliderEndDrag()
		{
			if (_mediaPlayer && _mediaPlayer.Control != null)
			{
				if (_wasPlayingBeforeTimelineDrag)
				{
					_mediaPlayer.Play();
					_wasPlayingBeforeTimelineDrag = false;
				}
			}
		}

		private TimeRange GetTimelineRange()
		{
			if (_mediaPlayer.Info != null)
			{
				return Helper.GetTimelineRange(_mediaPlayer.Info.GetDuration(), _mediaPlayer.Control.GetSeekableTimes());
			}
			return new TimeRange();
		}

		private bool CanHideControls()
		{
			bool result = true;
			if (!_autoHide)
			{
				result = false;
			}
			#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
			else if (Input.mousePresent)
			{
				// Check whether the mouse cursor is over the controls, in which case we can't hide the UI
				RectTransform rect = _controlsGroup.GetComponent<RectTransform>();
				Vector2 canvasPos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out canvasPos);

				Rect rr = RectTransformUtility.PixelAdjustRect(rect, null);
				result = !rr.Contains(canvasPos);
			}
			#endif
			return result;
		}

		private void UpdateControlsVisibility()
		{
			if (UserInteraction.IsUserInputThisFrame() || !CanHideControls())
			{
				UserInteraction.InactiveTime = 0f;
				FadeUpControls();
			}
			else
			{

				UserInteraction.InactiveTime += Time.unscaledDeltaTime;
				if (UserInteraction.InactiveTime >= _userInactiveDuration)
				{
					FadeDownControls();
				}
				else
				{
					FadeUpControls();
				}
			}
		}

		private void FadeUpControls()
		{
			if (!_controlsGroup.gameObject.activeSelf)
			{
				_controlsGroup.gameObject.SetActive(true);
			}
			_controlsFade = Mathf.Min(1f, _controlsFade + Time.deltaTime * 8f);
			_controlsGroup.alpha = Mathf.Pow(_controlsFade, 5f);
		}

		private void FadeDownControls()
		{
			if (_controlsGroup.gameObject.activeSelf)
			{
				_controlsFade = Mathf.Max(0f, _controlsFade - Time.deltaTime * 3f);
				_controlsGroup.alpha = Mathf.Pow(_controlsFade, 5f);
				if (_controlsGroup.alpha <= 0f)
				{
					_controlsGroup.gameObject.SetActive(false);
				}
			}
		}

		void Update()
		{
			if (!_mediaPlayer) return;

			UpdateControlsVisibility();
			UpdateAudioFading();
			UpdateAudioSpectrum();

			if (_mediaPlayer.Info != null)
			{
				TimeRange timelineRange = GetTimelineRange();

				// Update timeline hover popup
				#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
				if (_timelineTip != null)
				{
					if (_isHoveringOverTimeline)
					{
						Vector2 canvasPos;
						RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, Input.mousePosition, null, out canvasPos);

						_segmentsSeek.gameObject.SetActive(true);
						_timelineTip.gameObject.SetActive(true);
						Vector3 mousePos = _canvasTransform.TransformPoint(canvasPos);

						_timelineTip.position = new Vector2(mousePos.x, _timelineTip.position.y);

						if (UserInteraction.IsUserInputThisFrame())
						{
							// Work out position on the timeline
							Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(this._sliderTime.GetComponent<RectTransform>());
							float x = Mathf.Clamp01((canvasPos.x - bounds.min.x) / bounds.size.x);
							
							double time = (double)x * timelineRange.Duration;

							// Seek to the new position
							if (_thumbnailMediaPlayer != null && _thumbnailMediaPlayer.Control != null)
							{
								_thumbnailMediaPlayer.Control.SeekFast(time);
							}

							// Update time text
							Text hoverText = _timelineTip.GetComponentInChildren<Text>();
							if (hoverText != null)
							{
								time -= timelineRange.startTime;
								time = System.Math.Max(time, 0.0);
								time = System.Math.Min(time, timelineRange.Duration);
								hoverText.text = Helper.GetTimeString(time, false);
							}

							{
								// Update seek segment when hovering over timeline
								if (_segmentsSeek != null)
								{
									float[] ranges = new float[2];
									if (timelineRange.Duration > 0.0)
									{
										double t = ((_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime) / timelineRange.duration);
										ranges[1] = x;
										ranges[0] = (float)t;
									}
									_segmentsSeek.Segments = ranges;
								}
							}
						}
					}
					else
					{
						_timelineTip.gameObject.SetActive(false);
						_segmentsSeek.gameObject.SetActive(false);
					}
				}
				#endif

				// Updated stalled display
				if (_overlayManager)
				{
					_overlayManager.Reset();
					if (_mediaPlayer.Info.IsPlaybackStalled())
					{
						_overlayManager.TriggerStalled();
					}
				}

				// Update keyboard input
				if (_enableKeyboardControls)
				{
					#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
					// Keyboard toggle play/pause
					if (Input.GetKeyDown(KeyTogglePlayPause))
					{
						TogglePlayPause();
					}

					// Keyboard seek 5 seconds
					if (Input.GetKeyDown(KeyJumpBack))
					{
						SeekRelative(-_jumpDeltaTime);
					}
					else if (Input.GetKeyDown(KeyJumpForward))
					{
						SeekRelative(_jumpDeltaTime);
					}

					// Keyboard control volume
					if (Input.GetKeyDown(KeyVolumeUp))
					{
						ChangeAudioVolume(_keyVolumeDelta);
					}
					else if (Input.GetKeyDown(KeyVolumeDown))
					{
						ChangeAudioVolume(-_keyVolumeDelta);
					}

					// Keyboard toggle mute
					if (Input.GetKeyDown(KeyToggleMute))
					{
						ToggleMute();
					}
					#endif
				}

				// Animation play/pause button
				if (_playPauseMaterial != null)
				{
					float t = _playPauseMaterial.GetFloat(_propMorph.Id);
					float d = 1f;
					if (_mediaPlayer.Control.IsPlaying())
					{
						d = -1f;
					}
					t += d * Time.deltaTime * 6f;
					t = Mathf.Clamp01(t);
					_playPauseMaterial.SetFloat(_propMorph.Id, t);
				}

				// Animation volume/mute button
				if (_volumeMaterial != null)
				{
					float t = _volumeMaterial.GetFloat(_propMute.Id);
					float d = 1f;
					if (!_mediaPlayer.Control.IsMuted())
					{
						d = -1f;
					}
					t += d * Time.deltaTime * 6f;
					t = Mathf.Clamp01(t);
					_volumeMaterial.SetFloat(_propMute.Id, t);
					_volumeMaterial.SetFloat(_propVolume.Id, _audioVolume);
				}

				// Animation subtitles button
				if (_subtitlesMaterial)
				{
					float t = _subtitlesMaterial.GetFloat(_propMorph.Id);
					float d = 1f;
					if (_mediaPlayer.TextTracks.GetActiveTextTrack() == null)
					{
						d = -1f;
					}
					t += d * Time.deltaTime * 6f;
					t = Mathf.Clamp01(t);
					_subtitlesMaterial.SetFloat(_propMorph.Id, t);
				}

				// Animation options button
				if (_optionsMaterial)
				{
					float t = _optionsMaterial.GetFloat(_propMorph.Id);
					float d = 1f;
					if (!_showOptions)
					{
						d = -1f;
					}
					t += d * Time.deltaTime * 6f;
					t = Mathf.Clamp01(t);
					_optionsMaterial.SetFloat(_propMorph.Id, t);
				}

				// Update time/duration text display
				if (_textTimeDuration)
				{
					string t1 = Helper.GetTimeString((_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime), false);
					string d1 = Helper.GetTimeString(timelineRange.duration, false);
					_textTimeDuration.text = string.Format("{0} / {1}", t1, d1);
				}

				// Update volume slider
				if (!_useAudioFading)
				{
					UpdateVolumeSlider();
				}

				// Update time slider position
				if (_sliderTime)
				{
					double t = 0.0;
					if (timelineRange.duration > 0.0)
					{
						t = ((_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime) / timelineRange.duration);
					} 
					_sliderTime.value = Mathf.Clamp01((float)t);
				}

				// Update LIVE text visible
				if (_liveItem)
				{
					_liveItem.SetActive(double.IsInfinity(_mediaPlayer.Info.GetDuration()));
				}

				// Update subtitle button visible
				if (_buttonSubtitles)
				{
					_buttonSubtitles.gameObject.SetActive(_mediaPlayer.TextTracks.GetTextTracks().Count > 0);
				}

				// Update media name
				if (_textMediaName)
				{
					#if MEDIA_NAME
					string mediaName = string.Empty;
					if (!string.IsNullOrEmpty(_mediaPlayer.VideoPath))
					{
						mediaName  = System.IO.Path.GetFileName(_mediaPlayer.VideoPath);
						if (mediaName.Length > 26)
						{
							mediaName = mediaName.Substring(0, 26);
						}
					}
					#endif

					string resolutionName = string.Empty;
					if (_mediaPlayer.Info.GetVideoWidth() > 0)
					{
						resolutionName = Helper.GetFriendlyResolutionName(_mediaPlayer.Info.GetVideoWidth(), _mediaPlayer.Info.GetVideoHeight(), _mediaPlayer.Info.GetVideoFrameRate());						
					}

					#if MEDIA_NAME
					_textMediaName.text = string.Format("{0} {1}", mediaName, resolutionName);
					#else
					_textMediaName.text = resolutionName;
					#endif
				}

				// Update buffered segments
				if (_segmentsBuffered)
				{
					TimeRanges times = _mediaPlayer.Control.GetBufferedTimes();
					float[] ranges = null;
					if (times.Count > 0 && timelineRange.duration > 0.0)
					{
						ranges = new float[times.Count * 2];
						for (int i = 0; i < times.Count; i++)
						{
							ranges[i * 2 + 0] = Mathf.Max(0f, (float)((times[i].StartTime - timelineRange.startTime) / timelineRange.duration));
							ranges[i * 2 + 1] = Mathf.Min(1f,(float)((times[i].EndTime - timelineRange.startTime) / timelineRange.duration));
						}
					}
					_segmentsBuffered.Segments = ranges;
				}

				// Update progress segment
				if (_segmentsProgress)
				{
					TimeRanges times = _mediaPlayer.Control.GetBufferedTimes();
					float[] ranges = null;
					if (times.Count > 0 && timelineRange.Duration > 0.0)
					{
						ranges = new float[2];
						double x1 = (times.MinTime - timelineRange.startTime) / timelineRange.duration;
						double x2 = ((_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime) / timelineRange.duration);
						ranges[0] = Mathf.Max(0f, (float)x1);
						ranges[1] = Mathf.Min(1f, (float)x2);
					}
					_segmentsProgress.Segments = ranges;
				}
			}
		}

		void OnGUI()
		{
			// NOTE: These this IMGUI is just temporary until we implement the UI using uGUI
			if (!_showOptions) return;
			if (!_mediaPlayer || _mediaPlayer.Control == null) return;

			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(2f, 2f, 1f));

			GUI.backgroundColor = Color.red;
			GUILayout.BeginVertical(GUI.skin.box);
			GUI.backgroundColor = Color.white;

			GUILayout.Label("Duration " + _mediaPlayer.Info.GetDuration() + "s");
			GUILayout.BeginHorizontal();
			GUILayout.Label("States: ");
			GUILayout.Toggle(_mediaPlayer.Control.HasMetaData(), "HasMetaData", GUI.skin.button);
			GUILayout.Toggle(_mediaPlayer.Control.IsPaused(), "Paused", GUI.skin.button);
			GUILayout.Toggle(_mediaPlayer.Control.IsPlaying(), "Playing", GUI.skin.button);
			GUILayout.Toggle(_mediaPlayer.Control.IsBuffering(), "Buffering", GUI.skin.button);
			GUILayout.Toggle(_mediaPlayer.Control.IsSeeking(), "Seeking", GUI.skin.button);
			GUILayout.Toggle(_mediaPlayer.Control.IsFinished(), "Finished", GUI.skin.button);
			GUILayout.EndHorizontal();

			{
				TimeRanges times = _mediaPlayer.Control.GetBufferedTimes();
				if (times != null)
				{
					GUILayout.Label("Buffered Range " + times.MinTime + " - " + times.MaxTime);
				}
			}
			{
				TimeRanges times = _mediaPlayer.Control.GetSeekableTimes();
				if (times != null)
				{
					GUILayout.Label("Seek Range " + times.MinTime + " - " + times.MaxTime);
				}
			}


			{
				GUILayout.Label("Video Tracks: " + _mediaPlayer.VideoTracks.GetVideoTracks().Count);

				GUILayout.BeginVertical();

				VideoTrack selectedTrack = null;
				foreach (VideoTrack track in _mediaPlayer.VideoTracks.GetVideoTracks())
				{
					bool isSelected = (track == _mediaPlayer.VideoTracks.GetActiveVideoTrack());
					if (isSelected) GUI.color= Color.green;
					if (GUILayout.Button(track.DisplayName, GUILayout.ExpandWidth(false)))
					{
						selectedTrack = track;
					}
					if (isSelected) GUI.color= Color.white;
				}
				GUILayout.EndHorizontal();
				if (selectedTrack != null)
				{
					_mediaPlayer.VideoTracks.SetActiveVideoTrack(selectedTrack);
				}
			}
			{
				GUILayout.Label("Audio Tracks: " + _mediaPlayer.AudioTracks.GetAudioTracks().Count);

				GUILayout.BeginVertical();

				AudioTrack selectedTrack = null;
				foreach (AudioTrack track in _mediaPlayer.AudioTracks.GetAudioTracks())
				{
					bool isSelected = (track == _mediaPlayer.AudioTracks.GetActiveAudioTrack());
					if (isSelected) GUI.color= Color.green;
					if (GUILayout.Button(track.DisplayName, GUILayout.ExpandWidth(false)))
					{
						selectedTrack = track;
					}
					if (isSelected) GUI.color= Color.white;
				}
				GUILayout.EndHorizontal();
				if (selectedTrack != null)
				{
					_mediaPlayer.AudioTracks.SetActiveAudioTrack(selectedTrack);
				}
			}
			{
				GUILayout.Label("Text Tracks: " + _mediaPlayer.TextTracks.GetTextTracks().Count);

				GUILayout.BeginVertical();

				TextTrack selectedTrack = null;
				foreach (TextTrack track in _mediaPlayer.TextTracks.GetTextTracks())
				{
					bool isSelected = (track == _mediaPlayer.TextTracks.GetActiveTextTrack());
					if (isSelected) GUI.color= Color.green;
					if (GUILayout.Button(track.DisplayName, GUILayout.ExpandWidth(false)))
					{
						selectedTrack = track;
					}
					if (isSelected) GUI.color= Color.white;
				}
				GUILayout.EndHorizontal();
				if (selectedTrack != null)
				{
					_mediaPlayer.TextTracks.SetActiveTextTrack(selectedTrack);
				}
			}
			{
				GUILayout.Label("FPS: " + _mediaPlayer.Info.GetVideoDisplayRate().ToString("F2"));
			}
#if (UNITY_STANDALONE_WIN)
			if (_mediaPlayer.PlatformOptionsWindows.bufferedFrameSelection != BufferedFrameSelectionMode.None)
			{
				IBufferedDisplay bufferedDisplay = _mediaPlayer.BufferedDisplay;
				if (bufferedDisplay != null)
				{
					BufferedFramesState state = bufferedDisplay.GetBufferedFramesState();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Buffered Frames: " + state.bufferedFrameCount);
					GUILayout.HorizontalSlider(state.bufferedFrameCount, 0f, 12f);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Free Frames: " + state.freeFrameCount);
					GUILayout.HorizontalSlider(state.freeFrameCount, 0f, 12f);
					GUILayout.EndHorizontal();
					GUILayout.Label("Min Timstamp: " + state.minTimeStamp);
					GUILayout.Label("Max Timstamp: " + state.maxTimeStamp);
					GUILayout.Label("Display Timstamp: " + _mediaPlayer.TextureProducer.GetTextureTimeStamp());
				}
			}
#endif
			GUILayout.EndVertical();
		}
	}
}
#endif