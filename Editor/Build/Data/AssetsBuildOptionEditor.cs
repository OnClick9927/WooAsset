﻿using UnityEngine;
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
                    GUI.enabled = false;
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField(nameof(ServerDirectory), ServerDirectory);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(ServerDirectory);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(ServerDirectory);
                        GUILayout.EndHorizontal();
                        GUI.enabled = false;
                    }

                    GUI.enabled = true;
                    if (option.GetAssetModeType() == typeof(NormalAssetMode))
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.enableServer)));
                        if (option.enableServer)
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.serverPort)));
                    }
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

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.pkgs)));
                    if (option.copyToStream)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.buildInAssets)));
                    }


                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.version)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.MaxCacheVersionCount)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.cleanHistory)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.copyToStream)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.buildMode)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.bundleNameType)));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.typeTreeOption)));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.compress)));

                    option.build.typeIndex = EditorGUILayout.Popup("Asset Build", option.build.typeIndex, option.build.shortTypes);
                    option.encrypt.typeIndex = EditorGUILayout.Popup("Encrypt", option.encrypt.typeIndex, option.encrypt.shortTypes);


                    GUI.enabled = false;
                    EditorGUILayout.EnumPopup("Build Target", AssetsEditorTool.buildTarget);
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField("Editor Simulator Path", AssetsEditorTool.EditorSimulatorPath);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(EditorSimulatorPath);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(EditorSimulatorPath);
                        GUILayout.EndHorizontal();
                        GUI.enabled = false;
                    }
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField("Stream Bundle Directory", AssetsHelper.streamBundleDirectory);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(AssetsHelper.streamBundleDirectory);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(AssetsHelper.streamBundleDirectory);
                        GUILayout.EndHorizontal();
                        GUI.enabled = false;
                    }

                    GUILayout.Space(20);

                    {
                        EditorGUILayout.HelpBox("don't modify the file", MessageType.Warning);
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField("Output Path", AssetsEditorTool.outputPath);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(outputPath);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(outputPath);
                        GUILayout.EndHorizontal();
                        GUI.enabled = false;
                    }

                    {
                        EditorGUILayout.HelpBox("The first time you need to delete a folder,\n don't modify the file manually after that", MessageType.Warning);
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField("History Path", AssetsEditorTool.historyPath);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(historyPath);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(historyPath);
                        GUILayout.EndHorizontal();
                        GUI.enabled = false;
                    }



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