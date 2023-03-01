using UnityEditor;
using UnityEngine;


namespace WooAsset
{
    partial class AssetsWindow
    {
        [MenuItem("WooAsset/Open")]
        static void Open()
        {
            GetWindow<AssetsWindow>();
        }
    }
    partial class AssetsWindow : EditorWindow
    {
        private WindowLeft left = new WindowLeft();
        private WindowRight right = new WindowRight();
        private SplitView sp = new SplitView() { split = 300, minSize = 300 };
        private static AssetsToolSetting toolSetting { get { return AssetsToolSetting.Load<AssetsToolSetting>(); } }
        private static AssetsBuildSetting buildSetting { get { return AssetsBuildSetting.Load<AssetsBuildSetting>(); } }
        private static AssetsEditorCache cache { get { return AssetsEditorCache.Load<AssetsEditorCache>(); } }

        private void JustCollectAssets()
        {
            AssetsBuild.CollectInBuildAssets();
            FreshPreview();
        }
        private void PreView(bool md5)
        {
            AssetsBuild.FreshPreViewBundles(md5);
            FreshPreview();
        }
        private void CollectShaderVariant()
        {
            AssetsBuild.ShaderVariantCollector.Run(() => { PreView(false); });
        }
        private void FreshPreview()
        {
            right.ReLoad();

        }
        private void ClearPreview()
        {
            AssetsBuild.ClearCache();
            FreshPreview();

        }
        private void OnDisable()
        {
            right.OnDisable();
        }
        private void OnEnable()
        {
            sp.fistPan += left.OnGUI;
            sp.secondPan += right.OnGUI;
            left.OnEnable();
            right.OnEnable();

        }
        private void OnGUI()
        {
            var rs = this.LocalPosition().HorizontalSplit(20);
            Tool(rs[0]);
            sp.OnGUI(rs[1]);
        }

        private void Tool(Rect position)
        {
            GUILayout.BeginArea(position);
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button(new GUIContent("Tools"),EditorStyles.toolbarDropDown,GUILayout.Width(100)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Help/Build Atlas"), false, () => { AssetsBuild.AtlasBuild.Run(); });
                    menu.AddItem(new GUIContent("Help/Collect Shader Variant"), false, CollectShaderVariant);

                    menu.AddItem(new GUIContent("Preview/Just Collect Assets"), false, () => { JustCollectAssets(); });
                    menu.AddItem(new GUIContent("Preview/Bundle"), false, () => { PreView(false); });
                    menu.AddItem(new GUIContent("Preview/MD5 Bundle"), false, () => { PreView(true); });
                    menu.AddItem(new GUIContent("Preview/Clear"), false, () => { ClearPreview(); });
                    menu.AddItem(new GUIContent("Preview/Fresh"), false, () => { FreshPreview(); });




                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Bundle/Build"), false, () => { AssetsBuild.Build(); });
                    menu.AddItem(new GUIContent("Bundle/Copy To Steam"), false, AssetsBuild.CopyToStreamPath);

                    menu.AddItem(new GUIContent("Output/Open Folder"), false, AssetsBuild.OpenOutputFolder);
                    menu.AddItem(new GUIContent("Output/Clear Folder"), false, AssetsBuild.ClearOutputFolder);
                    menu.DropDown(GUILayoutUtility.GetLastRect());
                }
                GUILayout.FlexibleSpace();
                right.treeType = (WindowRight.TreeType)GUILayout.Toolbar((int)right.treeType, System.Enum.GetNames(typeof(WindowRight.TreeType)),EditorStyles.toolbarButton,GUILayout.Width(300));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }
    }
}
