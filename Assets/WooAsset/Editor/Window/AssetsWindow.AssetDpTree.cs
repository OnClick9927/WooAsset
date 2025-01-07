using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace WooAsset
{

    partial class AssetsWindow
    {
        private class AssetDpTree : AssetTreeBase
        {
            private EditorAssetData asset;

            public void SetAssetInfo(EditorAssetData info)
            {
                this.asset = info;
                this.Reload();
                this.multiColumnHeader.ResizeToFit();
            }
            public AssetDpTree(TreeViewState state, IPing<EditorAssetData> ping) : base(state, ping) { }

            protected override MultiColumnHeaderState.Column GetFirstColumn() => TreeColumns.dependence;
            private void Build(TreeViewItem root, List<string> assets, IList<TreeViewItem> result)
            {
                long len = asset.length;
                foreach (var item in assets)
                {
                    len += tree.GetAssetData(item).length;
                    CreateItem(item, root, result, 1);
                }
                TreeColumns.dependence.headerContent = new UnityEngine.GUIContent($"Dependence \ttotal:{GetSizeString(len)}");
            }

            protected override void CreateRows(TreeViewItem root, IList<TreeViewItem> result)
            {
                if (this.asset != null && this.asset.dependence.Count > 0)
                {
                    Build(root, asset.dependence, result);
                }
            }




        }
    }
}
