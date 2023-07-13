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
        interface IPing<T>
        {
            void Ping(T obj);
        }
        private class BundlesTree : TreeView, IPing<BundleGroup>, IPing<EditorAssetData>
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
                BundleUsage = 8
            }
            private DpViewType dpViewType = DpViewType.None;
            List<BundleGroup> previewBundles { get { return cache.previewBundles; } }
            public SearchType _searchType;
            private SearchField search;
            private AssetDpTree assetDp;
            private BundleDpTree bundleDp;
            private BundleUsageTree bundleUsage;

            private AssetUsageTree assetUsage;

            private SplitView sp = new SplitView() { vertical = false, minSize = 200, split = 300 };
            private SplitView sp2 = new SplitView() { vertical = true, minSize = 200, split = 300 };

            public BundlesTree(TreeViewState state, SearchType _searchType) : base(state)
            {
                assetDp = new AssetDpTree(new TreeViewState(), this);
                bundleDp = new BundleDpTree(new TreeViewState(), this);
                bundleUsage = new BundleUsageTree(new TreeViewState(), this);
                assetUsage = new AssetUsageTree(new TreeViewState(), this);
                this._searchType = _searchType;
                search = new SearchField(this.searchString, System.Enum.GetNames(typeof(SearchType)), (int)_searchType);
                search.onValueChange += (value) => { this.searchString = value.ToLower(); };
                search.onModeChange += (value) => { this._searchType = (SearchType)value; this.Reload(); };

                showAlternatingRowBackgrounds = true;

                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    TreeColumns.emptyTitle,
                    TreeColumns.usageCount,
                    TreeColumns.depenceCount,
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
                else if (dpViewType.HasFlag(DpViewType.Bundle) || dpViewType.HasFlag(DpViewType.BundleUsage))
                {
                    sp.OnGUI(rs[1]);
                    base.OnGUI(sp.rects[0]);
                    if (dpViewType == DpViewType.Bundle)
                        bundleDp.OnGUI(sp.rects[1]);
                    else if (dpViewType == DpViewType.BundleUsage)
                        bundleUsage.OnGUI(sp.rects[1]);
                    else
                    {
                        sp2.OnGUI(sp.rects[1]);
                        bundleDp.OnGUI(sp2.rects[0]);
                        bundleUsage.OnGUI(sp2.rects[1]);
                    }
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

            protected override void RowGUI(RowGUIArgs args)
            {
                bool draw = false;

                float indent = this.GetContentIndent(args.item);
                var first = RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0));
                long length = 0;
                if (args.item.depth == 0)
                {
                    GUI.Label(first, new GUIContent(args.label, Textures.folder));
                    BundleGroup group = cache.GetBundleGroupByBundleName(args.label);
                    
                    GUI.Label(args.GetCellRect(1), group.usage.Count.ToString());
                    GUI.Label(args.GetCellRect(2), group.dependence.Count.ToString());
                    if (group != null)
                        length = group.length;
                    if (ping_g == group) draw = true;
                }
                else
                {
                    string path = args.label;
                    EditorAssetData asset = cache.tree.GetAssetData(path);
                    length = asset.length;
                    GUI.Label(first, new GUIContent(path, Textures.GetMiniThumbnail(path)));
                    GUI.Label(args.GetCellRect(1), asset.usageCount.ToString());
                    GUI.Label(args.GetCellRect(2), asset.dps.Count.ToString());
                    GUI.Label(args.GetCellRect(4), asset.hash);
                    BundleGroup group = cache.GetBundleGroupByAssetPath(path);
                    if (group != null)
                    {
                        EditorGUI.SelectableLabel(args.GetCellRect(5), group.hash);
                    }
                    GUI.Label(args.GetCellRect(6), GetTagsString(cache.tags.GetAssetTags(path)));
                    if (ping_a == asset) draw = true;
                }

                GUI.Label(args.GetCellRect(3), GetSizeString(length));
                if (draw)
                    GUI.Label(RectEx.Zoom(args.rowRect, TextAnchor.MiddleCenter, -8), "", "LightmapEditorSelectedHighlight");
            }

            protected override void DoubleClickedItem(int id)
            {
                var rows = this.FindRows(new List<int>() { id });

                if (string.IsNullOrEmpty(searchString))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(rows[0].displayName));
                }
                else
                {
                    if (_searchType == SearchType.Bundle)
                        Ping(cache.GetBundleGroupByBundleName(rows[0].displayName));
                    else
                        Ping(cache.tree.GetAssetData(rows[0].displayName));
                }
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
                    if (asset.usageCount > 0)
                    {
                        dpViewType |= DpViewType.Usage;
                        assetUsage.SetAssetInfo(asset);
                    }
                    else
                        assetUsage.SetAssetInfo(null);

                    bundleDp.SetBundleGroup(null);
                    bundleUsage.SetBundleGroup(null);
                }
                else if (find.depth == 0)
                {
                    BundleGroup group = cache.GetBundleGroupByBundleName(path);
                    if (group.dependence.Count != 0)
                    {
                        bundleDp.SetBundleGroup(group);
                        dpViewType |= DpViewType.Bundle;
                    }
                    else
                        bundleDp.SetBundleGroup(null);
                    if (group.usage.Count != 0)
                    {
                        bundleUsage.SetBundleGroup(group);
                        dpViewType |= DpViewType.BundleUsage;
                    }
                    else
                        bundleUsage.SetBundleGroup(null);
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

            public async void Ping(BundleGroup group)
            {
                if (ping_g != null) return;
                ping_g = group;
                search.SetVelue("");
                var index = this.previewBundles.IndexOf(group);
                this.FrameItem(index);
                await Task.Delay(1000);
                ping_g = null;
                Reload();

            }
            BundleGroup ping_g;

            EditorAssetData ping_a;
            public async void Ping(EditorAssetData obj)
            {
                if (ping_a != null) return;
                search.SetVelue("");
                var id = AssetDatabase.LoadAssetAtPath<Object>(obj.path).GetInstanceID();
                var group = cache.GetBundleGroupByAssetPath(obj.path);
                var index = this.previewBundles.IndexOf(group);
                this.SetExpanded(index, true);
                Reload();
                ping_a = obj;
                this.FrameItem(id);
                await Task.Delay(1000);
                ping_a = null;
                Reload();
            }
        }


    }
}
