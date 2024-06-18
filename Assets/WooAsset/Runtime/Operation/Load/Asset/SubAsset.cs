using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public class SubAsset : Asset
    {
        private Object[] assets;
        public SubAsset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {

        }

        public virtual Object[] allAssets => isDone && !isErr ? assets : null;
        public IReadOnlyList<T> GetSubAssets<T>() where T : Object => !isDone || isErr
                ? null
                : allAssets
                .Where(x => x is T)
                .Select(x => x as T)
                .ToArray();
        public T GetSubAsset<T>(string name) where T : Object => !isDone || isErr
            ? null :
            allAssets
            .Where(x => x.name == name)
            .FirstOrDefault() as T;


        protected override AssetRequest LoadAsync(string path, Type type)
        {
            return bundle.LoadAssetWithSubAssetsAsync(path, type);
        }
        protected override void OnLoadAsyncEnd(AssetRequest request)
        {
            assets = request.allAssets;
        }
        protected override Object LoadSync(string path, Type type)
        {
            var result = bundle.LoadAssetWithSubAssets(path, type);
            assets = result;
            return result[0];
        }
    }

}
