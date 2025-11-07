using UnityEngine;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Demos
{
	/// <summary>
	/// Rotate the transform (usually with Camera attached) to look around during playback of 360/180 videos.
	/// Unity will rotate the camera automatically if VR is enabled, in which case this script does nothing.
	/// Otherwise if there is a gyroscope it will be used, otherwise the mouse/touch can be used.
	/// </summary>
	public class LookAround360 : MonoBehaviour
	{
		[SerializeField] bool _lockPitch = false;
		[SerializeField] float _maxSpinSpeed = 40f;
		[SerializeField, Range(1f, 10f)] float _spinDamping = 5f;

		private float _spinX;
		private float _spinY;

		private static bool IsVrPresent()
		{
			bool result = false;
		#if UNITY_2019_3_OR_NEWER
			var xrDisplaySubsystems = new List<UnityEngine.XR.XRDisplaySubsystem>();
			SubsystemManager.GetInstances<UnityEngine.XR.XRDisplaySubsystem>(xrDisplaySubsystems);
			foreach (var xrDisplay in xrDisplaySubsystems)
			{
				if (xrDisplay.running)
				{
					result = true;
					break;
				}
			}
		#else
		#if UNITY_2018_1_OR_NEWER
			result = (UnityEngine.XR.XRDevice.isPresent);
		#else
			result = (UnityEngine.VR.VRDevice.isPresent);
		#endif
		#endif
			return result;
		}

		void Start()
		{
			if (IsVrPresent())
			{
				this.enabled = false;
				return;
			}

			if (SystemInfo.supportsGyroscope)
			{
				Input.gyro.enabled = true;
			}
		}

		void Update()
		{
			if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
			{
				RotateFromGyro();
			}
			else
			{
				RotateFromMouseOrTouch();
			}
		}

		void OnDestroy()
		{
			if (SystemInfo.supportsGyroscope)
			{
				Input.gyro.enabled = false;
			}
		}

		void RotateFromGyro()
		{
			// Invert the z and w of the gyro attitude
			this.transform.localRotation = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);
		}

		void RotateFromMouseOrTouch()
		{
			if (Input.GetMouseButton(0))
			{
				float h = _maxSpinSpeed * -Input.GetAxis("Mouse X") * Time.deltaTime;
				float v = 0f;
				if (!_lockPitch)
				{
					v = _maxSpinSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
				}						
				h = Mathf.Clamp(h, -0.5f, 0.5f);
				v = Mathf.Clamp(v, -0.5f, 0.5f);
				_spinX += h;
				_spinY += v;
			}
			if (!Mathf.Approximately(_spinX, 0f) || !Mathf.Approximately(_spinY, 0f))
			{
				this.transform.Rotate(Vector3.up, _spinX);
				this.transform.Rotate(Vector3.right, _spinY);

				_spinX = Mathf.MoveTowards(_spinX, 0f, _spinDamping * Time.deltaTime);
				_spinY = Mathf.MoveTowards(_spinY, 0f, _spinDamping * Time.deltaTime);
			}
		}
	}
}