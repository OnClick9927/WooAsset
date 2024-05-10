namespace WooAsset
{
    partial class AssetsInternal
    {
        private class AssetMap : NameMap<AssetHandle>
        {
            protected override AssetHandle CreateNew(IAssetArgs args) => CreateAsset((AssetLoadArgs)args);
            public override void Release(string path)
            {
                AssetHandle asset = Find(path);
                if (asset == null) return;
                ReleaseRef(asset);
                bundles.Release(asset.bundleName);
                TryRealUnload(path);
                if (asset.dps != null)
                {
                    foreach (var item in asset.dps)
                    {
                        Release(item.path);
                    }
                }
            }
            internal void RemoveUselessAsset()
            {
                var all = GetAll();
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].isBundleUnloaded)
                    {
                        Remove(all[i].path);
                    }
                }
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
