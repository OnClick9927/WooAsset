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
        public ResourcesAsset(AssetLoadArgs loadArgs) : base(loadArgs)
        {
            var extend = AssetsHelper.GetFileExtension(path);
            resPath = path.Replace(extend, "");
        }
        protected async override void LoadUnityObject()
        {
            var _type = GetAssetType(type);
            _allAssets = Resources.LoadAll(resPath, _type);
            if (async)
            {
                loadOp = Resources.LoadAsync(resPath, _type);
                await loadOp;
                SetResult(loadOp.asset);
            }
            else
            {
                SetResult(Resources.Load(resPath, _type));
            }
        }

    }

}
