namespace WooAsset
{
    interface IAssetArgs
    {
        string uid { get; }
    }
    struct BundleLoadArgs : IAssetArgs
    {
        public bool async;
        public IAssetEncrypt encrypt;
        public BundleData data;
        public Operation dependence;
        public string bundleName => data.bundleName;
        public string uid => data.bundleName;

        public BundleLoadArgs(BundleData data, bool async, IAssetEncrypt en)
        {
            this.data = data;
            this.async = async;
            this.encrypt = en;
            this.dependence = null;
        }

    }
    struct AssetLoadArgs : IAssetArgs
    {
        public bool async;
        public System.Type type;
        public AssetData data;
        public bool sub;
        public string path;
        public string uid => data.path;

        private AssetLoadArgs(string path, AssetData data, bool async, System.Type type, bool sub)
        {
            this.data = data;
            this.async = async;
            this.type = type;
            this.sub = sub;
            this.path = path;
        }

        public static AssetLoadArgs NormalArg(AssetData data, bool async, System.Type type, bool sub)
        {
            return new AssetLoadArgs(data.path, data, async, type, sub);
        }
        public static AssetLoadArgs CustomArg(string path, bool async, System.Type type)
        {
            return new AssetLoadArgs(path, null, async, type, false);
        }

    }

}
