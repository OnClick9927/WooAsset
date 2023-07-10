using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;

namespace WooAsset
{

    partial class AssetsWindow
    {
        private class AssetUsageTree : TreeView
        {
            private EditorAssetData asset;
            public void SetAssetInfo(EditorAssetData info)
            {
                this.asset = info;
                this.Reload();
                this.multiColumnHeader.ResizeToFit();

            }
            public AssetUsageTree(TreeViewState state) : base(state)
            {
                showAlternatingRowBackgrounds = true;
                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    TreeColumns.usage,
                    TreeColumns.size,
                    TreeColumns.hash,
                    TreeColumns.bundle,
                    TreeColumns.bundleSize,
                    TreeColumns.tag,
                }));

                this.multiColumnHeader.ResizeToFit();
                Reload();
            }

            private void Build(TreeViewItem root, List<string> assets, IList<TreeViewItem> result)
            {
                foreach (var item in assets)
                {
                    CreateItem(item, root, result);
                }
            }
            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var result = GetRows() ?? new List<TreeViewItem>();
                result.Clear();
                if (asset != null)
                {
                    var usage = cache.tree.GetUsage(asset);

                    if (usage != null && usage.Count > 0)
                    {
                        Build(root, usage.ConvertAll(x => x.path), result);
                    }

                    SetupParentsAndChildrenFromDepths(root, result);
                }

                return result;
            }

            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem() { id = -10, depth = -1 };
            }
            private static TreeViewItem CreateItem(string path, TreeViewItem parent, IList<TreeViewItem> result)
            {
                Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var _item = new TreeViewItem()
                {
                    id = o.GetInstanceID(),
                    depth = 1,
                    displayName = path,
                };
                _item.parent = parent;
                parent.AddChild(_item);
                result.Add(_item);
                return _item;
            }
            private BundleGroup GetBundleGroupByAssetPath(string assetPath)
            {
                return cache.previewBundles.Find(x => x.ContainsAsset(assetPath));
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                string path = args.label;
                float indent = this.GetContentIndent(args.item);

                BundleGroup group = GetBundleGroupByAssetPath(path);
                EditorAssetData asset = cache.tree.GetAssetData(path);

                GUI.Label(RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0)), new GUIContent(path, Textures.GetMiniThumbnail(path)));
                GUI.Label(args.GetCellRect(1), GetSizeString(asset.length));
                GUI.Label(args.GetCellRect(2), asset.hash);
                GUI.Label(args.GetCellRect(5), GetTagsString(cache.tags.GetAssetTags(path)));
                if (group != null)
                {
                    EditorGUI.SelectableLabel(args.GetCellRect(3), group.hash);
                    GUI.Label(args.GetCellRect(4), GetSizeString(group.length));
                }
            }


        }
    }
}
