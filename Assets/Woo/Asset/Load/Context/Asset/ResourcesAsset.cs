using System.Collections.Generic;
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
                return loadOp != null ? loadOp.progress : 0;
            }
        }
        public ResourcesAsset(Bundle bundle, List<Asset> dps, AssetLoadArgs loadArgs) : base(bundle, dps, loadArgs)
        {
            var extend = Path.GetExtension(path);
            resPath = path.Replace(extend, "");
        }
        protected async override void OnLoad()
        {
            await bundle;
            loadOp = Resources.LoadAsync<Object>(resPath);
            await loadOp;
            SetResult(loadOp.asset);
        }
        protected override void OnUnLoad()
        {
            Resources.UnloadAsset(value);
        }
    }

}
