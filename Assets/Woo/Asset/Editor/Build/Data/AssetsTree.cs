using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace WooAsset
{
    [System.Serializable]
    public class AssetsTree
    {
        [SerializeField] private List<AssetInfo> rootDir = new List<AssetInfo>();
        [SerializeField] private List<AssetInfo> assets = new List<AssetInfo>();
        [SerializeField] private List<AssetInfo> singles = new List<AssetInfo>();

        public List<AssetInfo> GetAssets()
        {
            return assets;
        }
        public List<AssetInfo> GetRootDirPaths()
        {
            return rootDir;
        }
        public List<AssetInfo> GetSingleFiles()
        {
            return singles;
        }
        public List<AssetInfo> GetSubFolders(AssetInfo info)
        {
            string path = info.path;
            return assets.FindAll(x => x.parentPath == path && x.type == AssetInfo.AssetType.Directory);
        }
        public List<AssetInfo> GetSubFiles(AssetInfo info)
        {
            string path = info.path;
            return assets.FindAll(x => x.parentPath == path && x.type != AssetInfo.AssetType.Directory);
        }
        public AssetInfo GetAssetInfo(string path)
        {
            AssetInfo a = rootDir.Find(x => x.path == path);
            if (a == null) a = assets.Find(x => x.path == path);
            if (a == null) a = singles.Find(x => x.path == path);
            return a;
        }



        public void AddPath(string path)
        {
            path = AssetsInternal.ToRegularPath(path);
            if (assets.Find(a => a.path == path) != null) return;
            if (path.IsDirectory()) LoopAdd(path, "");
            else
            {
                var find = singles.Find(x => x.path == path);
                if (find != null || AssetsBuild.IsIgnorePath(path)) return;
                singles.Add(new AssetInfo(path, Path.GetDirectoryName(path).ToAssetsPath()));
            }
            for (int i = singles.Count - 1; i >= 0; i--)
            {
                var single = singles[i];
                var dir = AssetsInternal.ToRegularPath(Path.GetDirectoryName(single.path));
                if (assets.Find(s => s.path == single.path) != null)
                {
                    singles.RemoveAt(i);
                    continue;
                }
                if (assets.Find(s => dir == s.path) != null)
                {
                    assets.Add(new AssetInfo(path, dir));
                    singles.RemoveAt(i);
                    continue;
                }

            }

        }
        private void LoopAdd(string path, string parent)
        {
            AssetInfo info = new AssetInfo(path, parent);

            if (string.IsNullOrEmpty(parent))
                rootDir.Add(info);
            assets.Add(info);
            string[] dirs = AssetsBuild.GetLegalDirectories(path);
            foreach (var item in dirs)
            {
                LoopAdd(AssetsInternal.ToRegularPath(item), path);
            }
            string[] files = AssetsBuild.GetLegalFiles(path);
            foreach (var item in files)
            {
                AddPath(item);
            }
        }
        public void CollectDps()
        {
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (!asset.path.IsDirectory())
                {
                    string[] paths = AssetDatabase.GetDependencies(asset.path, true);

                    for (int j = 0; j < paths.Length; j++)
                    {
                        var path = AssetsInternal.ToRegularPath(paths[j]);
                        if (path != asset.path)
                        {
                            path = AssetsInternal.ToRegularPath(path);
                            if (!AssetsBuild.IsIgnorePath(path))
                            {
                                if (!asset.dps.Contains(path) && !path.IsDirectory())
                                    asset.dps.Add(path);
                                AddPath(AssetsInternal.ToRegularPath(path));
                            }
                        }
                    }

                }
            }
        }
        public void RemoveUselessInfos()
        {
            for (int i = 0; i < assets.Count; i++)
            {
                AssetInfo info = assets[i];
                if (NeedRemove(info))
                {
                    assets.RemoveAt(i);
                    RemoveUselessInfos();
                    break;
                }
            }
        }
        private int GetFileCount(AssetInfo info)
        {
            int sum = 0;
            sum += this.GetSubFiles(info).Count;
            var fs = this.GetSubFolders(info);
            foreach (var item in fs)
            {
                sum += GetFileCount(item);
            }
            return sum;
        }
        private bool NeedRemove(AssetInfo info)
        {
            if (info.type != AssetInfo.AssetType.Directory) return false;
            var count = GetFileCount(info);
            return count == 0;
        }

        public List<string> GetDps(string path)
        {
            var asset = assets.Find(x => x.path == path);
            if (asset != null) return asset.dps;
            return null;
        }
        public void Clear()
        {
            rootDir.Clear();
            assets.Clear();
            singles.Clear();
        }
        public Dictionary<AssetInfo, List<AssetInfo>> GetDpDic()
        {
            Dictionary<AssetInfo, List<AssetInfo>> dic = new Dictionary<AssetInfo, List<AssetInfo>>();
            foreach (var asset in assets)
            {
                if (asset.type == AssetInfo.AssetType.Directory) continue;
                var list = assets.FindAll(a => { return a.dps.Contains(asset.path); });
                dic.Add(asset, list);
            }
            return dic;
        }
    }
}
