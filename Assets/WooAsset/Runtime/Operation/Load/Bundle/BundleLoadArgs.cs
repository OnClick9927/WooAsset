namespace WooAsset
{
    public struct BundleLoadArgs : IAssetArgs
    {
        public string bundleName;
        public bool async;
        public IAssetStreamEncrypt encrypt;
        public BundleLoadArgs(string bundleName, bool async, IAssetStreamEncrypt en)
        {
            this.bundleName = bundleName;
            this.async = async;
            this.encrypt = en;
        }

        public string uid => bundleName;
    }

}
