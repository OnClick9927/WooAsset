using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
namespace WooAsset
{

    public class Asset : AssetHandle
    {
        private Object[] assets;

        private AssetBundleRequest loadOp;
        public Asset(AssetLoadArgs loadArgs) : base(loadArgs)
        {

        }
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                if (async)
                {

                    if (loadOp == null)
                        return bundle.progress * 0.25f + dpProgress * 0.5f;
                    return 0.25f + 0.25f * loadOp.progress + dpProgress * 0.5f;
                }
                return bundle.progress * 0.5f + dpProgress * 0.5f;
            }
        }

        public virtual T GetAsset<T>() where T : Object => isDone && !unloaded ? value as T : null;
        public virtual Type GetAssetType() => isDone && !isErr && !unloaded ? value.GetType() : null;
        public virtual Object[] allAssets => isDone && !isErr && !unloaded ? assets : null;
        public IReadOnlyList<T> GetSubAssets<T>() where T : Object => !isDone || isErr || unloaded
                ? null
                : allAssets
                .Where(x => x is T)
                .Select(x => x as T)
                .ToArray();
        public T GetSubAsset<T>(string name) where T : Object => !isDone || isErr || unloaded
            ? null :
            allAssets
            .Where(x => x.name == name)
            .FirstOrDefault() as T;



        protected async override void OnLoad()
        {
            await LoadBundle();
            if (bundle.unloaded)
            {
                this.SetErr($"bundle Has been Unloaded  {path}");
                InvokeComplete();
                return;
            }

            if (bundle.isErr)
            {
                this.SetErr(bundle.error);
                InvokeComplete();
                return;
            }
            if (async)
            {
                loadOp = bundle.LoadAssetAsync(path, typeof(UnityEngine.Object));
                await loadOp;
                assets = loadOp.allAssets;
                SetResult(loadOp.asset);
            }
            else
            {
                var result = bundle.LoadAsset(path, typeof(UnityEngine.Object));
                assets = result;
                SetResult(result[0]);
            }
        }
       
    }

}
