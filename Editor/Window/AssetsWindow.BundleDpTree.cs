using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Drawing;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class BundleDpTree : BundleTreeBase
        {
            long len;
            public void SetBundleGroup(EditorBundleData group)
            {
                len = 0;
                if (group != null)
                {
                    len = group.length;
                    var gs = group.GetDependence(cache.previewBundles);
                    foreach (var item in gs)
                    {
                        len += item.length;
                    }
                    base.SetBundleBuilds(gs);

                }
                else
                    base.SetBundleBuilds(null);
            }

            public BundleDpTree(TreeViewState state, IPing<EditorBundleData> ping) : base(state, ping)
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
                    TreeColumns.dependence.headerContent = new UnityEngine.GUIContent($"Dependence \ttotal:{GetSizeString(len)}");

                }
            }


            protected override MultiColumnHeaderState.Column GetFirstColomn() => TreeColumns.dependence;
        }
    }
}
