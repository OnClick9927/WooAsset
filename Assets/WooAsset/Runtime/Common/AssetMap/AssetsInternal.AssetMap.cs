namespace WooAsset
{
    partial class AssetsInternal
    {
        private class AssetMap : NameMap<AssetHandle>
        {
            protected override AssetHandle CreateNew(IAssetArgs args) => CreateAsset((AssetLoadArgs)args);
            private AssetHandle CreateAsset(AssetLoadArgs arg)
            {
                if (arg.isRes)
                    return new ResourcesAsset(arg);
                return arg.data.type == AssetType.Scene ? new SceneAsset(arg) : new Asset(arg);
            }

            public override void Release(string path)
            {
                AssetHandle asset = Find(path);
                if (asset == null) return;
                ReleaseRef(asset);
                bundles.Release(asset.bundleName);
                TryRealUnload(path);
            }

            protected override void OnRetain(AssetHandle asset, bool old)
            {
                RetainRef(asset);
                if (!old) return;
                bundles.RetainRef(asset.bundleName);
            }
        }
    }
}
