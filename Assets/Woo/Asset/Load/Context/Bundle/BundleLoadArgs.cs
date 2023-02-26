namespace WooAsset
{
    public struct BundleLoadArgs : IAssetArgs
    {
        public BundleLoadType type;
        public string bundleName;
        public string path;

        public BundleLoadArgs(BundleLoadType type, string path,  string bundleName)
        {
            this.type = type;
            this.path = path;
            this.bundleName = bundleName;
        }
    }
}
