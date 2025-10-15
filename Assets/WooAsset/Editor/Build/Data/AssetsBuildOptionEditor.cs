using System;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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
                    BeginGUI("Simulator");
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
                    {
                        GUI.enabled = false;
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField(nameof(ServerDirectory), ServerDirectory);
                        GUI.enabled = true;

                        if (GUILayout.Button("Open", GUILayout.Width(40)))
                            EditorUtility.OpenWithDefaultApp(ServerDirectory);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                            AssetsEditorTool.DeleteDirectory(ServerDirectory);
                        GUILayout.EndHorizontal();
                        //GUI.enabled = false;
                    }
                    GUILayout.Space(5);
                    option.mode.mode.typeIndex = EditorGUILayout.Popup("Mode", option.mode.mode.typeIndex, option.mode.mode.shortTypes);
                    GUILayout.Space(5);

                    if (option.GetAssetModeType() == typeof(RudeMode))
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.mode))
                            .FindPropertyRelative(nameof(AssetsBuildOption.mode.CheckAssetType)));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.mode))
                            .FindPropertyRelative(nameof(AssetsBuildOption.mode.Folders)));
                    }
                    if (option.GetAssetModeType() == typeof(NormalAssetsMode))
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.server))
                            .FindPropertyRelative(nameof(AssetsBuildOption.server.enable)));
                        if (option.server.enable)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.server))
                                .FindPropertyRelative(nameof(AssetsBuildOption.server.port)));
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.server))
                                .FindPropertyRelative(nameof(AssetsBuildOption.server.speed)),new GUIContent("Speed (s)"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.server))
                     .FindPropertyRelative(nameof(AssetsBuildOption.server.speedType)),new GUIContent(),GUILayout.Width(50));
                            GUILayout.EndHorizontal();
                        }
                    }
                    //MidGUI("Simulator");



                    //GUI.enabled = true;
           



                    MidGUI("Shader Variant");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.shader))
                                        .FindPropertyRelative(nameof(AssetsBuildOption.shader.InputDirectory)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.shader))
                        .FindPropertyRelative(nameof(AssetsBuildOption.shader.OutputDirectory)));
                    MidGUI("Sprite Atlas");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.spriteAtlas))
                        .FindPropertyRelative(nameof(AssetsBuildOption.spriteAtlas.atlasPaths)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.spriteAtlas))
                 .FindPropertyRelative(nameof(AssetsBuildOption.spriteAtlas.textureSetting)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.spriteAtlas))
                        .FindPropertyRelative(nameof(AssetsBuildOption.spriteAtlas.packSetting)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.spriteAtlas))
                        .FindPropertyRelative(nameof(AssetsBuildOption.spriteAtlas.PlatformSetting)));



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
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.ClearAssetCache)));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.version)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.MaxCacheVersionCount)));
                    //EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.cleanHistory)));
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

                    option.bundleOptimize.optimizer.typeIndex = EditorGUILayout.Popup("Optimizer", option.bundleOptimize.optimizer.typeIndex, option.bundleOptimize.optimizer.shortTypes);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.bundleOptimize)).FindPropertyRelative(nameof(AssetsBuildOption.bundleOptimize.count)));
                    MidGUI("Built-in Asset Select");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.buildIn))
                        .FindPropertyRelative(nameof(AssetsBuildOption.buildIn.copyToStream)));
                    GUI.enabled = option.buildIn.copyToStream;
                    option.buildIn.selector.typeIndex = EditorGUILayout.Popup("Selector", option.buildIn.selector.typeIndex, option.buildIn.selector.shortTypes);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AssetsBuildOption.buildIn))
                                   .FindPropertyRelative(nameof(AssetsBuildOption.buildIn.assets)));
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
            private Vector2 pos;
            public override void OnInspectorGUI()
            {
                tab = (Tab)GUILayout.Toolbar((int)tab, Enum.GetNames(typeof(Tab)));
                this.serializedObject.Update();
                OptionTab _tab = GetTab();
                pos = EditorGUILayout.BeginScrollView(pos);
                _tab.OnGUI(this.serializedObject);
                EditorGUILayout.EndScrollView();
                if (_tab.change)
                {
                    this.serializedObject.ApplyModifiedProperties();
                    option.Save();
                }


            }
        }


    }

}
