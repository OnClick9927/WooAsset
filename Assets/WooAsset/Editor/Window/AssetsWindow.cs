using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


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
            return $"{(length / Mathf.Pow(1024, stage)).ToString("0.00")} {stages[stage]}";
        }

        private static void DrawCount(Rect rect, int count)
        {
            GUI.enabled = count != 0;
            GUI.Label(rect, count == 0 ? "◌" : count.ToString());
            GUI.enabled = true;
        }
        private static List<string> stages = new List<string>()
        {
            "B","KB","MB","GB","TB"
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

        private AssetsTree col;
        private BundlesTree pre;
        private LifeTree rt;
        private Editor buildSetting_editor;

        public void OnEnable()
        {
            col = new AssetsTree(collectState, colSearchType);
            pre = new BundlesTree(previewState, preSearchType);
            rt = new LifeTree(rtState, rtSearchType);
            buildSetting_editor = Editor.CreateEditor(AssetsEditorTool.option);
            AssetsEditorTool.onAssetLifeChange += rt.Reload;
            AssetsEditorTool.onPipelineFinish += FreshPreview;

        }
        public void OnDisable()
        {
            AssetsEditorTool.onPipelineFinish -= FreshPreview;
            AssetsEditorTool.onAssetLifeChange -= rt.Reload;

            colSearchType = col._searchType;
            preSearchType = pre._searchType;
            rtSearchType = rt._searchType;
        }
        private void OnGUI()
        {
            var rs = RectEx.HorizontalSplit(new Rect(Vector2.zero, position.size), 30, 6, false);
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
                    col.OnGUI(rect);
                    break;
                case TreeType.Bundles:
                    pre.OnGUI(rect);
                    break;
                case TreeType.AssetLife:
                    rt.OnGUI(rect);
                    break;
                default:
                    break;
            }

        }


        public void FreshPreview()
        {
            col.Reload();
            pre.OnReload();
        }




        private void Tool(Rect position)
        {
            GUILayout.BeginArea(position);
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label("", GUILayout.Width(100));
                Rect rect = GUILayoutUtility.GetLastRect();
                if (GUI.Button(rect, GUIContent("Tools"), EditorStyles.toolbarDropDown))
                {
                    EditorUtility.DisplayPopupMenu(rect, TaskPipelineMenu.root, new MenuCommand(null));
                }
                GUILayout.FlexibleSpace();
                treeType = (TreeType)GUILayout.Toolbar((int)treeType, System.Enum.GetNames(typeof(TreeType)), EditorStyles.toolbarButton, GUILayout.Width(300));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }
    }
}
