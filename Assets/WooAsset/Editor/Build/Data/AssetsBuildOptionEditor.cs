using UnityEngine;
using UnityEditor;
using System;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        [CustomEditor(typeof(AssetsBuildOption))]
        class AssetsBuildOptionEditor : Editor
        {
            enum Mode
            {
                Runtime, Editor
            }
            private Mode mode;
            static void V(string title, Action action)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(title, EditorStyles.whiteLargeLabel);
                GUILayout.Label("", GUILayout.Height(0));
                var rect = GUILayoutUtility.GetLastRect();
                EditorGUI.LabelField(rect, "", new GUIStyle("in title"));
                action?.Invoke();
                GUILayout.Space(5);
                GUILayout.EndVertical();
            }
            public override void OnInspectorGUI()
            {
                mode = (Mode)GUILayout.Toolbar((int)mode, Enum.GetNames(typeof(Mode)));
                EditorGUI.BeginChangeCheck();
                if (mode == Mode.Runtime)
                {
                    V("Asset Mode",
                    () =>
                    {
                        option.mode.typeIndex = EditorGUILayout.Popup("Mode", option.mode.typeIndex, option.mode.shortTypes);
                    });
                    V("Simulated Asset Server", () =>
                    {
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("enableServer"));
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("serverDirectory"));
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("serverPort"));

                    });
                }
                else
                {
                    V("Shader Variant",
                    () =>
                    {
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("shaderVariantDirectory"));
                    });
                    V("Sprite Atlas",
                       () =>
                       {
                           EditorGUILayout.PropertyField(this.serializedObject.FindProperty("packSetting"));
                           EditorGUILayout.PropertyField(this.serializedObject.FindProperty("textureSetting"));
                           EditorGUILayout.PropertyField(this.serializedObject.FindProperty("PlatformSetting"));
                           EditorGUILayout.PropertyField(this.serializedObject.FindProperty("atlasPaths"));

                       });
                    V("Build",
                    () =>
                    {
                        

                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("buildGroups"));
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("version"));
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("forceRebuild"));
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("ignoreTypeTreeChanges"));
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("MaxCacheVersionCount"));
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("compress"));

                        option.build.typeIndex = EditorGUILayout.Popup("Asset Build", option.build.typeIndex, option.build.shortTypes);
                        option.encrypt.typeIndex = EditorGUILayout.Popup("Encrypt", option.encrypt.typeIndex, option.encrypt.shortTypes);
  

                        GUI.enabled = false;
                        EditorGUILayout.TextField("Output Path", AssetsEditorTool.outputPath);
                        EditorGUILayout.EnumPopup("Build Target", AssetsEditorTool.buildTarget);

                        GUILayout.Space(20);
                        EditorGUILayout.HelpBox("The first time you need to delete a folder,\n don't modify the file manually after that", MessageType.Warning);
                        EditorGUILayout.TextField("History Path", AssetsEditorTool.historyPath);

                        GUI.enabled = true;
                    });
                }



                if (EditorGUI.EndChangeCheck())
                {
                    this.serializedObject.ApplyModifiedProperties();
                    option.Save();
                }
            }
        }


    }

}
