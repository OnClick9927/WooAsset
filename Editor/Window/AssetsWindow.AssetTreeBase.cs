﻿using UnityEditor;
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
            protected EditorAssetCollection tree
            {
                get
                {
                    if (this.ping is BundlesTree)
                        return cache.tree_bundle;

                    if (this.ping is AssetsTree)
                        return cache.tree_asset;

                    return null;
                }
            }
            protected IPing<EditorAssetData> ping;
            public AssetTreeBase(TreeViewState state, IPing<EditorAssetData> ping) : base(state)
            {
                showAlternatingRowBackgrounds = true;
                this.ping = ping;

                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    GetFirstColumn(),
                    TreeColumns.record,

                    TreeColumns.usageCount,
                    TreeColumns.dependenceCount,
                    TreeColumns.type,
                    TreeColumns.size,
                    TreeColumns.tag,
                }));
                //this.rowHeight = 20;
                this.multiColumnHeader.ResizeToFit();
                Reload();
            }
            protected abstract MultiColumnHeaderState.Column GetFirstColumn();
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

                TreeViewItem _item = null;
                _item = new TreeViewItem()
                {
                    id = o ? o.GetInstanceID() : -1,
                    depth = depth,
                    displayName = path,
                };
                if (o)
                {

                }
                _item.parent = parent;
                parent.AddChild(_item);
                result.Add(_item);
                return _item;
            }
     

            protected override void RowGUI(RowGUIArgs args)
            {
                string path = args.label;


                float indent = this.GetContentIndent(args.item);
                var rect1 = RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0));
                if (args.item.id != -1)
                {

                    EditorAssetData asset = tree.GetAssetData(path);
                    var rs = RectEx.VerticalSplit(rect1, 18);
                    GUI.Label(rs[0], Textures.GetMiniThumbnail(path));

                    EditorGUI.SelectableLabel(rs[1], path);


                    EditorGUI.Toggle(args.GetCellRect(1), asset.record);
                    DrawCount(args.GetCellRect(2), asset.usageCount);
                    DrawCount(args.GetCellRect(3), asset.dependence.Count);
                    GUI.Label(args.GetCellRect(4), asset.type.ToString());
                    GUI.Label(args.GetCellRect(5), GetSizeString(asset.length));
                    EditorGUI.SelectableLabel(args.GetCellRect(6), GetTagsString(asset));
                }
                else
                {
                    GUI.contentColor = new Color(1f, 0.2f, 0, 1);
                    GUI.Label(rect1, new GUIContent($"Not Found---> {path}", Textures.err));
                    GUI.contentColor = Color.white;
                }

            }

            protected override void DoubleClickedItem(int id)
            {
                string path = this.FindItem(id, rootItem).displayName;
                EditorAssetData asset = tree.GetAssetData(path);
                this.ping.Ping(asset);
            }
        }
    }
}
