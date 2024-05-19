using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace WooAsset
{
    partial class AssetsWindow
    {

        private class AssetsTree : AssetTreeBase, IPing<EditorAssetData>
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
            private AssetUsageTree assetUsage;

            private SplitView sp = new SplitView() { vertical = false, minSize = 200, split = 300 };
            private SplitView sp2 = new SplitView() { vertical = true, minSize = 200, split = 300 };

            [Flags]
            private enum DpViewType
            {
                None = 0,
                Asset = 2,
                Usage = 4,
            }
            private DpViewType dpViewType = DpViewType.None;
            public AssetsTree(TreeViewState state, SearchType _searchType) : base(state, null)
            {
                this.ping = this;
                assetDp = new AssetDpTree(new TreeViewState(), this);
                assetUsage = new AssetUsageTree(new TreeViewState(), this);
                this._searchType = _searchType;
                search = new SearchField(this.searchString, System.Enum.GetNames(typeof(SearchType)), (int)_searchType);
                search.onValueChange += (value) => { this.searchString = value.ToLower(); };
                search.onModeChange += (value) => { this._searchType = (SearchType)value; this.Reload(); };
            }
            protected override MultiColumnHeaderState.Column GetFirtColumn() => TreeColumns.emptyTitle;

            protected override void SearchChanged(string newSearch) => Reload();


            private long totalSize;
            protected override void CreateRows(TreeViewItem root, IList<TreeViewItem> result)
            {
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
            }

            protected override void DoubleClickedItem(int id)
            {
                var rows = this.FindRows(new List<int>() { id });
                if (string.IsNullOrEmpty(searchString))
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(rows[0].displayName));
                else
                    base.DoubleClickedItem(id);

            }
            protected override void SingleClickedItem(int id)
            {
                var find = this.FindItem(id, this.rootItem);
                string path = find.displayName;
                EditorAssetData asset = cache.tree.GetAssetData(path);
                dpViewType = DpViewType.None;
                if (asset.usageCount > 0)
                {
                    dpViewType |= DpViewType.Usage;
                    assetUsage.SetAssetInfo(asset);
                }
                else
                    assetUsage.SetAssetInfo(null);
                if (asset.dependence.Count == 0)
                    assetDp.SetAssetInfo(null);
                else
                {
                    assetDp.SetAssetInfo(asset);
                    dpViewType |= DpViewType.Asset;
                }


                base.SingleClickedItem(id);
            }
            public override void OnGUI(Rect rect)
            {
                var rs = RectEx.HorizontalSplit(rect, 40);
                var rs1 = RectEx.HorizontalSplit(rs[0], 20);
                search.OnGUI(rs1[0]);
                GUI.Label(rs1[1], $"Total Size   {GetSizeString(totalSize)}");
                if (dpViewType == DpViewType.None)
                {
                    base.OnGUI(rs[1]);
                }
                else
                {

                    sp.OnGUI(rs[1]);
                    base.OnGUI(sp.rects[0]);
                    if (dpViewType == DpViewType.Asset)
                        assetDp.OnGUI(sp.rects[1]);
                    else if (dpViewType == DpViewType.Usage)
                        assetUsage.OnGUI(sp.rects[1]);
                    else
                    {
                        sp2.OnGUI(sp.rects[1]);
                        assetDp.OnGUI(sp2.rects[0]);
                        assetUsage.OnGUI(sp2.rects[1]);

                    }

                }
            }

            private void CollectPath(List<string> paths, List<string> result)
            {
                var tree = AssetsEditorTool.cache.tree;
                foreach (var path in paths)
                {
                    var data = tree.GetAssetData(path);
                    if (data.type == AssetType.Directory)
                    {
                        var folders = tree.GetSubFolders(data).ConvertAll(x => x.path);
                        var files = tree.GetSubFiles(data).ConvertAll(x => x.path);

                        CollectPath(folders, result);
                        CollectPath(files, result);
                    }
                    else
                    {
                        result.Add(path);
                    }
                }

            }
            protected override void ContextClicked()
            {
                var selection = this.GetSelection();
                var rows = this.FindRows(selection).ToList().ConvertAll(x => x.displayName);
                List<string> paths = new List<string>();
                CollectPath(rows, paths);
                var tags = AssetsEditorTool.option.GetAllTags();
                GenericMenu menu = new GenericMenu();
                foreach (var tag in tags)
                {
                    menu.AddItem(new GUIContent($"tag/add/{tag}"), false, () =>
                    {
                        foreach (var path in paths)
                            AssetsEditorTool.option.AddAssetTag(path, tag);
                        AssetsEditorTool.option.Save();
                        AssetTaskRunner.PreviewAllAssets();

                    });
                    menu.AddItem(new GUIContent($"tag/remove/{tag}"), false, () =>
                    {
                        foreach (var path in paths)
                            AssetsEditorTool.option.RemoveAssetTag(path, tag);
                        AssetsEditorTool.option.Save();
                        AssetTaskRunner.PreviewAllAssets();
                    });

                }

                menu.ShowAsContext();

            }

            private bool DirectoryLoop(EditorAssetData data)
            {
                var tree = AssetsEditorTool.cache.tree;
                var folds = tree.GetSubFolders(data);
                var files = tree.GetSubFiles(data);
                foreach (var file in files)
                    if (file.loopDependence)
                        return true;
                foreach (var fold in folds)
                    if (DirectoryLoop(fold))
                        return true;
                return false;
            }
            protected override void RowGUI(RowGUIArgs args)
            {
                var asset = cache.tree.GetAssetData(args.label);
                if (asset == null) return;
                if (asset.type != AssetType.Directory)
                    base.RowGUI(args);
                else
                {
                    float indent = this.GetContentIndent(args.item);
                    var first = RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0));
                    GUI.Label(first, GUIContent(args.label, Textures.GetMiniThumbnail(args.label)));
                    if (DirectoryLoop(asset))
                        GUI.Label(args.GetCellRect(1), Textures.err);
                    GUI.Label(args.GetCellRect(5), GetSizeString(asset.length));
                }
                if (ping_a == asset)
                    DrawPing(args.rowRect);
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
                foreach (var path in dirs) LoopCreateForSearch(result, parent, path);
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
                            source = AssetsHelper.GetFileName(asset.path);
                            break;
                        case SearchType.Tag:
                            source = GetTagsString(asset);
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
                        CreateItem(asset.path, parent, result, parent.depth + 1);
                    }
                }
            }

            private void BuildDirs(IList<TreeViewItem> result, TreeViewItem parent, List<EditorAssetData> dirs)
            {
                foreach (var path in dirs) LoopCreate(result, parent, path);
            }
            private static void BuildFiles(IList<TreeViewItem> result, TreeViewItem parent, List<EditorAssetData> paths)
            {
                foreach (var _path in paths) CreateItem(_path.path, parent, result, parent.depth + 1);
            }
            private void LoopCreate(IList<TreeViewItem> result, TreeViewItem parent, EditorAssetData info)
            {
                var item = CreateItem(info.path, parent, result, parent.depth + 1);
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
            EditorAssetData ping_a;

            public async void Ping(EditorAssetData obj)
            {
                if (ping_a != null) return;
                search.SetVelue("");
                var find = cache.tree.GetAllAssets().Where(x => x != obj && obj.path.Contains(x.path)).ToList();
                find.Sort((a, b) => { return a.path.Length - b.path.Length > 0 ? 1 : 1; });
                foreach (var item in find)
                {
                    var _id = AssetDatabase.LoadAssetAtPath<Object>(item.path).GetInstanceID();
                    this.SetExpanded(_id, true);
                }
                Reload();
                ping_a = obj;
                var id = AssetDatabase.LoadAssetAtPath<Object>(obj.path).GetInstanceID();
                this.FrameItem(id);
                await Task.Delay(1000);
                ping_a = null;
                Reload();
            }
        }
    }
}
