namespace WooAsset
{
    public struct SceneAssetLoadArgs : IAssetArgs
    {
        public string path;

        public SceneAssetLoadArgs(string path)
        {
            this.path = path;
        }
    }
}
