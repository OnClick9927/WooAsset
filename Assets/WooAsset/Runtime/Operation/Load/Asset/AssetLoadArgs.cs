using System;
using System.Collections.Generic;
using static WooAsset.ManifestData;
namespace WooAsset
{
    public struct AssetLoadArgs : IAssetArgs
    {
        public bool direct;
        public bool async;
        public Type type;
        public AssetData data;
        public string uid => data.path;
        public bool isRes;

        private AssetLoadArgs(AssetData data, bool async, Type type, bool direct, bool isRes)
        {
            this.data = data;
            this.direct = direct;
            this.async = async;
            this.type = type;
            this.isRes = isRes;
        }

        public static AssetLoadArgs NormalArg(AssetData data, bool async, Type type)
        {
            return new AssetLoadArgs(data, async, type, false, false);
        }
        public static AssetLoadArgs FileArg(AssetData data, bool async)
        {
            return new AssetLoadArgs(data, async, null, true, false);
        }
        public static AssetLoadArgs ResArg(AssetData data, bool async)
        {
            return new AssetLoadArgs(data, async, null, false, true);

        }

    }

}
