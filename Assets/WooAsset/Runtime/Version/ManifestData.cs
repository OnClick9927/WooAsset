

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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



        public void Read(List<AssetData> assets, List<BundleData> bundles)
        {
#if UNITY_EDITOR
            this.assets = assets;
            this.bundles = bundles;
            var _tags = new Dictionary<string, TagData>();
            for (int asset_index = 0; asset_index < assets.Count; asset_index++)
            {
                var asset = assets[asset_index];
                asset.bundleNameIndex = bundles.FindIndex(x => x.bundleName == asset.bundleName);
                var tags = asset.tags;
                if (tags != null && tags.Count > 0)
                {
                    for (int j = 0; j < tags.Count; j++)
                    {
                        var tag = tags[j];
                        TagData _tag = AssetsHelper.GetFromDictionary(_tags, tag);
                        _tag.tag = tag;
                        _tag.AddAsset(asset_index);
                    }

                }

            }
            for (int i = 0; i < bundles.Count; i++)
            {
                var bundle = bundles[i];
                if (bundle.dependence != null && bundle.dependence.Count > 0)
                    bundle.bundleDependence = bundle.dependence.ConvertAll(bundleName => bundles.FindIndex(y => y.bundleName == bundleName));
            }
            this.tags = _tags.Values.ToList();
#endif
        }

        [UnityEngine.SerializeField] private List<TagData> tags = new List<TagData>();
        [UnityEngine.SerializeField] private List<AssetData> assets = new List<AssetData>();
        [UnityEngine.SerializeField] private List<BundleData> bundles = new List<BundleData>();

        private List<string> segs = new List<string>();

        void IBufferObject.ReadData(BufferReader reader)
        {
            tags = reader.ReadObjectList<TagData>();
            assets = reader.ReadObjectList<AssetData>();
            bundles = reader.ReadObjectList<BundleData>();
            segs = reader.ReadUTF8List();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
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

                asset.path = sb.ToString();
                sb.Clear();
            }


        }
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
        void IBufferObject.WriteData(BufferWriter writer)
        {
            for (int asset_index = 0; asset_index < assets.Count; asset_index++)
            {
                var asset = assets[asset_index];
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
                asset.path_segs = path_segs;
                asset.dot_segs = dot_segs;

            }



            writer.WriteObjectList(tags);
            writer.WriteObjectList(assets);
            writer.WriteObjectList(bundles);
            writer.WriteUTF8List(segs);
        }


        public void Prepare()
        {
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                asset.bundleName = bundles[asset.bundleNameIndex].bundleName;
            }
            for (int i = 0; i < bundles.Count; i++)
            {
                var bundle = bundles[i];
                var dp_index = bundle.bundleDependence;
                if (dp_index != null && dp_index.Count > 0)
                    bundle.dependence = bundle.bundleDependence.ConvertAll(index => bundles[index].bundleName);
                else
                    bundle.dependence = null;
            }
            for (int i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                var indexes = tag.assetIndexes;
                tag.assets = tag.assetIndexes.ConvertAll(x => assets[x].path);
                for (int j = 0; j < indexes.Count; j++)
                {
                    var index = indexes[j];
                    assets[index].AddTag(tag.tag);
                }
            }
            if (tags != null)
            {
                _tags = new Dictionary<string, TagData>();
                foreach (var tag in tags)
                {
                    _tags.Add(tag.tag, tag);
                }
            }
            _bundles=new Dictionary<string, BundleData>();
            foreach (var bundle in bundles)
            {
                _bundles.Add(bundle.bundleName, bundle);
            }




            _assets = new Dictionary<string, AssetData>();
            _names = new Dictionary<string, RTName>();
            for (int i = 0; i < assets.Count; i++)
            {
                AssetData asset = assets[i];
                string path = asset.path;
                string assetName = AssetsHelper.GetFileName(path);
                string bundleName = asset.bundleName;
                _bundles[bundleName].AddAsset(path);
                _assets.Add(path, asset);
                AssetsHelper.GetFromDictionary(_names, assetName).AddAsset(path);
            }
            allPaths = _assets.Keys.ToList();
            allTags = _tags.Keys.ToList();
            allBundle = _bundles.Keys.ToList();
            allName = _names.Keys.ToList();
        }
        private Dictionary<string, AssetData> _assets;
        private Dictionary<string, RTName> _names;
        private Dictionary<string, TagData> _tags;
        private Dictionary<string, BundleData> _bundles;

        [NonSerialized] public List<string> allPaths;
        [NonSerialized] public List<string> allTags;
        [NonSerialized] public List<string> allBundle;
        [NonSerialized] public List<string> allName;

        public AssetData GetAssetData(string assetpath) => AssetsHelper.GetOrDefaultFromDictionary(_assets, assetpath);
        public List<string> GetTagAssetPaths(string tag) => AssetsHelper.GetOrDefaultFromDictionary(_tags, tag)?.assets;
        public IReadOnlyList<string> GetAssets(string bundleName) => AssetsHelper.GetOrDefaultFromDictionary(_bundles, bundleName)?.assets;
        public BundleData GetBundleData(string bundleName) => AssetsHelper.GetOrDefaultFromDictionary(_bundles, bundleName);


        public IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result)
        {
            result.Clear();
            for (int i = 0; i < allName.Count; i++)
            {
                if (!name.Contains(allName[i])) continue;
                _names.TryGetValue(allName[i], out RTName _name);
                var assets = _name?.assets;
                if (assets != null)
                    result.AddRange(assets);
            }
            return result;
        }



        public static void Merge(ManifestData src, ManifestData dest, List<string> prefer_bundles)
        {
            var assets_src = src.assets;
            var bundles_src = src.bundles;
            var assets_dest = dest.assets;
            var bundles_dest = dest.bundles;
            var src_tags = src.tags;
            var dest_tags = dest.tags;

            for (int src_index = 0; src_index < bundles_src.Count; src_index++)
            {
                var bundle = bundles_src[src_index];
                var bundleName = bundle.bundleName;
                var tar_index = bundles_dest.FindIndex(x => x.bundleName == bundleName);
                var update_index = tar_index == -1 ? bundles_dest.Count : tar_index;
                for (int j = 0; j < assets_src.Count; j++)
                {
                    var asset = assets_src[j];
                    if (asset.bundleNameIndex == src_index)
                    {
                        asset.bundleNameIndex = update_index;
                        asset.bundleName = bundleName;
                    }
                }
                for (int j = 0; j < bundles_src.Count; j++)
                {
                    var _bundle = bundles_src[j];
                    var dps = _bundle.bundleDependence;
                    if (dps != null && dps.Remove(src_index))
                        dps.Add(update_index);
                }
                if (tar_index != -1) continue;
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
                    var cur_bunle_exist = prefer_bundles != null && prefer_bundles.Contains(asset.bundleName);
                    if (!org_bundle_exist && cur_bunle_exist)
                        assets_dest[tar_index] = asset;
                }
                else
                    assets_dest.Add(asset);

                var list = src_tags.FindAll(x => x.assetIndexes != null && x.assetIndexes.Contains(src_index));
                if (list != null && list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var tag = list[i];
                        tag.assetIndexes.Remove(src_index);
                        tag.assetIndexes.Add(tar_index == -1 ? assets_dest.Count - 1 : tar_index);
                    }
                }
            }


            for (int src_index = 0; src_index < src_tags.Count; src_index++)
            {
                var tag_src = src_tags[src_index];
                TagData tag_dest = dest_tags.Find(x => x.tag == tag_src.tag);
                if (tag_dest == null)
                {
                    dest_tags.Add(tag_src);
                }
                else
                {
                    if (tag_dest.assetIndexes == null)
                        tag_dest.assetIndexes = new List<int>();
                    for (int i = 0; i < tag_src.assetIndexes.Count; i++)
                    {
                        var index = tag_src.assetIndexes[i];
                        if (tag_dest.assetIndexes.Contains(index)) continue;
                        tag_dest.assetIndexes.Add(index);
                    }
                }

            }

        }


    }
}
