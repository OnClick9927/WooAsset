using UnityEngine;
using UnityEditor;

namespace WooAsset
{
    partial class AssetsBuild
    {
        [CustomEditor(typeof(AssetsBuildSetting))]
        class AssetBuildSettingEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                SerializedObject obj = this.serializedObject;
                EditorGUI.BeginChangeCheck();
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(obj.FindProperty("tags"), new GUIContent("Tags"), true);
                setting.buildGroup.typeIndex = EditorGUILayout.Popup("Bundle Group", setting.buildGroup.typeIndex, setting.buildGroup.shortTypes);
                setting.encrypt.typeIndex = EditorGUILayout.Popup("Encrypt", setting.encrypt.typeIndex, setting.encrypt.shortTypes);
                GUI.enabled = false;
                EditorGUILayout.TextField("Output Path", AssetsBuild.outputPath);
                EditorGUILayout.EnumPopup("Build Target", EditorUserBuildSettings.activeBuildTarget);
                GUI.enabled = true;
                if (EditorGUI.EndChangeCheck())
                {
                    obj.ApplyModifiedProperties();
                    setting.Save();
                    AssetsBuild.RemoveUseLessTagAssets();
                }
            }
        }


    }

}
