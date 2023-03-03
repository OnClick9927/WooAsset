using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public class Asset : Asset<Object>
    {
        public Bundle bundle;
        private AssetBundleRequest loadOp;
        protected List<Asset> dps;
        public override string path { get { return loadArgs.path; } }
        private AssetLoadArgs loadArgs;
        public Asset(Bundle bundle, List<Asset> dps, AssetLoadArgs loadArgs)
        {
            this.bundle = bundle;
            this.dps = dps;
            this.loadArgs = loadArgs;
        }
        public override float progress
        {
            get
            {
                float sum = 0;
                if (bundle.isDone)
                {
                    sum = isDone ? 1 : (loadOp == null) ? 0 : loadOp.progress;
                }
                if (dps == null) return sum;
                float dpSum = 0;
                for (int i = 0; i < dps.Count; i++)
                    dpSum += dps[i].progress / dps.Count;
                return (dpSum + sum) * 0.5f;
            }
        }

        private Sprite sp;
        public T GetAsset<T>() where T : Object
        {
            if (value is Texture2D)
            {
                if (typeof(T) == typeof(Sprite))
                {
                    if (sp == null)
                    {
                        var tx = value as Texture2D;
                        sp = Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), Vector2.one * 0.5f);
                    }
                    return sp as T;
                }
            }
            return value as T;
        }
        protected async override void OnLoad()
        {
            await bundle;
            loadOp = bundle.LoadAssetAsync(path, typeof(UnityEngine.Object));
            await loadOp;
            SetResult(loadOp.asset);
        }
        protected override void OnUnLoad()
        {
            Resources.UnloadAsset(value);
        }
    }

}
