namespace WooAsset
{
    public class AssetsGroupOperation : Operation
    {
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                float sum = 0;
                for (int i = 0; i < assets.Length; i++)
                    sum += assets[i].progress;
                return sum / assets.Length;
            }
        }

        private string[] paths;
        private AssetHandle[] assets;

        public AssetHandle FindAsset(string path)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == path)
                {
                    return assets[i];
                }
            }
            return null;
        }
        public AssetsGroupOperation(string[] paths)
        {
            this.paths = paths;
            Done();
        }
        private async void Done()
        {
            if (paths != null)
            {
                assets = new AssetHandle[paths.Length];
                for (int i = 0; i < paths.Length; i++)
                {
                    assets[i] = AssetsInternal.LoadAsset(paths[i], false, true, typeof(UnityEngine.Object));
                }
                for (int i = 0; i < paths.Length; i++)
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
                for (int i = 0; i < assets.Length; i++)
                {
                    Assets.Release(assets[i]);
                }
            }
            paths = null;
            assets = null;
        }
    }
}
