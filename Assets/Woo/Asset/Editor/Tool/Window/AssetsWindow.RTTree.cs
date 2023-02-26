using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEditor;

using System.IO;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{


    partial class AssetsWindow
    {

        private class RTTree : TreeView
        {
            public enum SearchType
            {
                Asset,
                Bundle
            }
            public SearchType _searchType = SearchType.Bundle;
            private SearchField search;
            public RTTree(TreeViewState state, SearchType _searchType) : base(state)
            {
                this._searchType = _searchType;
                search = new SearchField(this.searchString, System.Enum.GetNames(typeof(SearchType)), (int)_searchType);
                search.onValueChange += (value) => { this.searchString = value.ToLower(); };
                search.onModeChange += (value) => { this._searchType = (SearchType)value; this.Reload(); };
                showAlternatingRowBackgrounds = true;

                this.Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem() { id = -10, depth = -1 };
            }
            public void BuildBundles(TreeViewItem root, IList<TreeViewItem> result)
            {
                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    new MultiColumnHeaderState.Column()
                    {
                        minWidth=200,
                        headerContent=new UnityEngine.GUIContent("Bundle"),

                    },
                    new MultiColumnHeaderState.Column()
                    {
                         headerContent=new UnityEngine.GUIContent("Ref"),
                         minWidth=100,
                         maxWidth=100
                    },
                    new MultiColumnHeaderState.Column()
                    {
                         headerContent=new UnityEngine.GUIContent("Load Time"),
                           maxWidth=200
                    },

                 }));
                this.multiColumnHeader.ResizeToFit();
                int index = 0;
                foreach (var life in AssetsEditorTool.bundles.Keys)
                {
                    TreeViewItem item = new TreeViewItem()
                    {
                        id = index++,
                        depth = 1,
                        parent = root,
                        displayName = life,
                        icon = EditorGUIUtility.TrIconContent("Folder Icon").image as Texture2D
                    };
                    if (DoesItemMatchSearch(item, this.searchString))
                    {
                        result.Add(item);
                    }
                }
            }
            public void BuildAssets(TreeViewItem root, IList<TreeViewItem> result)
            {
                int index = 0;
                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
        {
                    new MultiColumnHeaderState.Column()
                    {
                        minWidth=400,
                        headerContent=new UnityEngine.GUIContent("Asset"),
                    },
                    new MultiColumnHeaderState.Column()
                    {
                         headerContent=new UnityEngine.GUIContent("Tag"),
                         minWidth=100,
                         maxWidth=100
                    },
                    new MultiColumnHeaderState.Column()
                    {
                         headerContent=new UnityEngine.GUIContent("Ref"),
                         minWidth=100,
                         maxWidth=100
                    },
                    new MultiColumnHeaderState.Column()
                    {
                         headerContent=new UnityEngine.GUIContent("Load Time"),
                           maxWidth=200
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        minWidth=200,
                        headerContent=new UnityEngine.GUIContent("Bundle"),
                    },
                }));
                this.multiColumnHeader.ResizeToFit();

                foreach (var life in AssetsEditorTool.assets.Keys)
                {
                    TreeViewItem item = new TreeViewItem()
                    {
                        id = index++,
                        depth = 1,
                        parent = root,
                        displayName = life,
                        icon = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(life))
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
                var rs = rect.HorizontalSplit(20);
                search.OnGUI(rs[0]);
                base.OnGUI(rs[1]);
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                if (_searchType == SearchType.Bundle)
                {
                    string name = args.item.displayName;
                    AssetLife<Bundle> life = AssetsEditorTool.bundles[name];
                    GUI.Label(args.GetCellRect(0), new GUIContent(Path.GetFileName(name), args.item.icon));
                    GUI.Label(args.GetCellRect(1), life.count.ToString());
                    GUI.Label(args.GetCellRect(2), life.time.ToString());

                }
                else
                {
                    string name = args.item.displayName;
                    AssetLife<Asset> life = AssetsEditorTool.assets[name];
                    if (life.asset.bundle != null)
                    {
                        EditorGUI.SelectableLabel(args.GetCellRect(4), Path.GetFileName(life.asset.bundle.path));
                    }
                    GUI.Label(args.GetCellRect(0), new GUIContent(name, args.item.icon));
                    GUI.Label(args.GetCellRect(1), life.tag);
                    GUI.Label(args.GetCellRect(2), life.count.ToString());
                    GUI.Label(args.GetCellRect(3), life.time.ToString());
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
