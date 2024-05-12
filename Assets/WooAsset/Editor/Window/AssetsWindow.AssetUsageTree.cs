using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace WooAsset
{

    partial class AssetsWindow
    {

        private class AssetUsageTree : AssetTreeBase
        {
            private EditorAssetData asset;

            public void SetAssetInfo(EditorAssetData info)
            {
                this.asset = info;
                this.Reload();
                this.multiColumnHeader.ResizeToFit();

            }
            public AssetUsageTree(TreeViewState state, IPing<EditorAssetData> ping) : base(state, ping) { }
            protected override MultiColumnHeaderState.Column GetFirtColumn() => TreeColumns.usage;
            private void Build(TreeViewItem root, List<string> assets, IList<TreeViewItem> result)
            {
                foreach (var item in assets)
                {
                    CreateItem(item, root, result, 1);
                }
            }
            protected override void CreateRows(TreeViewItem root, IList<TreeViewItem> result)
            {
                if (asset != null && asset.usage.Count > 0)
                {
                    Build(root, asset.usage, result);

                }
            }
        }
    }
}
