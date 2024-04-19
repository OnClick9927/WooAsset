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
            static string key = "##AssetsBuildOptionEditor";
            enum Tab
            {
                Runtime, Tool, AssetTag, Build
            }
            private Tab tab;
            private void OnEnable() => tab = (Tab)EditorPrefs.GetInt(key, 0);
            private void OnDisable() => EditorPrefs.SetInt(key, (int)tab);
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
                tab = (Tab)GUILayout.Toolbar((int)tab, Enum.GetNames(typeof(Tab)));
                EditorGUI.BeginChangeCheck();
                switch (tab)
                {
                    case Tab.Runtime:
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
                        break;
                    case Tab.Tool:
                        V("Shader Variant",
                          () =>
                          {
                              EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.shaderVariantDirectory)));
                          });
                        V("Sprite Atlas",
                           () =>
                           {
                               EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.packSetting)));
                               EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.textureSetting)));
                               EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.PlatformSetting)));
                               EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.atlasPaths)));

                           });
                        break;
                    case Tab.AssetTag:
                        V("Asset Tags", () =>
                        {
                            EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.tags)));
                        });
                        break;
                    case Tab.Build:
                        V("Build",
                          () =>
               {


                   EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.buildPkgs)));
                   EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.buildInAssets)));


                   EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.version)));
                   EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.MaxCacheVersionCount)));
                   EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.forceRebuild)));
                   EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.cleanHistory)));
                   EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.AppendHashToAssetBundleName)));
                   EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.typeTreeOption)));

                   EditorGUILayout.PropertyField(this.serializedObject.FindProperty(nameof(AssetsBuildOption.compress)));

                   option.build.typeIndex = EditorGUILayout.Popup("Asset Build", option.build.typeIndex, option.build.shortTypes);
                   option.encrypt.typeIndex = EditorGUILayout.Popup("Encrypt", option.encrypt.typeIndex, option.encrypt.shortTypes);


                   GUI.enabled = false;
                   EditorGUILayout.EnumPopup("Build Target", AssetsEditorTool.buildTarget);
                   EditorGUILayout.TextField("Output Path", AssetsEditorTool.outputPath);
                   EditorGUILayout.TextField("Stream Bundle Directory", AssetsHelper.streamBundleDirectory);
                   GUILayout.Space(20);
                   EditorGUILayout.HelpBox("The first time you need to delete a folder,\n don't modify the file manually after that", MessageType.Warning);
                   EditorGUILayout.TextField("History Path", AssetsEditorTool.historyPath);

                   GUI.enabled = true;
               });
                        break;
                    default:
                        break;
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
