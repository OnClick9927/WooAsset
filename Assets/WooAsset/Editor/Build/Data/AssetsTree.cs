using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace WooAsset
{
    [System.Serializable]
    public class AssetsTree : IEqualityComparer<EditorAssetData>
    {

        [SerializeField] private List<string> rawAssets = new List<string>();
        [SerializeField] private List<string> rawAssets_copy = new List<string>();

        [SerializeField] private List<EditorAssetData> assets = new List<EditorAssetData>();
        public List<string> GetRawAssets() => rawAssets;
        public List<string> GetRawAssets_Copy() => rawAssets_copy;

        public AssetType GetAssetType(string path)
        {
            if (rawAssets_copy.Contains(path))
                return AssetType.RawCopyFile;
            if (rawAssets.Contains(path))
                return AssetType.Raw;
            var data= GetAssetData(path);
            return data == null ? AssetType.None : data.type;
        }
        public List<EditorAssetData> GetNoneParent() => assets.FindAll(x => GetAssetData(x.directory) == null);
        public EditorAssetData GetAssetData(string path) => assets.Find(x => x.path == path);
        public List<EditorAssetData> GetAllAssets() => assets;
        public List<EditorAssetData> GetSubFolders(EditorAssetData data) => assets.FindAll(x => x.directory == data.path && x.type == AssetType.Directory);
        public List<EditorAssetData> GetSubFiles(EditorAssetData data) => assets.FindAll(x => x.directory == data.path && x.type != AssetType.Directory);

        private bool IsIgnorePath(string path)
        {
            path = AssetsInternal.ToRegularPath(path);
            if (__GetAssetType(path) == AssetType.Raw)
                if (!rawAssets.Contains(path))
                    rawAssets.Add(path);
            if (__GetAssetType(path) == AssetType.RawCopyFile)
                if (!rawAssets_copy.Contains(path))
                    rawAssets_copy.Add(path);
            return assetBuild.IsIgnorePath(path);
        }
        private AssetType __GetAssetType(string path)
        {
            return assetBuild.GetAssetType(path);
        }
        IAssetBuild assetBuild;
        public void ReadPaths(List<string> folders, IAssetBuild assetBuild)
        {
            this.assetBuild = assetBuild;
            rawAssets.Clear();
            rawAssets_copy.Clear();
            assets.Clear();
            folders.RemoveAll(x => !Directory.Exists(x) || IsIgnorePath(x));
            for (int i = 0; i < folders.Count; i++)
                AddPath(folders[i]);
            CollectDps();
            assets = assets.Distinct(this).ToList();
            assets.RemoveAll(x => NeedRemove(x));
            CalcLength();
            CalcUsage();
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
            string path = AssetsInternal.ToRegularPath(directory);
            var root = EditorAssetData.Create(path, __GetAssetType(path));
            assets.Add(root);

            List<string> list = new List<string>(Directory.GetDirectories(directory, "*", SearchOption.AllDirectories));
            list.AddRange(Directory.GetFiles(directory, "*", SearchOption.AllDirectories));
            list.RemoveAll(x => IsIgnorePath(x));
            foreach (var item in list)
            {
                string _path = AssetsInternal.ToRegularPath(item);
                assets.Add(EditorAssetData.Create(_path, __GetAssetType(_path)));
            }
        }

        private void CollectDps()
        {
            var paths = AssetDatabase.GetDependencies(assets.FindAll(x => x.type != AssetType.Directory)
                .ConvertAll(x => x.path).ToArray(), true);
            for (int i = 0; i < paths.Length; i++)
            {
                var path = AssetsInternal.ToRegularPath(paths[i]);
                if (IsIgnorePath(path)) continue;
                if (assets.Find(x => x.path == path) != null) continue;
                assets.Add(EditorAssetData.Create(path, __GetAssetType(path)));

            }
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset.type == AssetType.Directory) continue;
                var result = AssetDatabase.GetDependencies(asset.path, true)
                    .ToList()
                    .ConvertAll(x => AssetsInternal.ToRegularPath(x))
                    .Where(x => x != asset.path && !IsIgnorePath(x) && !AssetsEditorTool.IsDirectory(x));
                asset.dependence = result.ToList();
            }
        }

        public bool Equals(EditorAssetData x, EditorAssetData y)
        {
            return x.path == y.path;
        }

        public int GetHashCode(EditorAssetData obj)
        {
            return obj.path.GetHashCode();
        }

        public bool ContainsAsset(string assetPath)
        {
            if (rawAssets_copy.Contains(assetPath))
                return true;
            if (rawAssets.Contains(assetPath))
                return true;
            return GetAssetData(assetPath) != null;
        }
    }
}
