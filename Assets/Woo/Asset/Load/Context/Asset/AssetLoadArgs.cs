namespace WooAsset
{
    public struct AssetLoadArgs : IAssetArgs
    {
        public string path;
        public AssetLoadArgs(string path)
        {
            this.path = path;
        }
    }

}
