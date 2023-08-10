using System.Collections.Generic;
using static WooAsset.AssetsHelper;

namespace WooAsset
{
    public class CheckBundleVersionOperation : Operation
    {
        private Downloader downloader;
        private AssetsVersionCollection remote;
        public virtual List<AssetsVersionCollection.VersionData> versions => remote?.versions;
        public override float progress => isDone ? 1 : downloader.progress;
        public CheckBundleVersionOperation()
        {
            Done();
        }

        public virtual VersionCompareOperation Compare(int versionIndex, params string[] tags)
        {
            return new VersionCompareOperation(this, versionIndex, tags);
        }
        protected virtual async void Done()
        {
            downloader = AssetsInternal.DownloadVersion(VersionBuffer.remoteHashName);
            await downloader;
            if (downloader.isErr)
            {
                SetErr(downloader.error);
            }
            else
            {
                remote = VersionBuffer.ReadAssetsVersionCollection(downloader.data, AssetsInternal.GetEncrypt());
            }
            AssetsHelper.Log($"Check Version Complete");
            InvokeComplete();
        }
    }

}
