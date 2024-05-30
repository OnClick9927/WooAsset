using System;
namespace WooAsset
{
    public struct AssetLoadArgs : IAssetArgs
    {
        public bool async;
        public Type type;
        public AssetData data;
        public bool sub;
        public string uid => data.path;

        private AssetLoadArgs(AssetData data, bool async, Type type, bool sub)
        {
            this.data = data;
            this.async = async;
            this.type = type;
            this.sub = sub;
        }

        public static AssetLoadArgs NormalArg(AssetData data, bool async, Type type, bool sub)
        {
            return new AssetLoadArgs(data, async, type, sub);
        }


    }

}
