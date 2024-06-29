using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace WooAsset
{
    [System.Serializable]
    public class AssetCollection
    {
        [SerializeField] private List<EditorAssetData> assets = new List<EditorAssetData>();
        private IAssetsBuild assetBuild;
        public List<EditorAssetData> GetNoneParent() => assets.FindAll(x => GetAssetData(x.directory) == null);
        public EditorAssetData GetAssetData(string path) => assets.Find(x => x.path == path);
        public List<EditorAssetData> GetAllAssets() => assets;
        public List<EditorAssetData> GetSubFolders(EditorAssetData data) => assets.FindAll(x => x.directory == data.path && x.type == AssetType.Directory);
        public List<EditorAssetData> GetSubFiles(EditorAssetData data) => assets.FindAll(x => x.directory == data.path && x.type != AssetType.Directory);
        private List<EditorAssetData> GetSubDatas(EditorAssetData data) => assets.FindAll(x => x.directory == data.path);

        public void ReadPaths(List<string> folders, IAssetsBuild assetBuild)
        {
            this.assetBuild = assetBuild;
            Dictionary<string, EditorAssetData> asset_Map = new Dictionary<string, EditorAssetData>();

            folders.RemoveAll(x => !AssetsEditorTool.ExistsDirectory(x));
            for (int i = 0; i < folders.Count; i++)
                AddPath(folders[i], asset_Map);
            CollectDps(asset_Map);

            var _assets = asset_Map.Values.ToList();
            this.assets = _assets;

            for (int i = _assets.Count - 1; i >= 0; i--)
            {
                var _asset = _assets[i];
                var need = NeedRemove(_asset);
                if (need)
                {
                    _assets.RemoveAt(i);
                    asset_Map.Remove(_asset.path);
                }
            }
            _assets.RemoveAll(x => NeedRemove(x));

            for (int i = 0; i < _assets.Count; i++)
                _assets[i].dependence.RemoveAll(x => !asset_Map.ContainsKey(x));


            for (int i = 0; i < _assets.Count; i++)
            {
                var asset = _assets[i];
                if (asset.type == AssetType.Directory)
                    asset.length = GetLength(asset);
                else
                    asset.usage = _assets.FindAll(x => x.dependence.Contains(asset.path)).Select(x => x.path).ToList();

            }

        }


        public void ReadAssetTags(Dictionary<string, List<string>> tag_dic)
        {
            foreach (var item in tag_dic)
            {
                GetAssetData(item.Key).tags = item.Value;
            }
        }
        public void ReadRecord(Dictionary<string, bool> record_dic)
        {
            foreach (var item in record_dic)
            {
                GetAssetData(item.Key).record = item.Value;
            }
        }

        private long GetLength(EditorAssetData data) => data.type == AssetType.Directory ? this.GetSubDatas(data).Sum(x => GetLength(x)) : data.length;
        private bool NeedRemove(EditorAssetData data)
        {
            if (data.type == AssetType.Ignore) return true;
            if (data.type != AssetType.Directory) return false;
            var fs = this.GetSubDatas(data);
            return !fs.Any(x => !NeedRemove(x));
        }

        private void AddPath(string directory, Dictionary<string, EditorAssetData> assetMap)
        {
            string path = AssetsHelper.ToRegularPath(directory);
            AddToAssets(path, assetMap);
            var list = AssetsEditorTool.GetDirectoryEntries(directory);
            foreach (var item in list) AddToAssets(item, assetMap);
        }
        private void AddToAssets(string path, Dictionary<string, EditorAssetData> assetMap)
        {
            if (!assetMap.ContainsKey(path))
            {
                var type = assetBuild.GetAssetType(path);
                if (type != AssetType.Ignore)
                {
                    assetMap.Add(path, EditorAssetData.Create(path, type));
                }
            }
        }
        private void CollectDps(Dictionary<string, EditorAssetData> assetMap)
        {

            //var paths = AssetDatabase.GetDependencies(assetMap.Values
            //    .Where(x => x.type != AssetType.Ignore && x.type != AssetType.Directory)
            //    .Select(x => x.path).ToArray(), true).Where(x => !assetMap.ContainsKey(x)).ToArray();
            Dictionary<string, string[]> dpMap = new Dictionary<string, string[]>();
            foreach (var asset in assetMap.Values.ToList())
            {
                if (asset.type == AssetType.Directory || asset.type == AssetType.Ignore) continue;
                var dps = AssetDatabase.GetDependencies(asset.path, true);
                dpMap[asset.path] = dps;
                foreach (var item in dps)
                    AddToAssets(item, assetMap);
            }
            //for (int i = 0; i < paths.Length; i++)
            //{

            //    AddToAssets(paths[i], assetMap);

            //}
            foreach (var asset in assetMap.Values)
            {
                if (asset.type == AssetType.Directory || asset.type == AssetType.Ignore) continue;
                if (!dpMap.ContainsKey(asset.path))
                {
                    var dps = AssetDatabase.GetDependencies(asset.path, true);
                    dpMap[asset.path] = dps;
                }
                //.Select(x => AssetsHelper.ToRegularPath(x))
                asset.dependence = dpMap[asset.path].Where(x => assetMap.ContainsKey(x) && x != asset.path).ToList();
            }

            //var paths = AssetDatabase.GetDependencies(assetMap.Values.Where(x => x.type != AssetType.Directory && x.type != AssetType.Ignore)
            //    .Select(x => x.path).ToArray(), true);
            //for (int i = 0; i < paths.Length; i++)
            //{
            //    var path = AssetsHelper.ToRegularPath(paths[i]);
            //    AddToAssets(path, assetMap);

            //}
            //for (int i = 0; i < assets.Count; i++)
            //{
            //    var asset = assets[i];
            //    if (asset.type == AssetType.Directory) continue;
            //    var result = AssetDatabase.GetDependencies(asset.path, true)
            //        .ToList()
            //        .ConvertAll(x => AssetsHelper.ToRegularPath(x))
            //        .Where(x => x != asset.path && !AssetsEditorTool.IsDirectory(x));
            //    asset.dependence = result.ToList();
            //}

        }


    }
}
