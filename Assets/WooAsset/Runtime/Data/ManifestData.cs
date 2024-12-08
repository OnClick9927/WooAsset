

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.Runtime.ConstrainedExecution;
using System.Linq;
using static UnityEngine.Networking.UnityWebRequest;


namespace WooAsset
{
    [Serializable]
    public class ManifestData : IBufferObject
    {
        public class RTName
        {
            public string name;
            public List<string> assets;

            public void AddAsset(string path)
            {
                if (assets == null) assets = new List<string>();
                if (!assets.Contains(path))
                    assets.Add(path);
            }
        }

        [System.Serializable]
        private class ForSer : IBufferObject
        {
            [System.Serializable]

            public class AssetDataForSer : IBufferObject
            {
                public int bundleNameIndex;
                public AssetType type;
                public int[] path_segs;
                public int[] dot_segs;
                public string guid;
                void IBufferObject.ReadData(BufferReader reader)
                {
                    bundleNameIndex = reader.ReadInt32();
                    type = (AssetType)reader.ReadUInt16();
                    path_segs = reader.ReadInt32Array();
                    dot_segs = reader.ReadInt32Array();
                    guid = reader.ReadUTF8();
                }

                void IBufferObject.WriteData(BufferWriter writer)
                {
                    writer.WriteInt32(bundleNameIndex);
                    writer.WriteUInt16((ushort)type);
                    writer.WriteInt32Array(path_segs);
                    writer.WriteInt32Array(dot_segs);
                    writer.WriteUTF8(guid);
                }
            }

            [System.Serializable]

            public class BundleDataForSer : IBufferObject
            {
                public List<int> bundleDependence;
                public string bundleName;
                public bool raw;
                public CompressType compress;
                public int enCode;
                public string hash;
                public long length;
                public string bundleHash;
                public uint bundleCrc;
                void IBufferObject.ReadData(BufferReader reader)
                {
                    raw = reader.ReadBool();
                    enCode = reader.ReadInt32();
                    length = reader.ReadInt64();

                    bundleDependence = reader.ReadInt32List();
                    bundleName = reader.ReadUTF8();
                    hash = reader.ReadUTF8();
                    compress = (CompressType)reader.ReadByte();
                    bundleHash = reader.ReadUTF8();
                    bundleCrc = reader.ReadUInt32();
                }

                void IBufferObject.WriteData(BufferWriter writer)
                {
                    writer.WriteBool(raw);
                    writer.WriteInt32(enCode);
                    writer.WriteInt64(length);

                    writer.WriteInt32List(bundleDependence);
                    writer.WriteUTF8(bundleName);
                    writer.WriteUTF8(hash);
                    writer.WriteByte((byte)compress);
                    writer.WriteUTF8(bundleHash);
                    writer.WriteUInt32(bundleCrc);
                }
            }

            [System.Serializable]
            public class TagDataForSer : IBufferObject
            {
                public string tag;
                public List<int> assetIndexes;

                void IBufferObject.ReadData(BufferReader reader)
                {
                    tag = reader.ReadUTF8();
                    assetIndexes = reader.ReadInt32List();
                }

                void IBufferObject.WriteData(BufferWriter writer)
                {
                    writer.WriteUTF8(tag);
                    writer.WriteInt32List(assetIndexes);
                }
            }

            public List<string> segs = new List<string>();
            public List<TagDataForSer> Tags;
            public List<BundleDataForSer> Bundles;
            public List<AssetDataForSer> Assets;
            private int GetSegIndex(string seg)
            {
                var index = this.segs.IndexOf(seg);
                if (index != -1)
                    return index;
                else
                {
                    this.segs.Add(seg);
                    return this.segs.Count - 1;
                }
            }

            public void Set(List<AssetData> assets, List<BundleData> bundles)
            {
                Assets = new List<AssetDataForSer>();
                Tags = GetTags(assets, bundles).ConvertAll(x => new TagDataForSer()
                {
                    tag = x.tag,
                    assetIndexes = x.assets.ConvertAll(assetPath => assets.FindIndex(data => data.path == assetPath)),
                });

                Bundles = bundles.ConvertAll(x => new BundleDataForSer()
                {
                    bundleName = x.bundleName,
                    bundleCrc = x.bundleCrc,
                    bundleHash = x.bundleHash,
                    compress = x.compress,
                    enCode = x.enCode,
                    hash = x.hash,
                    length = x.length,
                    raw = x.raw,
                    bundleDependence = x.dependence.ConvertAll(bundlName => bundles.FindIndex(y => y.bundleName == bundlName)),
                });

                for (int index = 0; index < assets.Count; index++)
                {
                    var asset = assets[index];

                    AssetDataForSer asset_ser = new AssetDataForSer()
                    {

                        bundleNameIndex = bundles.FindIndex(x => x.bundleName == asset.bundleName),
                        guid = asset.guid,
                        type = asset.type,

                    };

                    var path = asset.path;
                    var sps = path.Split('/');
                    int[] path_segs = new int[sps.Length - 1];
                    int[] dot_segs = null;

                    for (int seg_index = 0; seg_index < sps.Length; seg_index++)
                    {
                        string seg = sps[seg_index];
                        if (seg_index == sps.Length - 1)
                        {
                            var dot_sps = seg.Split('.');
                            dot_segs = new int[dot_sps.Length];
                            for (int k = 0; k < dot_sps.Length; k++)
                            {
                                var dot_sp = dot_sps[k];
                                dot_segs[k] = GetSegIndex(dot_sp);
                            }
                        }
                        else
                        {
                            path_segs[seg_index] = GetSegIndex(seg);
                        }
                    }
                    asset_ser.path_segs = path_segs;
                    asset_ser.dot_segs = dot_segs;
                    Assets.Add(asset_ser);
                }

            }

