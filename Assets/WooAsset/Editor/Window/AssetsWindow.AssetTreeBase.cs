using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;

namespace WooAsset
{

    partial class AssetsWindow
    {
        private abstract class AssetTreeBase : TreeView
        {
            protected IPing<EditorAssetData> ping;
            public AssetTreeBase(TreeViewState state, IPing<EditorAssetData> ping) : base(state)
            {
                showAlternatingRowBackgrounds = true;
                this.ping = ping;

                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    GetFirtColumn(),
                    TreeColumns.loopDepenence,
                    TreeColumns.record,

                    TreeColumns.usageCount,
                    TreeColumns.depenceCount,
                    TreeColumns.type,
                    TreeColumns.size,
                    TreeColumns.tag,
                }));

                this.multiColumnHeader.ResizeToFit();
                Reload();
            }
            protected abstract MultiColumnHeaderState.Column GetFirtColumn();
            protected abstract void CreateRows(TreeViewItem root, IList<TreeViewItem> result);
            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var result = GetRows() ?? new List<TreeViewItem>();
                result.Clear();
                CreateRows(root, result);
                if (result.Count > 0)
                    SetupParentsAndChildrenFromDepths(root, result);
                return result;
            }

            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem() { id = -10, depth = -1 };
            }
            protected static TreeViewItem CreateItem(string path, TreeViewItem parent, IList<TreeViewItem> result, int depth)
            {
                Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var _item = new TreeViewItem()
                {
                    id = o.GetInstanceID(),
                    depth = depth,
                    displayName = path,
                };
                _item.parent = parent;
                parent.AddChild(_item);
                result.Add(_item);
                return _item;
            }


            protected override void RowGUI(RowGUIArgs args)
            {
                string path = args.label;
                float indent = this.GetContentIndent(args.item);

                EditorAssetData asset = cache.tree.GetAssetData(path);

                var rect1 = RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0));
                var rs = RectEx.VerticalSplit(rect1, 18);
                GUI.Label(rs[0], Textures.GetMiniThumbnail(path));
                EditorGUI.SelectableLabel(rs[1], path);

                if (asset.loopDependence)
                    GUI.Label(args.GetCellRect(1), Textures.err);
                EditorGUI.Toggle(args.GetCellRect(2), asset.record);
                DrawCount(args.GetCellRect(3), asset.usageCount);
                DrawCount(args.GetCellRect(4), asset.dependence.Count);
                GUI.Label(args.GetCellRect(5), asset.type.ToString());
                GUI.Label(args.GetCellRect(6), GetSizeString(asset.length));
                EditorGUI.SelectableLabel(args.GetCellRect(7), GetTagsString(asset));

            }

            protected override void DoubleClickedItem(int id)
            {
                string path = this.FindItem(id, rootItem).displayName;
                EditorAssetData asset = cache.tree.GetAssetData(path);
                this.ping.Ping(asset);
            }
        }
    }
}
