using System;
using System.Collections.Generic;
using static WooAsset.ManifestData;
namespace WooAsset
{
    public struct AssetLoadArgs : IAssetArgs
    {
        public string bundleName => data.bundleName;
        public string path => data.path;
        public bool direct;
        public bool scene => data.type == AssetType.Scene;
        public List<AssetHandle> dps;
        public bool async;
        public Type type;
        public AssetData data;
        public string uid => path;

        public AssetLoadArgs(AssetData data, bool direct, List<AssetHandle> dps, bool async, Type type)
        {
            this.data = data;
            this.dps = dps;
            this.direct = direct;
            this.async = async;
            this.type = type;
        }
    }

}