            public void Get(out List<AssetData> assets, out List<BundleData> bundles)
            {
                assets = new List<AssetData>();
                bundles = Bundles.ConvertAll(x => new BundleData()
                {
                    bundleCrc = x.bundleCrc,
                    bundleHash = x.bundleHash,
                    bundleName = x.bundleName,
                    compress = x.compress,
                    enCode = x.enCode,
                    length = x.length,
                    hash = x.hash,
                    raw = x.raw,
                    dependence = x.bundleDependence.ConvertAll(index => Bundles[index].bundleName),
                });
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < Assets.Count; i++)
                {
                    var asset = Assets[i];
                    var bundle = bundles[asset.bundleNameIndex];
                    AssetData assetData = new AssetData()
                    {
                        bundleName = bundle.bundleName,
                        guid = asset.guid,
                        type = asset.type,
                    };
                    var path_segs = asset.path_segs;
                    var dot_segs = asset.dot_segs;

                    for (int j = 0; j < path_segs.Length; j++)
                    {
                        sb.Append(this.segs[path_segs[j]]);
                        sb.Append('/');
                    }
                    for (int j = 0; j < dot_segs.Length; j++)
                    {
                        sb.Append(this.segs[dot_segs[j]]);
                        if (j != dot_segs.Length - 1)
                            sb.Append('.');
                    }
                    assetData.path = sb.ToString();
                    sb.Clear();
                    bundle.AddAsset(assetData.path);
                    assets.Add(assetData);
                }


                for (int i = 0; i < Tags.Count; i++)
                {
                    var tag = Tags[i];

                    if (tag.assetIndexes != null)
                    {
                        foreach (var assetIndex in tag.assetIndexes)
                        {
                            var asset = assets[assetIndex];
                            asset.AddTag(tag.tag);
                        }
                    }
                }



            }
            public void ReadData(BufferReader reader)
            {
                Tags = reader.ReadObjectList<TagDataForSer>();
                Assets = reader.ReadObjectList<AssetDataForSer>();
                Bundles = reader.ReadObjectList<BundleDataForSer>();
                segs = reader.ReadUTF8List();
            }

            public void WriteData(BufferWriter writer)
            {
                writer.WriteObjectList(Tags);
                writer.WriteObjectList(Assets);
                writer.WriteObjectList(Bundles);
                writer.WriteUTF8List(segs);
            }
        }
        static List<TagData> GetTags(List<AssetData> assets, List<BundleData> bundles)
        {
            var result = assets.Where(x => x.tags != null && x.tags.Count > 0).Select(x => new { x.path, x.tags });
            var alltags = result.SelectMany(x => x.tags).Distinct();
            var tags = alltags.Select(tag =>
            {
                var _assets = result.Where(x => x.tags.Contains(tag)).Select(x => x.path);
                return new TagData()
                {
                    tag = tag,
                    assets = _assets.ToList()
                };
            }).ToList();
            return tags;
        }
        public void Read(string version, List<AssetData> assets, List<BundleData> bundles)
        {
#if UNITY_EDITOR
            this.assets = assets;
            this.bundles = bundles;
            this.version = version;
            ser.Set(this.assets, this.bundles);
#endif
        }
        [UnityEngine.SerializeField] private string version;

        //private List<TagData> tags = new List<TagData>();
        [UnityEngine.SerializeField] private List<AssetData> assets = new List<AssetData>();
        [UnityEngine.SerializeField] private List<BundleData> bundles = new List<BundleData>();
        private ForSer ser = new ForSer();



