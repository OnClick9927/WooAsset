using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEngine;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class BundleUsageTree : TreeView
        {
            private List<BundleGroup> groups = new List<BundleGroup>();
            private readonly IPing<BundleGroup> bundlesTree;

            public BundleUsageTree(TreeViewState state, IPing<BundleGroup> bundlesTree) : base(state)
            {
                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                TreeColumns.usage,
                   TreeColumns.usageCount,
                    TreeColumns.depenceCount,
                TreeColumns.size,

                }));

                this.multiColumnHeader.ResizeToFit();
                showAlternatingRowBackgrounds = true;

                Reload();
                this.bundlesTree = bundlesTree;
            }


            public void SetBundleGroup(BundleGroup group)
            {
                if (group != null)
                    this.groups = group.usage.ConvertAll(x => cache.GetBundleGroupByBundleName(x));
                else
                    this.groups = null;
                this.Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem() { id = -10, depth = -1 };

            }
            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var result = GetRows() ?? new List<TreeViewItem>();
                result.Clear();

                if (this.groups != null && this.groups.Count > 0)
                {
                    for (int i = 0; i < groups.Count; i++)
                    {
                        BuildBundle(i, root, result);
                    }
                }

                SetupParentsAndChildrenFromDepths(root, result);
                return result;
            }
            private TreeViewItem BuildBundle(int i, TreeViewItem root, IList<TreeViewItem> result)
            {
                var bundle = groups[i];
                var _item = new TreeViewItem()
                {
                    id = i,
                    depth = 0,
                    parent = root,
                    displayName = bundle.hash,


                };
                root.AddChild(_item);
                result.Add(_item);
                return _item;
            }

            protected override void RowGUI(RowGUIArgs args)
            {

                float indent = this.GetContentIndent(args.item);
                var first = RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0));
                GUI.Label(first, new GUIContent(args.label, Textures.folder));
                BundleGroup group = cache.GetBundleGroupByBundleName(args.label);

                GUI.Label(args.GetCellRect(1), group.usage.Count.ToString());
                GUI.Label(args.GetCellRect(2), groups.Count.ToString());
                GUI.Label(args.GetCellRect(3), GetSizeString(group.length));

            }
            protected override void DoubleClickedItem(int id)
            {
                var group = groups[id];
                bundlesTree.Ping(group);
            }
        }
    }
}
