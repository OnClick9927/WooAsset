

namespace WooAsset
{
    public partial class Assets
    {
        public class AssetsGroupOperation : AssetOperation
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
            private Asset[] assets;
            public AssetsGroupOperation(string[] paths)
            {
                this.paths = paths;
                Done();
            }
            private async void Done()
            {
                if (paths != null)
                {
                    assets = new Asset[paths.Length];
                    for (int i = 0; i < paths.Length; i++)
                    {
                        assets[i] = Assets.LoadAssetAsync(paths[i]);
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
                for (int i = 0; i < assets.Length; i++)
                {
                    AssetsInternal.Release(assets[i]);
                }
                paths = null;
                assets = null;
            }
        }
    }
}
