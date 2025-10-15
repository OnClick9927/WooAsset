using UnityEditor;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace WooAsset
{

    partial class AssetsEditorTool
    {
        internal class LifePart : IAssetLife<Bundle>, IAssetLife<AssetHandle>
        {
            async void IAssetLife<Bundle>.OnAssetCreate(string path, Bundle asset)
            {
                await asset;
                AssetLife<Bundle> life = new AssetLife<Bundle>()
                {
                    asset = asset,
                    assetLength = asset.length
                };
                bundles.Add(asset.bundleName, life);
                life.assetLength = asset.length;
                onAssetLifeChange?.Invoke();
            }
            void IAssetLife<Bundle>.OnAssetRetain(Bundle asset, int count) => onAssetLifeChange?.Invoke();
            void IAssetLife<Bundle>.OnAssetRelease(Bundle asset, int count) => onAssetLifeChange?.Invoke();
            void IAssetLife<Bundle>.OnAssetUnload(string path, Bundle asset)
            {
                bundles.Remove(asset.bundleName);
                onAssetLifeChange?.Invoke();
            }
            async void IAssetLife<AssetHandle>.OnAssetCreate(string path, AssetHandle asset)
            {
                await asset;
                var data = asset.data;
                var life = new AssetLife<AssetHandle>()
                {
                    asset = asset,
                    tags = Assets.GetAssetTags(path),
                    assetType = AssetsInternal.GetAssetData(path).type.ToString(),
                };
                assets.Add(path, life);
                onAssetLifeChange?.Invoke();
                //onAssetLifeChange?.Invoke();
            }
            void IAssetLife<AssetHandle>.OnAssetRelease(AssetHandle asset, int count) => onAssetLifeChange?.Invoke();
            void IAssetLife<AssetHandle>.OnAssetRetain(AssetHandle asset, int count) => onAssetLifeChange?.Invoke();
            void IAssetLife<AssetHandle>.OnAssetUnload(string path, AssetHandle asset)
            {
                assets.Remove(path);
                onAssetLifeChange?.Invoke();

            }
            public class AssetLife<T> where T : AssetOperation
            {
                public T asset;
                public long assetLength;
                public IReadOnlyList<string> tags;
                public string assetType;
            }
            public static Dictionary<string, AssetLife<Bundle>> bundles = new Dictionary<string, AssetLife<Bundle>>();
            public static Dictionary<string, AssetLife<AssetHandle>> assets = new Dictionary<string, AssetLife<AssetHandle>>();
            public static event Action onAssetLifeChange;

            public static void Clear()
            {
                bundles.Clear();
                assets.Clear();
            }

        }

    }
}
