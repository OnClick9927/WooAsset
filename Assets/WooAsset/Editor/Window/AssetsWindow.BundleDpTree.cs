using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class BundleDpTree : BundleTreeBase
        {
            public void SetBundleGroup(BundleGroup group)
            {
                if (group != null)
                    base.SetBundleGroups(group.dependence.ConvertAll(x => cache.GetBundleGroupByBundleName(x)));
                else
                    base.SetBundleGroups(null);
            }

            public BundleDpTree(TreeViewState state, IPing<BundleGroup> ping) : base(state, ping)
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


            protected override MultiColumnHeaderState.Column GetFirstColomn() => TreeColumns.dependence;
        }
    }
}
