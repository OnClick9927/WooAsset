using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class BundleUsageTree : BundleTreeBase
        {
            public void SetBundleGroup(BundleGroup group)
            {
                if (group != null)
                    base.SetBundleGroups(group.usage.ConvertAll(x => cache.GetBundleGroupByBundleName(x)));
                else
                    base.SetBundleGroups(null);
                this.Reload();
            }

            public BundleUsageTree(TreeViewState state, IPing<BundleGroup> ping) : base(state, ping)
            {

            }
            protected override void CreateRows(TreeViewItem root, IList<TreeViewItem> result)
            {

                if (this.groups != null && this.groups.Count > 0)
                {
                    for (int i = 0; i < groups.Count; i++)
                    {
                        BuildBundle(i, root, result);
                    }
                }
            }

            protected override MultiColumnHeaderState.Column GetFirstColomn() => TreeColumns.usage;
        }
    }
}
