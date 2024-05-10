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
         
            private abstract class OptionTab
            {
                public bool change { get; private set; }
                public abstract void OnGUI(SerializedObject serializedObject);
                protected void BeginGUI(string title)
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label(title, EditorStyles.whiteLargeLabel);
                    GUILayout.Label("", GUILayout.Height(0));
                    var rect = GUILayoutUtility.GetLastRect();
                    EditorGUI.LabelField(rect, "", (GUIStyle)"in title");
                }
                protected void MidGUI(string title)
                {
                    GUILayout.Space(5);
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label(title, EditorStyles.whiteLargeLabel);
                    GUILayout.Label("", GUILayout.Height(0));
                    var rect = GUILayoutUtility.GetLastRect();
                    EditorGUI.LabelField(rect, "", (GUIStyle)"in title");
                }
                protected void EndGUI()
                {
                    GUILayout.Space(5);
                    GUILayout.EndVertical();
                    change = EditorGUI.EndChangeCheck();
                }
            }

            private class RuntimeTab : OptionTab
            {
                public override void OnGUI(SerializedObject serializedObject)
                {
                    BeginGUI("Asset Mode");
                    option.mode.typeIndex = EditorGUILayout.Popup("Mode", option.mode.typeIndex, option.mode.shortTypes);
                    MidGUI("Simulated Asset Server");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("enableServer"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("serverDirectory"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("serverPort"));
                    EndGUI();


                }
            }
            private class ToolTab : OptionTab
            {
                public override void OnGUI(SerializedObject serializedObject)
                {
                    BeginGUI("Shader Variant");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.shaderVariantDirectory)));
                    MidGUI("Sprite Atlas");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.packSetting)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.textureSetting)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.PlatformSetting)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.atlasPaths)));
                    EndGUI();
                }
            }
            private class AssetTagTab : OptionTab
            {
                public override void OnGUI(SerializedObject serializedObject)
                {
                    BeginGUI("Asset Tags");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.tags)));
                    EndGUI();

                }
            }
            private class BuildTab : OptionTab
            {
                public override void OnGUI(SerializedObject serializedObject)
                {
                    BeginGUI("Build");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.buildPkgs)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.buildInAssets)));


                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.version)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.MaxCacheVersionCount)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.forceRebuild)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.cleanHistory)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.AppendHashToAssetBundleName)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.typeTreeOption)));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.compress)));

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
                    EndGUI();
                }
            }


            private RuntimeTab runtimeTab = new RuntimeTab();
            private ToolTab toolTab = new ToolTab();
            private AssetTagTab assetTagTab = new AssetTagTab();
            private BuildTab buildTab = new BuildTab();

            private OptionTab GetTab()
            {
                switch (tab)
                {
                    case Tab.Runtime:
                        return runtimeTab;
                    case Tab.Tool:
                        return toolTab;
                    case Tab.AssetTag:
                        return assetTagTab;
                    case Tab.Build:
                        return buildTab;
                    default:
                        throw new Exception();
                }

            }



            private void OnEnable() => tab = (Tab)EditorPrefs.GetInt(key, 0);
            private void OnDisable() => EditorPrefs.SetInt(key, (int)tab);
            public override void OnInspectorGUI()
            {
                tab = (Tab)GUILayout.Toolbar((int)tab, Enum.GetNames(typeof(Tab)));
                this.serializedObject.Update();
                OptionTab _tab = GetTab();
                _tab.OnGUI(this.serializedObject);

                if (_tab.change)
                {
                    this.serializedObject.ApplyModifiedProperties();
                    option.Save();
                }

               
            }
        }


    }

}
