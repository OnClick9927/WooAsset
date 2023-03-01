using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class PreviewTree : TreeView
        {
            public enum SearchType
            {
                Bundle,
                AssetByPath,
                AssetByTag

            }
            List<BundleGroup> previewBundles { get { return cache.GetPreviewBundles(); } }
            public SearchType _searchType;
            private SearchField search;
            private DpTree dp;
            private SplitView sp = new SplitView() { splitType = SplitType.Horizontal, minSize = 200, split = 300 };
            public PreviewTree(TreeViewState state, SearchType _searchType) : base(state)
            {
                dp = new DpTree(new TreeViewState());

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
                         headerContent=new UnityEngine.GUIContent("Size"),
                         maxWidth=150
                    },
                       new MultiColumnHeaderState.Column()
                    {
                         headerContent=new UnityEngine.GUIContent("Tag"),
                             maxWidth=150
                    },

               }));

                this.multiColumnHeader.ResizeToFit();
                Reload();
                sp.fistPan += Sp_fistPan;
                sp.secondPan += Sp_secondPan;
            }

            private void Sp_secondPan(Rect obj)
            {
                dp.OnGUI(obj);
            }

            private void Sp_fistPan(Rect obj)
            {
                base.OnGUI(obj);
            }

            protected override void SearchChanged(string newSearch)
            {
                Reload();
            }
            public override void OnGUI(Rect rect)
            {
                var rs = rect.HorizontalSplit(20);
                search.OnGUI(rs[0]);
                if (dp.asset == null || dp.asset.dps.Count == 0)
                {
                    base.OnGUI(rs[1]);
                }
                else
                {
                    sp.OnGUI(rs[1]);
                }

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
                    icon = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path))
                };
                _item.parent = parent;
                parent.AddChild(_item);
                result.Add(_item);
                return _item;
            }

            private TreeViewItem BuildBundle(int i, TreeViewItem root, IList<TreeViewItem> result)
            {
                var bundle = previewBundles[i];
                var _item = new TreeViewItem()
                {
                    id = i,
                    depth = 0,
                    parent = root,
                    displayName = bundle.name,
                    icon = EditorGUIUtility.TrIconContent("Folder Icon").image as Texture2D,


                };
                root.AddChild(_item);
                result.Add(_item);
                return _item;
            }
            private void InnerBuildRows(TreeViewItem root, IList<TreeViewItem> result)
            {
                for (int i = 0; i < previewBundles.Count; i++)
                {
                    var bundle = previewBundles[i];

                    if (string.IsNullOrEmpty(searchString))
                    {

                        var _item = BuildBundle(i, root, result);
                        if (bundle.assets.Count <= 0) continue;
                        if (IsExpanded(_item.id))
                        {
                            for (int j = 0; j < bundle.assets.Count; j++)
                            {
                                var path = bundle.assets[j];
                                CreateItem(path, _item, result);
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
                            if (!bundle.name.ToLower().Contains(searchString)) continue;
                            BuildBundle(i, root, result);
                        }
                        else
                        {
                            if (bundle.assets.Count <= 0) continue;
                            for (int j = 0; j < bundle.assets.Count; j++)
                            {
                                var path = bundle.assets[j];
                                if (_searchType == SearchType.AssetByPath)
                                {
                                    if (!path.ToLower().Contains(searchString)) continue;
                                }
                                else if (_searchType == SearchType.AssetByTag)
                                {
                                    var tag = cache.GetAssetTag(path);
                                    if (string.IsNullOrEmpty(tag)) continue;
                                    if (!tag.Contains(searchString)) continue;
                                }
                                CreateItem(path, root, result);
                            }
                        }

                    }
                }
            }
            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var result = GetRows() ?? new List<TreeViewItem>();
                result.Clear();

                if (previewBundles != null)
                {
                    InnerBuildRows(root, result);
                }

                SetupParentsAndChildrenFromDepths(root, result);
                return result;
            }
            protected override void RowGUI(RowGUIArgs args)
            {
                float indent = this.GetContentIndent(args.item);
                var first = args.GetCellRect(0).Zoom(AnchorType.MiddleRight, new Vector2(-indent, 0));
                long length = 0;
                if (args.item.depth == 0)
                {
                    GUI.Label(first, new GUIContent(args.label, args.item.icon));
                    BundleGroup group = cache.GetBundleGroupByBundleName(args.label);
                    length = group.length;
                }
                else
                {
                    string path = args.label;
                    GUI.Label(first, new GUIContent(path, args.item.icon));
                    BundleGroup group = cache.GetBundleGroupByAssetPath(path);
                    length = group.GetLength(path);
                    GUI.Label(args.GetCellRect(2), new GUIContent(cache.GetAssetTag(path)));
                }

                GUI.Label(args.GetCellRect(1), GetSizeString(length));

            }
            public static string GetSizeString(long length)
            {
                var tmp = length;

                int stage = 0;
                while (tmp > 1024)
                {
                    tmp /= 1024;
                    stage++;
                }
                return $"{(length / Mathf.Pow(1024, stage)).ToString("0.00")} {stages[stage]}";
            }
            private static List<string> stages = new List<string>()
            {
                "B","KB","MB","GB","TB"
            };
            protected override void DoubleClickedItem(int id)
            {
                var rows = this.FindRows(new List<int>() { id });
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(rows[0].displayName));
            }
            protected override void SingleClickedItem(int id)
            {
                var find = this.FindItem(id, this.rootItem);
                if (find.depth == 1)
                {
                    string path = find.displayName;
                    dp.SetAssetInfo(cache.GetAssetInfo(path));
                }
                else
                {
                    dp.SetAssetInfo(null);
                }
                base.SingleClickedItem(id);
            }
            protected override bool CanMultiSelect(TreeViewItem item)
            {
                return false;
            }

        }
    }
}
