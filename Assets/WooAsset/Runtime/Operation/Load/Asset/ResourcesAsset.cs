using UnityEngine;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public class ResourcesAsset : Asset
    {
        private ResourceRequest loadOp;
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
          
        }
        protected async override void LoadUnityObject()
        {
            var _type = GetAssetType(type);
            _allAssets = Resources.LoadAll(path, _type);
            if (async)
            {
                loadOp = Resources.LoadAsync(path, _type);
                await loadOp;
                SetResult(loadOp.asset);
            }
            else
            {
                SetResult(Resources.Load(path, _type));
            }
        }

    }

}
