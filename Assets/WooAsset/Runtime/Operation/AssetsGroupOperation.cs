using System.Collections;
using System.Collections.Generic;

namespace WooAsset
{
    public class AssetsGroupOperation : GroupOperation<AssetHandle>, IEnumerable<AssetHandle>
    {
        private List<AssetHandle> assets;
        private IReadOnlyList<string> paths;

        public AssetHandle FindAsset(string path)
        {
            if (paths == null) return null;
            var data = AssetsInternal.GetAssetData(path);
            if (data == null) return null;
            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i] == data.path)
                {
                    return assets[i];
                }
            }
            return null;
        }
        public AssetsGroupOperation(IReadOnlyList<string> paths)
        {
            this.paths = paths;
            if (paths != null)
            {
                assets = new List<AssetHandle>(paths.Count);
                for (int i = 0; i < paths.Count; i++)
                    assets.Add(AssetsInternal.LoadAsset(paths[i], false, true, typeof(UnityEngine.Object)));
            }
            base.Done(assets);
        }

        public void Release()
        {
            if (assets != null)
                for (int i = 0; i < assets.Count; i++)
                    Assets.Release(assets[i]);
            assets = null;
        }

        IEnumerator<AssetHandle> IEnumerable<AssetHandle>.GetEnumerator()
        {
            if (assets == null) return null;
            return assets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<AssetHandle>)this).GetEnumerator();
        }
    }
}
