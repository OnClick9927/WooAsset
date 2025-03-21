﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;


namespace WooAsset
{
    partial class AssetsWindow : EditorWindow
    {
        private static AssetsEditorCache cache { get { return AssetsEditorTool.cache; } }
        private static string GetSizeString(long length)
        {
            var tmp = length;

            int stage = 0;
            while (tmp > 1024)
            {
                tmp /= 1024;
                stage++;
            }
            return $" {(length / Mathf.Pow(1024, stage)).ToString("0.00")} {stages[stage]}";
        }

        private static void DrawCount(Rect rect, int count)
        {
            GUI.enabled = count != 0;
            GUI.Label(rect, count == 0 ? "◌" : count.ToString());
            GUI.enabled = true;
        }
        private static List<string> stages = new List<string>()
        {
            "B","K","M","G","T","P"
        };
        private static string GetTagsString(IReadOnlyList<string> sources)
        {
            if (sources == null || sources.Count == 0) return string.Empty;
            return string.Join(",", sources);
        }
        private static string GetTagsString(EditorAssetData data)
        {
            return GetTagsString(data.tags);
        }

        private static GUIContent c = new GUIContent();
        public static GUIContent GUIContent(string txt)
        {
            c.image = null;
            c.tooltip = null;
            c.text = txt;
            return c;
        }
        public static GUIContent GUIContent(string txt, Texture tx)
        {
            c.tooltip = null;
            c.image = tx;
            c.text = txt;
            return c;
        }

        private static void DrawPing(Rect rect)
        {
            GUI.Label(RectEx.Zoom(rect, TextAnchor.MiddleCenter, -8), "", "LightmapEditorSelectedHighlight");
        }
        public enum TreeType
        {
            Option,
            Assets,
            Bundles,
            AssetLife
        }
        [MenuItem(TaskPipelineMenu.root + "Window")]
        static void Open()
        {
            GetWindow<AssetsWindow>();
        }
        private TreeType treeType = TreeType.Option;
        private TreeViewState collectState = new TreeViewState();
        private TreeViewState previewState = new TreeViewState();
        private TreeViewState rtState = new TreeViewState();
        private AssetsTree.SearchType colSearchType = AssetsTree.SearchType.Name;
        private BundlesTree.SearchType preSearchType = BundlesTree.SearchType.AssetByPath;
        private LifeTree.SearchType rtSearchType = LifeTree.SearchType.Asset;

        private AssetsTree tree_win;
        private BundlesTree bundle_win;
        private LifeTree life_win;
        private Editor buildSetting_editor;

        public void OnEnable()
        {
            tree_win = new AssetsTree(collectState, colSearchType);
            bundle_win = new BundlesTree(previewState, preSearchType);
            life_win = new LifeTree(rtState, rtSearchType);
            buildSetting_editor = Editor.CreateEditor(AssetsEditorTool.option);
            AssetsEditorTool.LifePart.onAssetLifeChange += life_win.Reload;
            AssetsEditorTool.onPipelineFinish += FreshPreview;

        }
        public void OnDisable()
        {
            AssetsEditorTool.onPipelineFinish -= FreshPreview;
            AssetsEditorTool.LifePart.onAssetLifeChange -= life_win.Reload;

            colSearchType = tree_win._searchType;
            preSearchType = bundle_win._searchType;
            rtSearchType = life_win._searchType;
        }
        static float scale = 1.1f;
        private void OnGUI()
        {
            GUIUtility.ScaleAroundPivot(Vector2.one * scale, Vector2.zero);
            var rs = RectEx.HorizontalSplit(new Rect(Vector2.zero, position.size / scale), 30, 6, false);
            Tool(rs[0]);
            ContentGUI(rs[1]);
        }

        private Vector2 scroll;
        public void ContentGUI(Rect rect)
        {
            switch (treeType)
            {
                case TreeType.Option:
                    GUILayout.BeginArea(rect);
                    scroll = GUILayout.BeginScrollView(scroll);
                    buildSetting_editor.OnInspectorGUI();
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    break;
                case TreeType.Assets:
                    tree_win.OnGUI(rect);
                    break;
                case TreeType.Bundles:
                    bundle_win.OnGUI(rect);
                    break;
                case TreeType.AssetLife:
                    life_win.OnGUI(rect);
                    break;
                default:
                    break;
            }

        }


        public void FreshPreview()
        {
            tree_win.Reload();
            bundle_win.OnReload();
            life_win.Reload();
        }




        private void Tool(Rect position)
        {
            GUILayout.BeginArea(position);
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(100));
                if (GUI.Button(rect, GUIContent("Tools"), EditorStyles.toolbarDropDown))
                {
                    EditorUtility.DisplayPopupMenu(rect, TaskPipelineMenu.root, new MenuCommand(null));
                }


                GUILayout.FlexibleSpace();
                var tmp = (TreeType)GUILayout.Toolbar((int)treeType, System.Enum.GetNames(typeof(TreeType)), EditorStyles.toolbarButton, GUILayout.Width(300));
                if (tmp!= treeType)
                {
                    treeType = tmp;
                    FreshPreview();
                }
                
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }
    }
}
