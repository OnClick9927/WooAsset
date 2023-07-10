using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;
using System.Linq;
using System;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class BundlesTree : TreeView
        {
            public enum SearchType
            {
                Bundle,
                AssetByPath,
                AssetByTag

            }
            [Flags]
            private enum DpViewType
            {
                None = 0,
                Asset = 1,
                Bundle = 2,
                Usage = 4,
            }
            private DpViewType dpViewType = DpViewType.None;
            List<BundleGroup> previewBundles { get { return cache.previewBundles; } }
            public SearchType _searchType;
            private SearchField search;
            private AssetDpTree assetDp;
            private BundleDpTree bundleDp;
            private AssetUsageTree assetUsage;

            private SplitView sp = new SplitView() { vertical = false, minSize = 200, split = 300 };
            private SplitView sp2 = new SplitView() { vertical = true, minSize = 200, split = 300 };

            public BundlesTree(TreeViewState state, SearchType _searchType) : base(state)
            {
                assetDp = new AssetDpTree(new TreeViewState());
                bundleDp = new BundleDpTree(new TreeViewState());
                assetUsage=new AssetUsageTree(new TreeViewState());
                this._searchType = _searchType;
                search = new SearchField(this.searchString, System.Enum.GetNames(typeof(SearchType)), (int)_searchType);
                search.onValueChange += (value) => { this.searchString = value.ToLower(); };
                search.onModeChange += (value) => { this._searchType = (SearchType)value; this.Reload(); };

                showAlternatingRowBackgrounds = true;

                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    TreeColumns.emptyTitle,
                    TreeColumns.size,
                     TreeColumns.hash,
                     TreeColumns.bundle,
                    TreeColumns.tag,

               }));

                this.multiColumnHeader.ResizeToFit();
                Reload();

            }

            protected override void SearchChanged(string newSearch)
            {
                Reload();
            }
            public override void OnGUI(Rect rect)
            {
                var rs = RectEx.HorizontalSplit(rect, 40);
                var rs1 = RectEx.HorizontalSplit(rs[0], 20);
                search.OnGUI(rs1[0]);
                GUI.Label(rs1[1], $"Total Size   {GetSizeString(totalSize)}");
                if (dpViewType == DpViewType.None)
                    base.OnGUI(rs[1]);
                else if (dpViewType == DpViewType.Bundle)
                {
                    sp.OnGUI(rs[1]);
                    base.OnGUI(sp.rects[0]);
                    bundleDp.OnGUI(sp.rects[1]);

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

            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem() { id = -10, depth = -1 };
            }



            private void CreateItem(string path, TreeViewItem parent, IList<TreeViewItem> result)
            {
                Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (o == null) return;
                var _item = new TreeViewItem()
                {
                    id = o.GetInstanceID(),
                    depth = 1,
                    displayName = path,
                };
                _item.parent = parent;
                parent.AddChild(_item);
                result.Add(_item);
                return;
            }

            private TreeViewItem BuildBundle(int i, TreeViewItem root, IList<TreeViewItem> result)
            {
                var bundle = previewBundles[i];
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
            private void InnerBuildRows(TreeViewItem root, IList<TreeViewItem> result)
            {
                int count = 0;
                for (int i = 0; i < previewBundles.Count; i++)
                {
                    var bundle = previewBundles[i];

                    if (string.IsNullOrEmpty(searchString))
                    {

                        var _item = BuildBundle(i, root, result);
                        count++;
                        if (count > 100) return;
                        if (bundle.assetCount <= 0) continue;
                        if (IsExpanded(_item.id))
                        {
                            var assets = bundle.GetAssets();
                            for (int j = 0; j < assets.Count; j++)
                            {
                                CreateItem(assets[j], _item, result);
                            }
                        }
                        else
                        {
                            _item.children = CreateChildListForCollapsedParent();
                        }
                    }
                    else
                    {
                        if (_searchType == SearchType.Bundle)
                        {
                            if (!bundle.hash.ToLower().Contains(searchString)) continue;
                            BuildBundle(i, root, result);
                        }
                        else
                        {
                            if (bundle.assetCount <= 0) continue;
                            var assets = bundle.GetAssets();
                            for (int j = 0; j < assets.Count; j++)
                            {
                                var path = assets[j];
                                if (_searchType == SearchType.AssetByPath)
                                {
                                    if (!path.ToLower().Contains(searchString)) continue;
                                }
                                else if (_searchType == SearchType.AssetByTag)
                                {
                                    var tag = cache.tags.GetAssetTags(path);
                                    if (tag == null || tag.Count == 0) continue;
                                    if (tag.ToList().Find(x => x.ToLower().Contains(searchString)) != null) continue;
                                }
                                CreateItem(path, root, result);
                            }
                        }

                    }
                }
            }

            private long totalSize;

            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var result = GetRows() ?? new List<TreeViewItem>();
                result.Clear();
                totalSize = 0;
                if (previewBundles != null)
                {
                    totalSize = previewBundles.Sum(x => x.length);
                    InnerBuildRows(root, result);
                }

                SetupParentsAndChildrenFromDepths(root, result);
                return result;
            }
            private BundleGroup GetBundleGroupByAssetPath(string assetPath)
            {
                return previewBundles.Find(x => x.ContainsAsset(assetPath));
            }
            private BundleGroup GetBundleGroupByBundleName(string bundleName)
            {
                return previewBundles.Find(x => x.hash == bundleName);
            }
            protected override void RowGUI(RowGUIArgs args)
            {

                float indent = this.GetContentIndent(args.item);
                var first = RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0));
                long length = 0;
                if (args.item.depth == 0)
                {
                    GUI.Label(first, new GUIContent(args.label, Textures.folder));
                    BundleGroup group = GetBundleGroupByBundleName(args.label);
                    if (group != null)
                        length = group.length;
                }
                else
                {
                    string path = args.label;
                    GUI.Label(first, new GUIContent(path, Textures.GetMiniThumbnail(path)));
                    BundleGroup group = GetBundleGroupByAssetPath(path);
                    if (group != null)
                    {
                        EditorAssetData asset = cache.tree.GetAssetData(path);
                        length = asset.length;
                        GUI.Label(args.GetCellRect(2), asset.hash);
                        EditorGUI.SelectableLabel(args.GetCellRect(3), group.hash);
                    }
                    GUI.Label(args.GetCellRect(4), GetTagsString(cache.tags.GetAssetTags(path)));
                }

                GUI.Label(args.GetCellRect(1), GetSizeString(length));

            }

            protected override void DoubleClickedItem(int id)
            {
                var rows = this.FindRows(new List<int>() { id });
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(rows[0].displayName));
            }
            private List<BundleGroup> GetDependenceBundleGroup(BundleGroup group, List<BundleGroup> result)
            {
                if (result == null)
                    result = new List<BundleGroup>();
                foreach (var assetPath in group.GetAssets())
                {
                    EditorAssetData data = cache.tree.GetAssetData(assetPath);
                    if (data != null)
                    {
                        var dps = data.dps;
                        foreach (var dp in dps)
                        {
                            BundleGroup _group = GetBundleGroupByAssetPath(dp);
                            result.Add(_group);
                            GetDependenceBundleGroup(_group, result);
                        }
                    }

                }
                return result.Distinct().ToList();
            }
            protected override void SingleClickedItem(int id)
            {
                var find = this.FindItem(id, this.rootItem);
                string path = find.displayName;
                dpViewType = DpViewType.None;

                if (find.depth == 1)
                {
                    var asset = cache.tree.GetAssetData(path);
                    if (asset.dps.Count != 0)
                    {
                        assetDp.SetAssetInfo(asset);
                        dpViewType |= DpViewType.Asset;
                    }
                    else
                        assetDp.SetAssetInfo(null);
                    var useage = cache.tree.GetUsage(asset);
                    if (useage != null && useage.Count > 0)
                    {
                        dpViewType |= DpViewType.Usage;
                        assetUsage.SetAssetInfo(asset);
                    }
                    else
                        assetUsage.SetAssetInfo(null);

                    bundleDp.SetBundleGroup(null);

                }
                else if (find.depth == 0)
                {
                    BundleGroup group = GetBundleGroupByBundleName(path);
                    var groups = GetDependenceBundleGroup(group, new List<BundleGroup>());
                    if (groups.Count != 0)
                    {
                        bundleDp.SetBundleGroup(groups);
                        dpViewType |= DpViewType.Bundle;
                    }
                    else
                        bundleDp.SetBundleGroup(null);
                    assetDp.SetAssetInfo(null);
                    assetUsage.SetAssetInfo(null);

                }

                base.SingleClickedItem(id);
            }
            protected override bool CanMultiSelect(TreeViewItem item)
            {
                return false;
            }
            public void OnReload()
            {
                dpViewType = DpViewType.None;
                this.SetSelection(new List<int>());
                Reload();
            }

        }
    }
}
