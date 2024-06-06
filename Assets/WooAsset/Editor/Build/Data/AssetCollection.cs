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
        private IAssetBuild assetBuild;
        public List<EditorAssetData> GetNoneParent() => assets.FindAll(x => GetAssetData(x.directory) == null);
        public EditorAssetData GetAssetData(string path) => assets.Find(x => x.path == path);
        public List<EditorAssetData> GetAllAssets() => assets;
        public List<EditorAssetData> GetSubFolders(EditorAssetData data) => assets.FindAll(x => x.directory == data.path && x.type == AssetType.Directory);
        public List<EditorAssetData> GetSubFiles(EditorAssetData data) => assets.FindAll(x => x.directory == data.path && x.type != AssetType.Directory);

        public void ReadPaths(List<string> folders, IAssetBuild assetBuild)
        {
            this.assetBuild = assetBuild;
            assets.Clear();
            folders.RemoveAll(x => !AssetsEditorTool.ExistsDirectory(x) || CheckRawIsIgnorePath(x));
            for (int i = 0; i < folders.Count; i++)
                AddPath(folders[i]);
            CollectDps();
            List<string> distinctPaths = new List<string>();
            for (int i = assets.Count - 1; i >= 0; i--)
            {
                var asset = assets[i];
                string path = asset.path;
                if (distinctPaths.Contains(path))
                    assets.RemoveAt(i);
                else
                    distinctPaths.Add(path);
            }
            assets.RemoveAll(x => NeedRemove(x));
            CalcLength();
            CalcUsage();
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

        private void HandleLoopDependence(List<EditorAssetData> err) => assetBuild.HandleLoopDependence(err);
        private AssetType GetAssetType(string path) => assetBuild.GetAssetType(path);
        private bool CheckRawIsIgnorePath(string path)
        {
            path = AssetsHelper.ToRegularPath(path);
            var type = GetAssetType(path);
            return type == AssetType.Ignore;
        }
        private long GetLength(EditorAssetData data)
        {
            if (data.type != AssetType.Directory) return data.length;
            long sum = 0;
            foreach (var item in this.GetSubFiles(data))
                sum += GetLength(item);
            foreach (var item in this.GetSubFolders(data))
                sum += GetLength(item);
            return sum;
        }
        private bool NeedRemove(EditorAssetData data)
        {
            if (data.type != AssetType.Directory) return false;
            if (this.GetSubFiles(data).Count != 0) return false;
            var fs = this.GetSubFolders(data);
            foreach (var item in fs)
            {
                if (!NeedRemove(item))
                {
                    return false;
                }
            }
            return true;
        }
        private void CalcLength()
        {
            var _assets = assets;
            for (int i = 0; i < _assets.Count; i++)
            {
                var asset = _assets[i];
                if (asset.type != AssetType.Directory) continue;
                asset.length = GetLength(asset);
            }
        }
        private void CalcUsage()
        {
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset.type == AssetType.Directory) continue;
                asset.usage = assets.FindAll(x => x.dependence.Contains(asset.path)).Select(x => x.path).ToList();
            }
        }

        private void AddPath(string directory)
        {
            string path = AssetsHelper.ToRegularPath(directory);
            var root = EditorAssetData.Create(path, GetAssetType(path));
            assets.Add(root);

            List<string> list = new List<string>(AssetsEditorTool.GetDirectoryDirectories(directory));
            list.AddRange(AssetsHelper.GetDirectoryFiles(directory));
            list.RemoveAll(x => CheckRawIsIgnorePath(x));
            foreach (var item in list)
            {
                string _path = AssetsHelper.ToRegularPath(item);
                assets.Add(EditorAssetData.Create(_path, GetAssetType(_path)));
            }
        }

        private void CollectDps()
        {
            var paths = AssetDatabase.GetDependencies(assets.FindAll(x => x.type != AssetType.Directory)
                .ConvertAll(x => x.path).ToArray(), true);
            for (int i = 0; i < paths.Length; i++)
            {
                var path = AssetsHelper.ToRegularPath(paths[i]);
                if (CheckRawIsIgnorePath(path)) continue;
                if (assets.Find(x => x.path == path) != null) continue;
                assets.Add(EditorAssetData.Create(path, GetAssetType(path)));

            }
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset.type == AssetType.Directory) continue;
                var result = AssetDatabase.GetDependencies(asset.path, true)
                    .ToList()
                    .ConvertAll(x => AssetsHelper.ToRegularPath(x))
                    .Where(x => x != asset.path && !CheckRawIsIgnorePath(x) && !AssetsEditorTool.IsDirectory(x));
                asset.dependence = result.ToList();
            }
            List<EditorAssetData> errs = new List<EditorAssetData>();
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                var dps = asset.dependence.ConvertAll(x => GetAssetData(x));
                var find = dps.FindAll(x => x.dependence.Contains(asset.path));
                if (find != null && find.Count != 0)
                    errs.Add(asset);
            }
            HandleLoopDependence(errs);
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                var dps = asset.dependence.ConvertAll(x => GetAssetData(x));
                var find = dps.FindAll(x => x.dependence.Contains(asset.path));
                if (find != null && find.Count != 0)
                    asset.loopDependence = true;

            }
        }

    
    }
}
