using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{

    [System.Serializable]
    public class EditorAssetCollection
    {
        [Serializable]
        private class EditorAssetDataKeyedList : KeyedList<string, EditorAssetData>
        {
            protected override string GetKey(EditorAssetData item) => item.path;
        }


        [SerializeField] private EditorAssetDataKeyedList assets = new EditorAssetDataKeyedList();
        private IAssetsBuild assetBuild;
        public List<EditorAssetData> GetNoneParent() => assets.FindAll(x => GetAssetData(x.directory) == null);
        public EditorAssetData GetAssetData(string path) => assets.Find(path);
        public List<EditorAssetData> GetAllAssets() => assets.GetValues();
        public List<EditorAssetData> GetSubFolders(EditorAssetData data) => assets.FindAll(x => x.directory == data.path && x.type == AssetType.Directory);
        public List<EditorAssetData> GetSubFiles(EditorAssetData data) => assets.FindAll(x => x.directory == data.path && x.type != AssetType.Directory);



        public void ReadPaths(List<string> folders, IAssetsBuild assetBuild)
        {
            this.assets.Clear();
            this.assetBuild = assetBuild;
            EditorAssetDataKeyedList asset_Map = new EditorAssetDataKeyedList();

            folders.RemoveAll(x => !AssetsEditorTool.ExistsDirectory(x));
            AddFolders(folders.ToArray(), asset_Map);
            CollectDps(asset_Map);
            Remove(asset_Map);
            Remove(asset_Map);

            var _assets = asset_Map.GetValues();
            for (int i = 0; i < _assets.Count; i++)
            {
                var asset = _assets[i];
                    asset.dependence.RemoveAll(x => !asset_Map.ContainsKey(x));
                if (asset.type == AssetType.Directory)
                    asset.length = GetLength(asset, _assets);
                else
                    asset.usage = _assets.FindAll(x => x.dependence.Contains(asset.path)).Select(x => x.path).ToList();

            }

            this.assets.SetList(_assets);

        }

        private static List<EditorAssetData> GetSubDatas(EditorAssetData data, List<EditorAssetData> assets) => assets.FindAll(x => x.directory == data.path);

        private static long GetLength(EditorAssetData data, List<EditorAssetData> assets) => data.type == AssetType.Directory ? GetSubDatas(data, assets).Sum(x => GetLength(x,assets)) : data.length;
        private static bool NeedRemove(EditorAssetData data, List<EditorAssetData> assets)
        {
            if (data.type == AssetType.Ignore) return true;
            if (data.type != AssetType.Directory) return false;
            var fs = GetSubDatas(data, assets);
            return !fs.Any(x => !NeedRemove(x, assets));
        }
        private static void Remove(EditorAssetDataKeyedList asset_Map)
        {
            List<EditorAssetData> need_remove = new List<EditorAssetData>();

            var _assets = asset_Map.GetValues();
            foreach (var _asset in _assets)
            {
                var need = NeedRemove(_asset, _assets);
                if (need)
                    need_remove.Add(_asset);
            }
            foreach (var _asset in need_remove)
                asset_Map.Remove(_asset);
        }

        private void AddFolders(string[] folders, EditorAssetDataKeyedList assetMap)
        {
            foreach (var item in folders)
            {
                string path = AssetsEditorTool.ToRegularPath(item);
                AddToAssets(path, assetMap);
            }
            var paths = AssetDatabase.FindAssets("t:object", folders).Select(x => AssetDatabase.GUIDToAssetPath(x));
            //var list = AssetsEditorTool.GetDirectoryEntries(directory);
            foreach (var item in paths) AddToAssets(item, assetMap);
        }
        private void AddToAssets(string path, EditorAssetDataKeyedList assetMap)
        {
            if (!assetMap.ContainsKey(path))
            {
                var type = assetBuild.GetAssetType(path);
                if (type != AssetType.Ignore)
                {
                    assetMap.Add(EditorAssetData.Create(path, type));
                }
            }
        }
        private void CollectDps(EditorAssetDataKeyedList assetMap)
        {
            Dictionary<string, string[]> dpMap = new Dictionary<string, string[]>();
            var values = new List<EditorAssetData>(assetMap.GetValues());
            foreach (var asset in values)
            {
                if (asset.type == AssetType.Directory || asset.type == AssetType.Ignore) continue;
                var dps = AssetsEditorTool.GetAssetDependencies(asset.path);
                dpMap[asset.path] = dps;
                foreach (var item in dps)
                    AddToAssets(item, assetMap);
            }
            values.Clear();
            values.AddRange(assetMap.GetValues());
            foreach (var asset in values)
            {
                if (asset.type == AssetType.Directory || asset.type == AssetType.Ignore) continue;
                if (!dpMap.ContainsKey(asset.path))
                {
                    var dps = AssetsEditorTool.GetAssetDependencies(asset.path);
                    dpMap[asset.path] = dps;
                }
                asset.dependence = dpMap[asset.path].Where(x => assetMap.ContainsKey(x) && x != asset.path).ToList();
            }



        }


    }
}
