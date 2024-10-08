using System.Collections;
using System.Collections.Generic;

namespace WooAsset
{
    public class AssetsGroupOperation : Operation, IEnumerable<AssetHandle>
    {
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                float sum = 0;
                for (int i = 0; i < assets.Count; i++)
                    sum += assets[i].progress;
                return sum / assets.Count;
            }
        }

        private IReadOnlyList<string> paths;
        private List<AssetHandle> assets;

        public int count => paths.Count;

        public AssetHandle FindAsset(string path)
        {
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
            Done();
        }
        private async void Done()
        {
            if (paths != null)
            {
                assets = new List<AssetHandle>(paths.Count);
                for (int i = 0; i < paths.Count; i++)
                {
                    assets.Add(AssetsInternal.LoadAsset(paths[i], false, true, typeof(UnityEngine.Object)));
                }
                for (int i = 0; i < paths.Count; i++)
                {
                    await assets[i];
                }
            }
            InvokeComplete();
        }
        public void Release()
        {
            if (assets != null)
            {
                for (int i = 0; i < assets.Count; i++)
                {
                    Assets.Release(assets[i]);
                }
            }
            paths = null;
            assets = null;
        }

        IEnumerator<AssetHandle> IEnumerable<AssetHandle>.GetEnumerator()
        {
            return assets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<AssetHandle>)this).GetEnumerator();
        }
    }
}
