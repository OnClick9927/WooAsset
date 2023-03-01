using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace WooAsset
{


    partial class AssetsWindow
    {
        [System.Serializable]
        private class WindowRight
        {
            public enum TreeType
            {
                Assets,
                Bundles,
                AssetLife
            }
            public TreeType treeType = TreeType.Assets;
            [SerializeField] private TreeViewState collectState = new TreeViewState();
            [SerializeField] private TreeViewState previewState = new TreeViewState();
            [SerializeField] private TreeViewState rtState = new TreeViewState();

            [SerializeField] private CollectTree.SearchType colSearchType = CollectTree.SearchType.Name;
            [SerializeField] private PreviewTree.SearchType preSearchType = PreviewTree.SearchType.AssetByPath;
            [SerializeField] private RTTree.SearchType rtSearchType = RTTree.SearchType.Asset;

            private CollectTree col;
            private PreviewTree pre;
            private RTTree rt;

            public void OnEnable()
            {
                col = new CollectTree(collectState, colSearchType);
                pre = new PreviewTree(previewState, preSearchType);
                rt = new RTTree(rtState, rtSearchType);
                AssetsEditorTool.onAssetLifChange += rt.Reload;
            }
            public void OnDisable()
            {
                colSearchType = col._searchType;
                preSearchType = pre._searchType;
                rtSearchType = rt._searchType;
                AssetsEditorTool.onAssetLifChange -= rt.Reload;
            }
            public void OnGUI(Rect rect)
            {
                switch (treeType)
                {
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
            public void ReLoad()
            {
                col.Reload();
                pre.Reload();
            }
        }
    }
}