        void IBufferObject.ReadData(BufferReader reader)
        {
            version = reader.ReadUTF8();
            ser = reader.ReadObject<ForSer>();
            ser.Get(out this.assets, out this.bundles);
        }
        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteUTF8(version);
            //ser = new ForSer();
            ser.Set(this.assets, this.bundles);
            writer.WriteObject(ser);
        }


        public void Prepare(bool fuzzySearch, FileNameSearchType fileNameSearchType)
        {
            var tags = GetTags(this.assets, this.bundles);
            _tags = tags.ToDictionary(x => x.tag);
            _bundles = bundles.ToDictionary(x => x.bundleName);
            _assets = new Dictionary<string, AssetData>();
            _names = new Dictionary<string, RTName>();
            _fuzzleAssets = new Dictionary<string, string>();
            _guidAssets = new Dictionary<string, string>();
            for (int i = 0; i < assets.Count; i++)
            {
                AssetData asset = assets[i];
                string path = asset.path;
                string assetName = string.Empty;
                if (fileNameSearchType == FileNameSearchType.FileName)
                    assetName = AssetsHelper.GetFileName(path);
                else if (fileNameSearchType == FileNameSearchType.FileNameWithoutExtension)
                    assetName = AssetsHelper.GetFileNameWithoutExtension(path);
                _guidAssets.Add(asset.guid, asset.path);
                if (fuzzySearch)
                {
                    string assetName_noEx = AssetsHelper.GetFileNameWithoutExtension(path);
                    string dir = AssetsHelper.GetDirectoryName(path);
                    var key = AssetsHelper.ToRegularPath(AssetsHelper.CombinePath(dir, assetName_noEx));
                    if (_fuzzleAssets.ContainsKey(key))
                        AssetsHelper.LogError($"fuzzy search:  same name asset in directory : {dir}  name {assetName_noEx} ");
                    else
                        _fuzzleAssets.Add(key, path);
                }

                string bundleName = asset.bundleName;
                _bundles[bundleName].AddAsset(path);
                _assets.Add(path, asset);
                AssetsHelper.GetFromDictionary(_names, assetName).AddAsset(path);
            }
            allPaths = AssetsHelper.ToKeyList(_assets);
            allTags = AssetsHelper.ToKeyList(_tags);
            allBundle = AssetsHelper.ToKeyList(_bundles);
            //allName = AssetsHelper.ToKeyList(_names);
        }

        private Dictionary<string, string> _guidAssets;
        private Dictionary<string, string> _fuzzleAssets;
        private Dictionary<string, AssetData> _assets;
        private Dictionary<string, RTName> _names;
        private Dictionary<string, TagData> _tags;
        private Dictionary<string, BundleData> _bundles;

        [NonSerialized] public List<string> allPaths;
        [NonSerialized] public List<string> allTags;
        [NonSerialized] public List<string> allBundle;
        //[NonSerialized] public List<string> allName;
        public string GetVersion() => version;

        public AssetData GetAssetData(string assetPath) => AssetsHelper.GetOrDefaultFromDictionary(_assets, assetPath);
        public List<string> GetTagAssetPaths(string tag) => AssetsHelper.GetOrDefaultFromDictionary(_tags, tag)?.assets;
        public IReadOnlyList<string> GetAssets(string bundleName) => AssetsHelper.GetOrDefaultFromDictionary(_bundles, bundleName)?.assets;
        public BundleData GetBundleData(string bundleName) => AssetsHelper.GetOrDefaultFromDictionary(_bundles, bundleName);
        public List<BundleData> GetAllBundleData() => bundles;

        public IReadOnlyList<string> GetAssetsByAssetName(string name)
        {
            if (_names.TryGetValue(name, out RTName _name))
                return _name?.assets;
            return null;
        }
        public string GUIDToAssetPath(string guid) => AssetsHelper.GetOrDefaultFromDictionary(_guidAssets, guid);
        public AssetData GetFuzzyAssetData(string path)
        {
            string target = string.Empty;
            if (_fuzzleAssets.TryGetValue(path, out target))
                return GetAssetData(target);
            return null;
        }

        public static void Merge(ManifestData src, ManifestData dest, List<string> prefer_bundles)
        {
            dest.version = src.version;
            var assets_src = src.assets;
            var bundles_src = src.bundles;
            var assets_dest = dest.assets;
            var bundles_dest = dest.bundles;
            for (int src_index = 0; src_index < bundles_src.Count; src_index++)
            {
                var bundle = bundles_src[src_index];
                var bundleName = bundle.bundleName;

                if (bundles_dest.Find(x => x.bundleName == bundleName) == null)
                    bundles_dest.Add(bundle);
            }
            for (int src_index = 0; src_index < assets_src.Count; src_index++)
            {
                var asset = assets_src[src_index];
                var tar_index = assets_dest.FindIndex(x => x.path == asset.path);

                if (tar_index != -1)
                {
                    var origin = assets_dest[tar_index];
                    var org_bundle_exist = prefer_bundles != null && prefer_bundles.Contains(origin.bundleName);
                    var cur_bundle_exist = prefer_bundles != null && prefer_bundles.Contains(asset.bundleName);
                    if (!org_bundle_exist && cur_bundle_exist)
                        assets_dest[tar_index] = asset;
                }
                else
                    assets_dest.Add(asset);
            }




        }


    }
}
