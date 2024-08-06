using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using static UnityEditor.ObjectChangeEventStream;
using static WooAsset.ManifestData;

namespace WooAsset
{

    [Serializable]
    public class EditorBundleData
    {
        [UnityEngine.SerializeField] private List<EditorAssetData> assets = new List<EditorAssetData>();
        [UnityEngine.SerializeField] private List<string> usage = new List<string>();
        [UnityEngine.SerializeField] private List<string> dependence = new List<string>();
        [UnityEngine.SerializeField] private bool _loop;
        [UnityEngine.SerializeField] private string _hash;
        [UnityEngine.SerializeField] private long _length;
        [UnityEngine.SerializeField] private bool _raw;
        [UnityEngine.SerializeField] private int _enCode;



        public bool loopDependence { get => _loop; private set { _loop = value; } }
        public int usageCount => usage.Count;
        public int dependenceCount => dependence.Count;


        public string hash { get => _hash; private set { _hash = value; } }
        public long length { get => _length; private set { _length = value; } }
        public bool raw { get => _raw; private set { _raw = value; } }

        public BundleData CreateBundleData(CompressType compress)
        {
            return new BundleData()
            {
                bundleName = hash,
                dependence = dependence,
                raw = raw,
                enCode = _enCode,
                compress = compress,
                assets = assets.ConvertAll(x => x.path),
                hash = hash,
                length = length,
            };
        }

        public List<EditorBundleData> GetUsage(List<EditorBundleData> src) => src.FindAll(x => usage.Contains(x.hash));
        public List<EditorBundleData> GetDependence(List<EditorBundleData> src) => src.FindAll(x => dependence.Contains(x.hash));
        public bool CheckLoop(List<EditorBundleData> builds)
        {
            var dps = GetDependence(builds);
            var find = dps.FindAll(x => x.dependence.Contains(hash));
            loopDependence = find != null && find.Count != 0;
            return loopDependence;
        }
        public bool GetIsEmpty() => assets.Count == 0;
        public void CalculateHash(Dictionary<string, List<string>> hashMap)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in assets)
            {
                sb = sb.Append(item.hash).Append(AssetsHelper.GetStringHash(item.path));
                List<string> hashList = AssetsHelper.GetOrDefaultFromDictionary(hashMap, item.path);
                if (hashList != null)
                    for (int i = 0; i < hashList.Count; i++)
                        sb.Append(hashList[i]);

            }

            this.hash = AssetsHelper.GetStringHash(sb.ToString());
        }
        public void RemoveAsset(string assetPath)
        {
            if (!ContainsAsset(assetPath)) return;
            EditorAssetData _find = assets.Find(x => x.path == assetPath);
            assets.Remove(_find);
            length -= _find.length;

        }
        public bool ContainsAsset(string assetPath) => assets.Find(x => x.path == assetPath) != null;
        public IReadOnlyList<string> GetAssets() => assets.ConvertAll(x => x.path).ToList();
        public AssetBundleBuild ToAssetBundleBuild()
        {
            return new AssetBundleBuild()
            {
                assetBundleName = hash,
                assetNames = GetAssets().ToArray()
            };
        }
        public void SyncRealHash(string hash) => this.hash = hash;
        private void CalcLength()
        {
            length = 0;
            foreach (var item in assets)
            {
                length += item.length;
            }
        }

        public void FindDependence(List<EditorBundleData> source, List<EditorAssetData> assets)
        {
            dependence.Clear();
            var result = GetAssets()
                   .Select(assetPath => assets.Find(asset => asset.path == assetPath))
                   .SelectMany(x => x.dependence)
                   .Distinct()
                   .Select(assetPath => source.Find(y => y.ContainsAsset(assetPath)))
                   .Distinct()
                   .Where(x => x.hash != hash)
                   .Select(x => x.hash);
            dependence.AddRange(result);
        }
        public void SetDependence(List<string> dp) => this.dependence = dp;
        public void FindUsage(List<EditorBundleData> source)
        {
            usage.Clear();
            var result = source
                .FindAll(x => x.dependence.Contains(hash)).Select(x => x.hash);
            usage.AddRange(result);
        }

        public static EditorBundleData Create(List<EditorAssetData> assets)
        {
            EditorBundleData group = new EditorBundleData();
            group.assets = new List<EditorAssetData>(assets);
            group.CalcLength();
            return group;
        }
        public static EditorBundleData Create(EditorAssetData asset)
        {
            EditorBundleData group = new EditorBundleData();
            group.assets = new List<EditorAssetData>
            {
                asset
            };
            group.CalcLength();
            return group;
        }
        public static EditorBundleData CreateRaw(EditorAssetData asset)
        {
            EditorBundleData group = new EditorBundleData() { raw = true };
            group.assets = new List<EditorAssetData>
            {
                asset,
            };
            group.CalcLength();
            return group;
        }

        public void SetEncryptCode(int code) => _enCode = code;
        public int GetEncryptCode() => _enCode;

        internal void ReplaceDpendenceHash(string from, string to)
        {
            if (usage.Remove(from))
                usage.Add(to);
            if (dependence.Remove(from))
                dependence.Add(to);
        }
    }
}
