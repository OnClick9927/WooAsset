using System.Collections.Generic;

namespace WooAsset
{
    class MixedAssetLife : IAssetLife<AssetHandle>, IAssetLife<Bundle>
    {

        private List<IAssetLife<AssetHandle>> assets = new List<IAssetLife<AssetHandle>>();
        private List<IAssetLife<Bundle>> bundles = new List<IAssetLife<Bundle>>();


        public void AddLife(IAssetLife life)
        {
            if (life == null) return;
            if (life is IAssetLife<AssetHandle>)
                assets.Add(life as IAssetLife<AssetHandle>);
            if (life is IAssetLife<Bundle>)
                bundles.Add(life as IAssetLife<Bundle>);
        }

        public void RemoveAssetLife(IAssetLife life)
        {
            if (life == null) return;
            if (life is IAssetLife<AssetHandle>)
                assets.Remove(life as IAssetLife<AssetHandle>);
            if (life is IAssetLife<Bundle>)
                bundles.Remove(life as IAssetLife<Bundle>);
        }



        void IAssetLife<AssetHandle>.OnAssetCreate(string path, AssetHandle asset)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                assets[i]?.OnAssetCreate(path, asset);
            }
        }

        void IAssetLife<Bundle>.OnAssetCreate(string path, Bundle asset)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                bundles[i]?.OnAssetCreate(path, asset);
            }
        }

        void IAssetLife<AssetHandle>.OnAssetRelease(AssetHandle asset, int count)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                assets[i]?.OnAssetRelease(asset, count);
            }
        }

        void IAssetLife<Bundle>.OnAssetRelease(Bundle asset, int count)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                bundles[i]?.OnAssetRelease(asset, count);
            }
        }

        void IAssetLife<AssetHandle>.OnAssetRetain(AssetHandle asset, int count)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                assets[i]?.OnAssetRetain(asset, count);
            }
        }

        void IAssetLife<Bundle>.OnAssetRetain(Bundle asset, int count)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                bundles[i]?.OnAssetRetain(asset, count);
            }
        }

        void IAssetLife<AssetHandle>.OnAssetUnload(string path, AssetHandle asset)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                assets[i]?.OnAssetUnload(path, asset);
            }
        }

        void IAssetLife<Bundle>.OnAssetUnload(string path, Bundle asset)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                bundles[i]?.OnAssetUnload(path, asset);
            }
        }


    }
}
