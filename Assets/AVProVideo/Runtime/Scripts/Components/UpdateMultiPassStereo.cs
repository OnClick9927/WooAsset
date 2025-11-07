using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// This script is needed to send the camera position to the stereo shader so that
	/// it can determine which eye it is rendering.  This is only needed for multi-pass
	/// rendering, as single pass has a built-in shader variable
	/// </summary>
	[AddComponentMenu("AVPro Video/Update Multi-Pass Stereo", 320)]
	[HelpURL("http://renderheads.com/products/avpro-video/")]
	public class UpdateMultiPassStereo : MonoBehaviour
	{
		[Header("Stereo camera")]

		[SerializeField] Camera _camera = null;

		public Camera Camera
		{
			get { return _camera; }
			set { _camera = value; }
		}

		private static readonly LazyShaderProperty PropCameraPosition = new LazyShaderProperty("_CameraPosition");
		private static readonly LazyShaderProperty PropViewMatrix = new LazyShaderProperty("_ViewMatrix");

		// State

		private Camera _foundCamera;

		void Awake()
		{
			if (_camera == null)
			{
				Debug.LogWarning("[AVProVideo] No camera set for UpdateMultiPassStereo component. If you are rendering in multi-pass stereo then it is recommended to set this.");
			}
		}

		private static bool IsMultiPassVrEnabled()
		{
			#if UNITY_TVOS
				return false;
			#else
			#if UNITY_2017_2_OR_NEWER
			if (!UnityEngine.XR.XRSettings.enabled) return false;
			#endif

			#if UNITY_2018_3_OR_NEWER
			if (UnityEngine.XR.XRSettings.stereoRenderingMode != UnityEngine.XR.XRSettings.StereoRenderingMode.MultiPass) return false;
			#endif

			return true;
			#endif
		}

		// We do a LateUpdate() to allow for any changes in the camera position that may have happened in Update()
		private void LateUpdate()
		{
			if (!IsMultiPassVrEnabled())
			{
				return;
			}

			if (_camera != null && _foundCamera != _camera)
			{
				_foundCamera = _camera;
			}
			if (_foundCamera == null)
			{
				_foundCamera = Camera.main;
				if (_foundCamera == null)
				{
					Debug.LogWarning("[AVProVideo] Cannot find main camera for UpdateMultiPassStereo, this can lead to eyes flickering");
					if (Camera.allCameras.Length > 0)
					{
						_foundCamera = Camera.allCameras[0];
						Debug.LogWarning("[AVProVideo] UpdateMultiPassStereo using camera " + _foundCamera.name);
					}
				}
			}

			if (_foundCamera != null)
			{
				Shader.SetGlobalVector(PropCameraPosition.Id, _foundCamera.transform.position);
				Shader.SetGlobalMatrix(PropViewMatrix.Id, _foundCamera.worldToCameraMatrix.transpose);
			}
		}
	}
}