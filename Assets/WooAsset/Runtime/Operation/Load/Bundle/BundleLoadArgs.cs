namespace WooAsset
{
    public struct BundleLoadArgs : IAssetArgs
    {
        public string bundleName;
        public bool async;
        public IAssetStreamEncrypt encrypt;
        public bool raw;
        public BundleLoadArgs(string bundleName, bool async, IAssetStreamEncrypt en, bool raw)
        {
            this.bundleName = bundleName;
            this.async = async;
            this.encrypt = en;
            this.raw = raw;
        }

        public string uid => bundleName;
    }

}
