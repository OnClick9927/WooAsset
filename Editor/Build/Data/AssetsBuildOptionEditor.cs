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
            static string key = "###AssetsBuildOptionEditor";
            enum Tab
            {
                Tool, Build,
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
                    var rect = EditorGUILayout.GetControlRect(GUILayout.Height(0));
                    EditorGUI.LabelField(rect, "", (GUIStyle)"in title");
                }
                protected void MidGUI(string title)
                {
                    GUILayout.Space(5);
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label(title, EditorStyles.whiteLargeLabel);
                    var rect = EditorGUILayout.GetControlRect(GUILayout.Height(0));
                    EditorGUI.LabelField(rect, "", (GUIStyle)"in title");
                }
                protected void EndGUI()
                {
                    GUILayout.Space(5);
                    GUILayout.EndVertical();
                    change = EditorGUI.EndChangeCheck();
                }
            }

            private class ToolTab : OptionTab
            {
                public override void OnGUI(SerializedObject serializedObject)
                {
                    BeginGUI("Asset Mode");
                    option.mode.typeIndex = EditorGUILayout.Popup("Mode", option.mode.typeIndex, option.mode.shortTypes);

                    MidGUI("Simulator");
                    {
                        GUILayout.BeginHorizontal();
                        GUI.enabled = false;

                        EditorGUILayout.TextField(nameof(EditorSimulatorPath), EditorSimulatorPath);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(EditorSimulatorPath);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(EditorSimulatorPath);
                        GUILayout.EndHorizontal();
                    }
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
                    if (option.GetAssetModeType() == typeof(NormalAssetsMode))
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.enableServer)));
                        if (option.enableServer)
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.serverPort)));
                    }



                    MidGUI("Shader Variant");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.shaderVariantOutputDirectory)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.shaderVariantInputDirectory)));
                    MidGUI("Sprite Atlas");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.packSetting)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.textureSetting)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.PlatformSetting)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.atlasPaths)));


                    MidGUI("Asset Tags");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.tags)));
                    MidGUI("Record Ignore");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.recordIgnore)));
                    EndGUI();


                }
            }

            private class BuildTab : OptionTab
            {
                public override void OnGUI(SerializedObject serializedObject)
                {
                    BeginGUI("Build");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.pkgs)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.version)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.MaxCacheVersionCount)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.cleanHistory)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.buildMode)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.bundleNameCalculate)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.bundleNameType)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.typeTreeOption)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.compress)));
                    GUILayout.Space(10);

                    option.buildPipeline.typeIndex = EditorGUILayout.Popup("Build Pipeline", option.buildPipeline.typeIndex, option.buildPipeline.shortTypes);
                    option.build.typeIndex = EditorGUILayout.Popup("Asset Build", option.build.typeIndex, option.build.shortTypes);
                    option.encrypt.typeIndex = EditorGUILayout.Popup("Encrypt", option.encrypt.typeIndex, option.encrypt.shortTypes);



                    MidGUI("Bundle Result Optimize");

                    option.bundleOptimizer.typeIndex = EditorGUILayout.Popup("Optimizer", option.bundleOptimizer.typeIndex, option.bundleOptimizer.shortTypes);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.optimizationCount)));
                    MidGUI("Built-in Asset Select");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.copyToStream)));
                    GUI.enabled = option.copyToStream;
                    option.buildInBundleSelector.typeIndex = EditorGUILayout.Popup("Selector", option.buildInBundleSelector.typeIndex, option.buildInBundleSelector.shortTypes);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.buildInAssets)));

                    MidGUI("Default");
                    GUI.enabled = false;
                    EditorGUILayout.EnumPopup(nameof(AssetsEditorTool.BuildTarget), AssetsEditorTool.BuildTarget);
                    EditorGUILayout.TextField(nameof(AssetsEditorTool.BuildTargetName), AssetsEditorTool.BuildTargetName);

                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField(nameof(AssetsEditorTool.StreamBundlePath), AssetsEditorTool.StreamBundlePath);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(AssetsEditorTool.StreamBundlePath);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(AssetsEditorTool.StreamBundlePath);
                        GUILayout.EndHorizontal();
                        GUI.enabled = false;
                    }

                    GUILayout.Space(20);

                    {
                        EditorGUILayout.HelpBox("don't modify the file", MessageType.Warning);
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField("Output Path", AssetsEditorTool.OutputPath);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(OutputPath);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(OutputPath);
                        GUILayout.EndHorizontal();
                        GUI.enabled = false;
                    }

                    {
                        EditorGUILayout.HelpBox("The first time you need to delete a folder,\n don't modify the file manually after that", MessageType.Warning);
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField(nameof(AssetsEditorTool.HistoryPath), AssetsEditorTool.HistoryPath);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(HistoryPath);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(HistoryPath);
                        GUILayout.EndHorizontal();
                        GUI.enabled = false;
                    }



                    GUI.enabled = true;
                    EndGUI();
                }
            }


            private ToolTab toolTab = new ToolTab();
            private BuildTab buildTab = new BuildTab();

            private OptionTab GetTab()
            {
                switch (tab)
                {
                    case Tab.Tool:
                        return toolTab;
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
