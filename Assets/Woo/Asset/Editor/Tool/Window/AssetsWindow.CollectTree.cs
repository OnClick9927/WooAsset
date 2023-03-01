using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;
using System.IO;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class CollectTree : TreeView
        {
            public enum SearchType
            {
                Name,
                Tag,
                Type
            }
            private SearchField search;
            public SearchType _searchType = SearchType.Name;
            public CollectTree(TreeViewState state, SearchType _searchType) : base(state)
            {
                this._searchType = _searchType;
                search = new SearchField(this.searchString, System.Enum.GetNames(typeof(SearchType)), (int)_searchType);
                search.onValueChange += (value) => { this.searchString = value.ToLower(); };
                search.onModeChange += (value) => { this._searchType = (SearchType)value; this.Reload(); };
                showAlternatingRowBackgrounds = true;
                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    new MultiColumnHeaderState.Column()
                    { 
                        minWidth=400
                    },
                    new MultiColumnHeaderState.Column()
                    {
                         headerContent=new UnityEngine.GUIContent("Type"),
                         maxWidth=200
                    },
                    new MultiColumnHeaderState.Column()
                    {
                         headerContent=new UnityEngine.GUIContent("Tag"),
                           maxWidth=200
                    },

                }));
                this.multiColumnHeader.ResizeToFit();
                this.Reload();

            }
            protected override void SearchChanged(string newSearch)
            {
                Reload();
            }
            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem() { id = -10, depth = -1 };
            }

            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var result = GetRows() ?? new List<TreeViewItem>();
                result.Clear();
                var root_dirs = cache.GetRootDirPaths();
                var single_files = cache.GetSingleFiles();
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
            public override void OnGUI(Rect rect)
            {
                var rs = rect.HorizontalSplit(20);
                search.OnGUI(rs[0]);
                base.OnGUI(rs[1]);
            }

            protected override void ContextClicked()
            {
                var select = this.GetSelection();
                List<string> paths = new List<string>();
                foreach (var item in select)
                {
                    string path = this.FindItem(item, this.rootItem).displayName;
                    if (cache.GetAssetInfo(path).type != AssetInfo.AssetType.Directory)
                    {
                        paths.Add(path);
                    }

                }
                GenericMenu menu = new GenericMenu();
                var tags = buildSetting.tags;
                foreach (var tag in tags)
                {
                    menu.AddItem(new GUIContent($"SetTag/{tag}"), false, () =>
                    {
                        AssetsBuild.AddAssetTag(tag, paths);
                    });
                }
                menu.AddItem(new GUIContent("RemoveTag"), false, () =>
                {
                    AssetsBuild.RemoveTagAssets(paths);
                });
                menu.ShowAsContext();
            }


            protected override void RowGUI(RowGUIArgs args)
            {
                float indent = this.GetContentIndent(args.item);
                var first = args.GetCellRect(0).Zoom(AnchorType.MiddleRight, new Vector2(-indent, 0));
                if (string.IsNullOrEmpty(searchString))
                    GUI.Label(first, new GUIContent(Path.GetFileName(args.label), args.item.icon));
                else
                    GUI.Label(first, new GUIContent(args.label, args.item.icon));

                var info = cache.GetAssetInfo(args.label);
                if (info.type != AssetInfo.AssetType.Directory)
                {
                    GUI.Label(args.GetCellRect(1), info.type.ToString());
                    GUI.Label(args.GetCellRect(2), cache.GetAssetTag(info.path));
                }
            }


            private void LoopCreateForSearch(IList<TreeViewItem> result, TreeViewItem root, AssetInfo info)
            {
                var paths = cache.GetSubFolders(info);
                var filepaths = cache.GetSubFiles(info);
                if (paths.Count > 0 || filepaths.Count > 0)
                {
                    BuildDirsForSearch(result, root, paths);
                    BuildFilesForSearch(result, root, filepaths);
                }

            }
            private void BuildDirsForSearch(IList<TreeViewItem> result, TreeViewItem parent, List<AssetInfo> dirs)
            {
                foreach (var path in dirs)
                {
                    LoopCreateForSearch(result, parent, path);
                }
            }
            private void BuildFilesForSearch(IList<TreeViewItem> result, TreeViewItem parent, List<AssetInfo> assets)
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
                            source = cache.GetAssetTag(asset.path);
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
                var _item = new TreeViewItem()
                {
                    id = o.GetInstanceID(),
                    depth = parent.depth + 1,
                    displayName = path,
                    parent = parent,
                    icon = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path))
                };
                parent.AddChild(_item);
                result.Add(_item);
                return _item;
            }
            private void BuildDirs(IList<TreeViewItem> result, TreeViewItem parent, List<AssetInfo> dirs)
            {
                foreach (var path in dirs)
                {
                    LoopCreate(result, parent, path);
                }
            }
            private static void BuildFiles(IList<TreeViewItem> result, TreeViewItem parent, List<AssetInfo> paths)
            {
                foreach (var _path in paths)
                {
                    CreateItem(_path.path, parent, result);
                }
            }
            private void LoopCreate(IList<TreeViewItem> result, TreeViewItem parent, AssetInfo info)
            {
                var item = CreateItem(info.path, parent, result);
                var paths = cache.GetSubFolders(info);
                var filepaths = cache.GetSubFiles(info);
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
