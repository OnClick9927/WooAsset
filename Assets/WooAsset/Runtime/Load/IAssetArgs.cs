namespace WooAsset
{
    interface IAssetArgs
    {
        string uid { get;}
    }
    public struct BundleLoadArgs : IAssetArgs
    {
        public bool async;
        public IAssetEncrypt encrypt;
        public BundleData data;
        public Operation dependence;
        public string bundleName => data.bundleName;
        public string uid => data.bundleName;

        public BundleLoadArgs(BundleData data, bool async, IAssetEncrypt en, Operation dependence)
        {
            this.data = data;
            this.async = async;
            this.encrypt = en;
            this.dependence = dependence;
        }

    }
    public struct AssetLoadArgs : IAssetArgs
    {
        public bool async;
        public System.Type type;
        public AssetData data;
        public bool sub;
        public string uid => data.path;

        private AssetLoadArgs(AssetData data, bool async, System.Type type, bool sub)
        {
            this.data = data;
            this.async = async;
            this.type = type;
            this.sub = sub;
        }

        public static AssetLoadArgs NormalArg(AssetData data, bool async, System.Type type, bool sub)
        {
            return new AssetLoadArgs(data, async, type, sub);
        }


    }

}
