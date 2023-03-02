

using UnityEngine;
namespace WooAsset
{
    partial class AssetsInternal
    {
        public class LoadManifestOperation : AssetOperation
        {
            public override float progress
            {
                get
                {
                    if (isDone) return 1;
                    if (op == null)
                        return bundle.progress * 0.5f;
                    return bundle.progress * 0.5f + op.progress * 0.5f;
                }
            }

            private Bundle bundle;
            private AssetBundleRequest op;

            public LoadManifestOperation()
            {
                Done();
            }

            private async void Done()
            {
                if (isNormalMode)
                {
                    string path = AssetManifest.Path;
                    bundle = await LoadBundleAsync(AssetsInternal.GetNameHash(path));
                    op = await bundle.LoadAssetAsync(path, typeof(AssetManifest));
                    manifest = op.asset as AssetManifest;
                    manifest.Prepare();
                }
                InvokeComplete();
            }
        }
    }
}
