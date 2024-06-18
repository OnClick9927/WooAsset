using System;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public class Asset : AssetHandle<UnityEngine.Object>
    {
        private AssetRequest loadOp;
        public Asset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {

        }


        public sealed override float progress
        {
            get
            {
                if (isDone) return 1;
                if (async)
                {
                    if (loadOp == null)
                        return bundle.progress * 0.5f;
                    return 0.5f + 0.5f * loadOp.progress;
                }
                return bundle.progress;
            }
        }

        public T GetAsset<T>() where T : Object => isDone ? value as T : null;
        public Type GetAssetType() => isDone && !isErr ? value.GetType() : null;



        protected virtual AssetRequest LoadAsync(string path, Type type) => bundle.LoadAssetAsync(path, type);
        protected virtual void OnLoadAsyncEnd(AssetRequest request) { }
        protected virtual Object LoadSync(string path, Type type) => bundle.LoadAsset(path, type);
        protected sealed async override void InternalLoad()
        {
            if (bundle.isErr)
            {
                this.SetErr(bundle.error);
                InvokeComplete();
                return;
            }
            var _type = AssetsHelper.GetAssetType(assetType, type);
            if (async)
            {
                loadOp = LoadAsync(path, _type);
                await loadOp;
                OnLoadAsyncEnd(loadOp);
                SetResult(loadOp.asset);
            }
            else
            {
                var result = LoadSync(path, _type);
                SetResult(result);
            }

        }

    }

}
