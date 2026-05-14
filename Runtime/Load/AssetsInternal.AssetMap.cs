namespace WooAsset
{
    partial class AssetsInternal
    {
        private class AssetMap : NameMap<AssetHandle, AssetLoadArgs>
        {

            public AssetHandle LoadAsset(AssetData data, bool async, System.Type type, bool sub)
            {
                var args = AssetLoadArgs.NormalArg(data, async, type, sub);
                return LoadAsync(args);
            }
            public AssetHandle LoadAssetCustom(string path, bool async, System.Type type)
            {
                var args = AssetLoadArgs.CustomArg(path, async, type);
                return LoadAsync(args);
            }
            protected override AssetHandle CreateNew(AssetLoadArgs args)
            {
                if (ResourceAsset.IsFit(args.path))
                {
                    return new ResourceAsset(args.path, args.async, args.type);
                }
                Bundle bundle = bundles.LoadBundle(args.data.bundleName, args.async);
                AssetHandle handle;
                if (args.data.type == AssetType.Raw)
                    handle = new RawAsset(args, bundle);
                else if (args.data.type == AssetType.Scene)
                    handle = new SceneAsset(args, bundle);
                else if (args.sub)
                    handle = new SubAsset(args, bundle);
                else
                    handle = new Asset(args, bundle);
                return handle;
            }


            public override void Release(string path)
            {
                AssetHandle asset = Find(path);
                if (asset == null) return;
                ReleaseRef(asset);
                TryRealUnload(path);
                if (asset.IsBundleAsset)
                    bundles.Release(asset.bundleName);
            }

            protected override void OnRetain(AssetHandle asset, bool old)
            {
                base.OnRetain(asset, old);
                if (!old) return;
                if (asset.IsBundleAsset)
                    bundles.LoadBundle(asset.bundleName, false);
            }
        }
    }
}
