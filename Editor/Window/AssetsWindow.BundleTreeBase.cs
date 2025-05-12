using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static WooAsset.AssetsEditorTool;
using System.Linq;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private abstract class BundleTreeBase : TreeView
        {

            protected virtual List<EditorBundleData> groups { get; private set; }
            protected IPing<EditorBundleData> ping;
            protected void SetBundleBuilds(List<EditorBundleData> groups)
            {
                this.groups = groups;
                this.Reload();
            }

            public BundleTreeBase(TreeViewState state, IPing<EditorBundleData> ping) : base(state)
            {
                var _base = new MultiColumnHeaderState.Column[]
                {
                    GetFirstColomn(),
                   TreeColumns.usageCount,
                    TreeColumns.dependenceCount,
                    TreeColumns.size,
                    TreeColumns.loopDependence,
                    TreeColumns.Raw,

                };

                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(_base));
                this.multiColumnHeader.ResizeToFit();
                this.ping = ping;
                showAlternatingRowBackgrounds = true;
                Reload();
            }
            protected abstract MultiColumnHeaderState.Column GetFirstColomn();
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
            protected TreeViewItem BuildBundle(int i, TreeViewItem root, IList<TreeViewItem> result)
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
            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem() { id = -10, depth = -1 };

            }
            protected override void RowGUI(RowGUIArgs args)
            {
                float indent = this.GetContentIndent(args.item);
                var rect1 = RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0));
                var rs = RectEx.VerticalSplit(rect1, 18);
                GUI.Label(rs[0], Textures.folder);
                EditorGUI.LabelField(rs[1], args.label);


                EditorBundleData group = cache.GetBundleGroupByBundleName(args.label);
                DrawCount(args.GetCellRect(1), group.usageCount);
                DrawCount(args.GetCellRect(2),  group.dependenceCount);
                GUI.Label(args.GetCellRect(3), GetSizeString(group.length));
                if (group.loopDependence)
                    GUI.Label(args.GetCellRect(4), Textures.err);
                if (group.raw)
                    GUI.Toggle(args.GetCellRect(5), true, "");
            }
            protected override void DoubleClickedItem(int id)
            {
                var group = groups[id];
                ping.Ping(group);
            }
            protected virtual void CreateMenus(List<string> paths, GenericMenu menu)
            {

            }
            protected override void ContextClicked()
            {
                var selection = this.GetSelection();
                var rows = this.FindRows(selection).ToList().ConvertAll(x => x.displayName);
                List<string> paths = rows;
                GenericMenu menu = new GenericMenu();
                if (paths.Count == 1)
                {
                    menu.AddItem(new UnityEngine.GUIContent("CopyPath"), false, () =>
                    {
                        GUIUtility.systemCopyBuffer = paths[0];
                    });
                }
                CreateMenus(paths, menu);
                if (menu.GetItemCount() > 0)
                {

                    menu.ShowAsContext();
                }

                base.ContextClicked();
            }
        }
    }
}
