using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the AudioOutput component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(AudioOutput))]
	public class AudioOutputEditor : UnityEditor.Editor
	{
		private static readonly GUIContent _guiTextChannel = new GUIContent("Channel");
		private static readonly GUIContent _guiTextChannels = new GUIContent("Channels");
		private static readonly string[] _channelMaskOptions = { "1", "2", "3", "4", "5", "6", "7", "8" };

		private SerializedProperty _propChannelMask;
		private SerializedProperty _propAudioOutputMode;
		private int _unityAudioSampleRate;
		private int _unityAudioSpeakerCount;
		private string _bufferedMs;

		void OnEnable()
		{
			_propChannelMask = this.CheckFindProperty("_channelMask");
			_propAudioOutputMode = this.CheckFindProperty("_audioOutputMode");
			_unityAudioSampleRate = Helper.GetUnityAudioSampleRate();
			_unityAudioSpeakerCount = Helper.GetUnityAudioSpeakerCount();
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawDefaultInspector();

			// Display the channel mask as either a bitfield or value slider
			if ((AudioOutput.AudioOutputMode)_propAudioOutputMode.enumValueIndex == AudioOutput.AudioOutputMode.MultipleChannels)
			{
				_propChannelMask.intValue = EditorGUILayout.MaskField(_guiTextChannels, _propChannelMask.intValue, _channelMaskOptions);
			}
			else
			{
				int prevVal = 0;
				for(int i = 0; i < 8; ++i)
				{
					if((_propChannelMask.intValue & (1 << i)) > 0)
					{
						prevVal = i;
						break;
					}
				}
				
				int newVal = Mathf.Clamp(EditorGUILayout.IntSlider(_guiTextChannel, prevVal, 0, 7), 0, 7);
				_propChannelMask.intValue = 1 << newVal;
			}

			GUILayout.Label("Unity Audio", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Speakers", _unityAudioSpeakerCount.ToString());
			EditorGUILayout.LabelField("Sample Rate", _unityAudioSampleRate.ToString() + "hz");
			EditorGUILayout.Space();

			AudioOutput audioOutput = (AudioOutput)this.target;
			if (audioOutput != null)
			{
				if (audioOutput.Player != null && audioOutput.Player.Control != null)
				{
					int channelCount = audioOutput.Player.Control.GetAudioChannelCount();
					if (channelCount >= 0)
					{
						GUILayout.Label("Media Audio", EditorStyles.boldLabel);
						EditorGUILayout.LabelField("Channels: " + channelCount);
						AudioChannelMaskFlags audioChannels = audioOutput.Player.Control.GetAudioChannelMask();
						GUILayout.Label(audioChannels.ToString(), EditorHelper.IMGUI.GetWordWrappedTextAreaStyle());

						if (Time.frameCount % 4 == 0)
						{
							int bufferedSampleCount = audioOutput.Player.Control.GetAudioBufferedSampleCount();
							float bufferedMs = (bufferedSampleCount * 1000f) / (_unityAudioSampleRate * channelCount);
							_bufferedMs = "Buffered: " + bufferedMs.ToString("F2") + "ms";
						}

						EditorGUILayout.LabelField(_bufferedMs);
						EditorGUILayout.Space();
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}