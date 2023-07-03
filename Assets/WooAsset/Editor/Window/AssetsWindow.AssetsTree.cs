using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;
using System.IO;
using System.Linq;

namespace WooAsset
{
    partial class AssetsWindow
    {

        private class AssetsTree : TreeView
        {
            public enum SearchType
            {
                Name,
                Tag,
                Type
            }
            private SearchField search;
            public SearchType _searchType = SearchType.Name;
            private AssetDpTree assetDp;
            private SplitView sp = new SplitView() { vertical = false, minSize = 200, split = 300 };
            private enum DpViewType
            {
                None,
                Asset,
            }
            private DpViewType dpViewType = DpViewType.None;
            public AssetsTree(TreeViewState state, SearchType _searchType) : base(state)
            {
                assetDp = new AssetDpTree(new TreeViewState());
                this._searchType = _searchType;
                search = new SearchField(this.searchString, System.Enum.GetNames(typeof(SearchType)), (int)_searchType);
                search.onValueChange += (value) => { this.searchString = value.ToLower(); };
                search.onModeChange += (value) => { this._searchType = (SearchType)value; this.Reload(); };
                showAlternatingRowBackgrounds = true;
                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    TreeColumns.emptyTitle,
                    TreeColumns.type,
                    TreeColumns.size,
                    TreeColumns.hash,
                    TreeColumns.tag,

                }));
                this.multiColumnHeader.ResizeToFit();
                this.Reload();
                sp.fistPan += Sp_fistPan;
                sp.secondPan += Sp_secondPan;
            }

            private void Sp_secondPan(Rect obj)
            {
                switch (dpViewType)
                {
                    case DpViewType.None:
                        break;
                    case DpViewType.Asset:
                        assetDp.OnGUI(obj);
                        break;
                }
            }

            private void Sp_fistPan(Rect obj)
            {
                base.OnGUI(obj);

            }

            protected override void SearchChanged(string newSearch)
            {
                Reload();
            }
            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem() { id = -10, depth = -1 };
            }

            private long totalSize;
            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var result = GetRows() ?? new List<TreeViewItem>();
                result.Clear();
                var no_parent = cache.tree.GetNoneParent();
                var root_dirs = no_parent.FindAll(x => x.type == AssetType.Directory);
                var single_files = no_parent.FindAll(x => x.type != AssetType.Directory);
                totalSize = no_parent.Sum(x => x.length);
                if (string.IsNullOrEmpty(this.searchString))
                {
                    BuildDirs(result, root, root_dirs);
                    BuildFiles(result, root, single_files);
                }
                else
                {
                    BuildDirsForSearch(result, root, root_dirs);
                    BuildFilesForSearch(result, root, single_files);
                }
                SetupParentsAndChildrenFromDepths(root, result);

                return result;
            }
            protected override void DoubleClickedItem(int id)
            {
                var rows = this.FindRows(new List<int>() { id });
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(rows[0].displayName));
            }
            protected override void SingleClickedItem(int id)
            {
                var find = this.FindItem(id, this.rootItem);
                string path = find.displayName;
                EditorAssetData asset = cache.tree.GetAssetData(path);
                if (asset.dps.Count == 0)
                {
                    assetDp.SetAssetInfo(null);
                    dpViewType = DpViewType.None;
                }
                else
                {
                    assetDp.SetAssetInfo(asset);
                    dpViewType = DpViewType.Asset;
                }


                base.SingleClickedItem(id);
            }
            public override void OnGUI(Rect rect)
            {
                var rs = RectEx.HorizontalSplit(rect, 40);
                var rs1 = RectEx.HorizontalSplit(rs[0], 20);
                search.OnGUI(rs1[0]);
                GUI.Label(rs1[1], $"Total Size   {GetSizeString(totalSize)}");
                switch (dpViewType)
                {
                    case DpViewType.None:
                        base.OnGUI(rs[1]);
                        break;
                    case DpViewType.Asset:
                        sp.OnGUI(rs[1]);
                        break;
                }
            }



            protected override void RowGUI(RowGUIArgs args)
            {
                var data = cache.tree.GetAssetData(args.label);
                if (data == null) return;
                float indent = this.GetContentIndent(args.item);
                var first = RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0));
                if (string.IsNullOrEmpty(searchString))
                    GUI.Label(first, new GUIContent(Path.GetFileName(args.label), Textures.GetMiniThumbnail(args.label)));
                else
                    GUI.Label(first, new GUIContent(args.label, Textures.GetMiniThumbnail(args.label)));
                if (data.type != AssetType.Directory)
                {
                    GUI.Label(args.GetCellRect(1), data.type.ToString());

                    GUI.Label(args.GetCellRect(4), GetTagsString(cache.tags.GetAssetTags(data.path)));
                }
                GUI.Label(args.GetCellRect(2), GetSizeString(data.length));
                GUI.Label(args.GetCellRect(3), data.hash);
            }


            private void LoopCreateForSearch(IList<TreeViewItem> result, TreeViewItem root, EditorAssetData info)
            {
                var paths = cache.tree.GetSubFolders(info);
                var filepaths = cache.tree.GetSubFiles(info);
                if (paths.Count > 0 || filepaths.Count > 0)
                {
                    BuildDirsForSearch(result, root, paths);
                    BuildFilesForSearch(result, root, filepaths);
                }

            }
            private void BuildDirsForSearch(IList<TreeViewItem> result, TreeViewItem parent, List<EditorAssetData> dirs)
            {
                foreach (var path in dirs)
                {
                    LoopCreateForSearch(result, parent, path);
                }
            }
            private void BuildFilesForSearch(IList<TreeViewItem> result, TreeViewItem parent, List<EditorAssetData> assets)
            {
                foreach (var asset in assets)
                {
                    ;
                    var source = string.Empty;
                    switch (_searchType)
                    {
                        case SearchType.Name:
                            source = Path.GetFileName(asset.path);
                            break;
                        case SearchType.Tag:
                            source = GetTagsString(cache.tags.GetAssetTags(asset.path));
                            break;
                        case SearchType.Type:
                            source = asset.type.ToString();
                            break;
                    }
                    if (string.IsNullOrEmpty(source)) continue;
                    source = source.ToLower();
                    bool could = source.Contains(this.searchString);
                    if (could)
                    {
                        CreateItem(asset.path, parent, result);
                    }
                }
            }



            private static TreeViewItem CreateItem(string path, TreeViewItem parent, IList<TreeViewItem> result)
            {
                Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (o == null) return null;
                var _item = new TreeViewItem()
                {
                    id = o.GetInstanceID(),
                    depth = parent.depth + 1,
                    displayName = path,
                    parent = parent,
                };
                parent.AddChild(_item);
                result.Add(_item);
                return _item;
            }
            private void BuildDirs(IList<TreeViewItem> result, TreeViewItem parent, List<EditorAssetData> dirs)
            {
                foreach (var path in dirs)
                {
                    LoopCreate(result, parent, path);
                }
            }
            private static void BuildFiles(IList<TreeViewItem> result, TreeViewItem parent, List<EditorAssetData> paths)
            {
                foreach (var _path in paths)
                {
                    CreateItem(_path.path, parent, result);
                }
            }
            private void LoopCreate(IList<TreeViewItem> result, TreeViewItem parent, EditorAssetData info)
            {
                var item = CreateItem(info.path, parent, result);
                if (item == null) return;
                var paths = cache.tree.GetSubFolders(info);
                var filepaths = cache.tree.GetSubFiles(info);
                if (paths.Count > 0 || filepaths.Count > 0)
                {
                    if (IsExpanded(item.id))
                    {
                        BuildDirs(result, item, paths);
                        BuildFiles(result, item, filepaths);
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                }

            }
        }
    }
}
