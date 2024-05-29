namespace WooAsset
{
    partial class AssetsInternal
    {
        private class AssetMap : NameMap<AssetHandle>
        {

            protected override AssetHandle CreateNew(IAssetArgs args)
            {
                AssetLoadArgs arg = (AssetLoadArgs)args;
                Bundle bundle = bundles.LoadBundle(arg.data.bundleName, arg.async);
                AssetHandle handle;
                if (arg.data.type == AssetType.Raw)
                    handle = new RawAsset(arg, bundle);
                else if (arg.data.type == AssetType.Scene)
                    handle = new SceneAsset(arg, bundle);
                else
                    handle = new Asset(arg, bundle);
                return handle;
            }


            public override void Release(string path)
            {
                AssetHandle asset = Find(path);
                if (asset == null) return;
                ReleaseRef(asset);
                TryRealUnload(path);
                bundles.Release(asset.bundleName);
            }

            protected override void OnRetain(AssetHandle asset, bool old)
            {
                base.OnRetain(asset, old);
                if (!old) return;
                bundles.LoadBundle(asset.bundleName, false);
            }
        }
    }
}
