using System;
using System.Collections.Generic;
using static WooAsset.ManifestData;
namespace WooAsset
{
    public struct AssetLoadArgs : IAssetArgs
    {
        public bool async;
        public Type type;
        public AssetData data;
        public string uid => data.path;

        private AssetLoadArgs(AssetData data, bool async, Type type)
        {
            this.data = data;
            this.async = async;
            this.type = type;
        }

        public static AssetLoadArgs NormalArg(AssetData data, bool async, Type type)
        {
            return new AssetLoadArgs(data, async, type);
        }


    }

}
