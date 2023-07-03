using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public class ResourcesAsset : Asset
    {
        private ResourceRequest loadOp;
        private string resPath;
        public override float progress
        {
            get
            {
                if (isDone)
                    return 1;
                if (async)
                    return loadOp != null ? loadOp.progress : 0;
                return 0;
            }
        }
        public Object[] _allAssets;
        public override Object[] allAssets => _allAssets;
        public override T GetAsset<T>()
        {
            if (value is Texture2D)
            {
                if (typeof(T) == typeof(Sprite))
                {
                    var sp = Resources.Load(resPath, typeof(Sprite));
                    SetResult(sp);
                }
            }
            return value as T;
        }

        public ResourcesAsset(AssetLoadArgs loadArgs) : base(loadArgs)
        {
            var extend = Path.GetExtension(path);
            resPath = path.Replace(extend, "");
        }
        protected async override void OnLoad()
        {
            _allAssets = Resources.LoadAll<Object>(resPath);
            if (async)
            {
                loadOp = Resources.LoadAsync<Object>(resPath);
                await loadOp;
                SetResult(loadOp.asset);
            }
            else
            {
                SetResult(Resources.Load<Object>(resPath));
            }
        }

    }

}
