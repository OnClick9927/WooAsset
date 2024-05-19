using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEditor;
using static WooAsset.AssetsEditorTool;
using System.Linq;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class LifeTree : TreeView
        {
            public enum SearchType
            {
                Asset,
                Bundle
            }
            public SearchType _searchType = SearchType.Bundle;
            private SearchField search;
            private long totalBundleSize = 0;
            public LifeTree(TreeViewState state, SearchType _searchType) : base(state)
            {
                this._searchType = _searchType;
                search = new SearchField(this.searchString, System.Enum.GetNames(typeof(SearchType)), (int)_searchType);
                search.onValueChange += (value) => { this.searchString = value.ToLower(); };
                search.onModeChange += (value) => { this._searchType = (SearchType)value; this.Reload(); };
                showAlternatingRowBackgrounds = true;
                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
{
                 TreeColumns.emptyTitle,
             TreeColumns.reference,
             TreeColumns.loadTime,
                          TreeColumns.size,

             TreeColumns.type,
                 TreeColumns.bundle,
             TreeColumns.tag,
}));

                this.multiColumnHeader.ResizeToFit();
                this.Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                totalBundleSize = AssetsEditorTool.bundles.Values.Sum(x => x.assetLength);
                return new TreeViewItem() { id = -10, depth = -1 };
            }
            public void BuildBundles(TreeViewItem root, IList<TreeViewItem> result)
            {
                foreach (var life in AssetsEditorTool.bundles.Keys)
                {
                    TreeViewItem item = new TreeViewItem()
                    {
                        id = life.GetHashCode(),
                        depth = 0,
                        displayName = life,
                        parent = root,
                    };

                    if (!DoesItemMatchSearch(item, this.searchString)) continue;
                    result.Add(item);
                    Bundle bundle = AssetsEditorTool.bundles[life].asset;
                    var r = AssetsInternal.GetAllAssetPaths(bundle.bundleName);
                    if (r == null) continue;
                    var paths = r.ToList().FindAll(x => AssetsHelper.GetOrDefaultFromDictionary(AssetsEditorTool.assets, x) != null);
                    if (paths == null && paths.Count == 0) continue;
                    if (IsExpanded(item.id) || !string.IsNullOrEmpty(this.searchString))
                    {
                        for (int i = 0; i < paths.Count; i++)
                        {
                            var path = paths[i];
                            TreeViewItem _item = new TreeViewItem()
                            {
                                id = path.GetHashCode(),
                                depth = item.depth + 1,
                                displayName = path,
                            };
                            result.Add(_item);
                        }
                    }
                    else
                        item.children = CreateChildListForCollapsedParent();
                }
            }
            public void BuildAssets(TreeViewItem root, IList<TreeViewItem> result)
            {
                foreach (var path in AssetsEditorTool.assets.Keys)
                {
                    TreeViewItem item = new TreeViewItem()
                    {
                        id = path.GetHashCode(),
                        depth = 0,
                        parent = root,
                        displayName = path,
                    };
                    if (DoesItemMatchSearch(item, this.searchString))
                    {
                        result.Add(item);
                    }
                }

            }
            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var result = GetRows() ?? new List<TreeViewItem>();
                result.Clear();
                if (_searchType == SearchType.Bundle)
                {
                    BuildBundles(root, result);
                }
                else
                {
                    BuildAssets(root, result);
                }
                SetupParentsAndChildrenFromDepths(root, result);
                return result;
            }
            public override void OnGUI(Rect rect)
            {
                var rs = RectEx.HorizontalSplit(rect, 40);
                var rs1 = RectEx.HorizontalSplit(rs[0], 20);
                search.OnGUI(rs1[0]);
                GUI.Label(rs1[1], $"Total Bundle Size   {GetSizeString(totalBundleSize)}");
                base.OnGUI(rs[1]);
            }

            private void DrawBundle(RowGUIArgs args)
            {
                string name = args.item.displayName;

                AssetLife<Bundle> life;
                if (AssetsEditorTool.bundles.TryGetValue(name, out life))
                {
                    float indent = this.GetContentIndent(args.item);
                    GUI.Label(RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0)), GUIContent(AssetsHelper.GetFileName(name), Textures.folder));
                    GUI.Label(args.GetCellRect(1), life.asset.refCount.ToString());
                    GUI.Label(args.GetCellRect(2), life.asset.time.ToString());
                    GUI.Label(args.GetCellRect(3), GetSizeString(life.assetLength));

                }
            }
            private void DrawAsset(RowGUIArgs args)
            {
                string name = args.item.displayName;

                AssetLife<AssetHandle> life;
                if (AssetsEditorTool.assets.TryGetValue(name, out life))
                {
                    float indent = _searchType == SearchType.Asset ? 0 : string.IsNullOrEmpty(searchString) ? this.GetContentIndent(args.item) : 30;
                    GUI.Label(RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0)), GUIContent(name, Textures.GetMiniThumbnail(args.label)));
                    GUI.Label(args.GetCellRect(1), life.asset.refCount.ToString());
                    GUI.Label(args.GetCellRect(2), life.asset.time.ToString());
                    //GUI.Label(args.GetCellRect(3), GetSizeString(life.assetLength));
                    GUI.Label(args.GetCellRect(4), life.assetType.ToString());
                    EditorGUI.SelectableLabel(args.GetCellRect(5), AssetsHelper.GetFileName(life.asset.bundleName));
                    GUI.Label(args.GetCellRect(6), GetTagsString(life.tags));
                }
            }
            protected override void RowGUI(RowGUIArgs args)
            {
                if (_searchType == SearchType.Bundle)
                {
                    if (args.item.depth == 0)
                    {
                        DrawBundle(args);
                    }
                    else if (args.item.depth == 1)
                    {
                        DrawAsset(args);
                    }
                }
                else
                {
                    DrawAsset(args);
                }
            }
            protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
            {
                if (string.IsNullOrEmpty(search))
                    return true;
                return item.displayName.ToLower().Contains(search);
            }
            protected override void SearchChanged(string newSearch)
            {
                Reload();
            }
        }
    }
}
