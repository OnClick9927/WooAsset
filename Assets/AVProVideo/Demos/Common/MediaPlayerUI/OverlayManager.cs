// UnityEngine.UI was moved to a package in 2019.2.0
// Unfortunately no way to test for this across all Unity versions yet
// You can set up the asmdef to reference the new package, but the package doesn't 
// existing in Unity 2017 etc, and it throws an error due to missing reference
#define AVPRO_PACKAGE_UNITYUI
#if (UNITY_2019_2_OR_NEWER && AVPRO_PACKAGE_UNITYUI) || (!UNITY_2019_2_OR_NEWER)

using UnityEngine;
using UnityEngine.UI;

//-----------------------------------------------------------------------------
// Copyright 2018-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Demos.UI
{
	public class OverlayManager : MonoBehaviour
	{
		public enum Feedback
		{
			Play,
			Pause,
			SeekForward,
			SeekBack,
			VolumeUp,
			VolumeDown,
			VolumeMute,
		}

		[SerializeField] Image _stalledImage = null;
		[SerializeField] Image _feedbackImage = null;
		[SerializeField] CanvasGroup _feedbackCanvas = null;
		[SerializeField] float _startScale = 0.25f;
		[SerializeField] float _endScale = 1.0f;
		[SerializeField] float _animationSpeed = 1.5f;

		private Material _feedbackMaterial;
		private float _feedbackTimer;

		private readonly LazyShaderProperty _propMute = new LazyShaderProperty("_Mute");
		private readonly LazyShaderProperty _propVolume = new LazyShaderProperty("_Volume");

		void Start()
		{
			_feedbackMaterial = new Material(_feedbackImage.material);
			_feedbackImage.material = _feedbackMaterial;
			_feedbackCanvas.alpha = 0f;
			_feedbackTimer = 1f;
		}

		void OnDestroy()
		{
			if (_feedbackMaterial)
			{
				Material.Destroy(_feedbackMaterial);
				_feedbackMaterial = null;
			}
		}

		public void Reset()
		{
			_stalledImage.enabled = false;
		}

		public void TriggerStalled()
		{
			_stalledImage.enabled = true;
		}

		private const string KeywordPlay = "UI_PLAY";
		private const string KeywordPause = "UI_PAUSE";
		private const string KeywordSeekBack = "UI_BACK";
		private const string KeywordSeekForward = "UI_FORWARD";
		private const string KeywordVolume = "UI_VOLUME";
		private const string KeywordVolumeUp = "UI_VOLUMEUP";
		private const string KeywordVolumeDown = "UI_VOLUMEDOWN";
		private const string KeywordVolumeMute = "UI_VOLUMEMUTE";

		public void TriggerFeedback(Feedback feedback)
		{
			_feedbackMaterial.DisableKeyword(KeywordPlay);
			_feedbackMaterial.DisableKeyword(KeywordPause);
			_feedbackMaterial.DisableKeyword(KeywordSeekBack);
			_feedbackMaterial.DisableKeyword(KeywordSeekForward);
			_feedbackMaterial.DisableKeyword(KeywordVolume);
			_feedbackMaterial.DisableKeyword(KeywordVolumeUp);
			_feedbackMaterial.DisableKeyword(KeywordVolumeDown);
			_feedbackMaterial.DisableKeyword(KeywordVolumeMute);

			string keyword = null;
			switch (feedback)
			{
				case Feedback.Play:
					keyword = KeywordPlay;
					break;
				case Feedback.Pause:
					keyword = KeywordPause;
					break;
				case Feedback.SeekBack:
					keyword = KeywordSeekBack;
					break;
				case Feedback.SeekForward:
					keyword = KeywordSeekForward;
					break;
				case Feedback.VolumeUp:
					keyword = KeywordVolume;
					_feedbackMaterial.SetFloat(_propMute.Id, 0f);
					_feedbackMaterial.SetFloat(_propVolume.Id, 1f);
					break;
				case Feedback.VolumeDown:
					keyword = KeywordVolume;
					_feedbackMaterial.SetFloat(_propMute.Id, 0f);
					_feedbackMaterial.SetFloat(_propVolume.Id, 0.5f);
					break;
				case Feedback.VolumeMute:
					keyword = KeywordVolume;
					_feedbackMaterial.SetFloat(_propVolume.Id, 1f);
					_feedbackMaterial.SetFloat(_propMute.Id, 1f);
					break;
			}

			if (!string.IsNullOrEmpty(keyword))
			{
				_feedbackMaterial.EnableKeyword(keyword);
			}

			_feedbackCanvas.alpha = 1f;
			_feedbackCanvas.transform.localScale = new Vector3(_startScale, _startScale, _startScale);
			_feedbackTimer = 0f;
			Update();
		}

		private void Update()
		{
			// Update scaling and fading
			float t2 = Mathf.Clamp01(_feedbackTimer);
			float t = Mathf.Clamp01((_feedbackTimer - 0.5f) * 2f);
			_feedbackCanvas.alpha = Mathf.Lerp(1f, 0f, PowerEaseOut(t, 1f));
			if (_feedbackCanvas.alpha > 0f)
			{
				float scale = Mathf.Lerp(_startScale, _endScale, PowerEaseOut(t2, 2f));
				_feedbackCanvas.transform.localScale = new Vector3(scale, scale, scale);
			}

			_feedbackTimer += Time.deltaTime * _animationSpeed;
		}

		private static float PowerEaseOut(float t, float power)
		{
			return 1f - Mathf.Abs(Mathf.Pow(t - 1f, power));
		}
	}
}
#endif