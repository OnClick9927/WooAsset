using System.Collections.Generic;
using static WooAsset.AssetsVersionCollection;
using static WooAsset.AssetsVersionCollection.VersionData;


namespace WooAsset
{
    public class CheckBundleVersionOperation : Operation
    {
        private Downloader downloader;
        private AssetsVersionCollection remote;
        public AssetsVersionCollection Versions => remote;
        public override float progress => isDone ? 1 : downloader.progress;
        public CheckBundleVersionOperation()
        {
            Done();
        }



        public virtual VersionCompareOperation Compare(VersionData version, List<PackageData> pkgs)
        {
            return new VersionCompareOperation(this, version, pkgs, AssetsInternal.GetEncrypt());
        }
        protected virtual async void Done()
        {
            downloader = AssetsInternal.DownloadRemoteVersion();
            await downloader;
            if (downloader.isErr)
            {
                SetErr(downloader.error);
            }
            else
            {
                remote = VersionHelper.ReadAssetsVersionCollection(downloader.data, AssetsInternal.GetEncrypt());
            }
            AssetsHelper.Log($"Check Version Complete");
            InvokeComplete();
        }
    }

}
