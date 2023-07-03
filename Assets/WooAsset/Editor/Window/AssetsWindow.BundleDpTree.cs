using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEngine;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class BundleDpTree : TreeView
        {
            private List<BundleGroup> groups = new List<BundleGroup>();

            public BundleDpTree(TreeViewState state) : base(state)
            {
                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
TreeColumns.dependence,
TreeColumns.size,

                }));

                this.multiColumnHeader.ResizeToFit();
                Reload();
            }


            public void SetBundleGroup(List<BundleGroup> groups)
            {
                showAlternatingRowBackgrounds = true;
                this.groups = groups;
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
            public BundleGroup GetBundleGroupByBundleName(string bundleName)
            {
                return cache.previewBundles.Find(x => x.hash == bundleName);
            }
            protected override void RowGUI(RowGUIArgs args)
            {

                float indent = this.GetContentIndent(args.item);
                var first = RectEx.Zoom(args.GetCellRect(0),TextAnchor.MiddleRight, new Vector2(-indent, 0));
                GUI.Label(first, new GUIContent(args.label, Textures.folder));
                BundleGroup group = GetBundleGroupByBundleName(args.label);
                GUI.Label(args.GetCellRect(1), GetSizeString(group.length));

            }

        }
    }
}
