

using System.Collections.Generic;

namespace WooAsset
{
    public class UnzipRawFileOperation : AssetOperation
    {
        public override float progress => _progress;
        protected float _progress;
        public UnzipRawFileOperation(List<string> files)
        {
            if (files == null || files.Count == 0)
            {
                _progress = 1;
                InvokeComplete();
            }
            else
                Done(files);
        }
        public virtual async void Done(List<string> files)
        {
            _progress = 0f;
            for (int i = 0; i < files.Count; i++)
            {
                string path = files[i];
                var asset = await AssetsInternal.LoadAsset(path, true, false) as Asset;
                if (asset.isErr)
                {
                    SetErr(asset.error);
                    break;
                }
                else
                {
                    RawObject obj = asset.GetAsset<RawObject>();
                    string targetPath = AssetsInternal.GetRawFileToDlcPath(obj.rawPath);

                    bool exist = AssetsHelper.ExistsFile(targetPath);
                    if (!exist)
                        await AssetsHelper.WriteFile(obj.bytes, targetPath, true);
                    AssetsInternal.Release(asset.path);
                    _progress = i / (float)files.Count;
                }
            }
            _progress = 1;
            InvokeComplete();
        }
    }

}
