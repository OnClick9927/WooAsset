namespace WooAsset
{
    public struct BundleLoadArgs : IAssetArgs
    {
        public string bundleName;
        public bool async;
        public BundleLoadArgs(string bundleName, bool async)
        {
            this.bundleName = bundleName;
            this.async = async;
        }
    }
}
