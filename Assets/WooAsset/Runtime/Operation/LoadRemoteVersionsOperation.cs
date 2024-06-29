namespace WooAsset
{
    public class LoadRemoteVersionsOperation : Operation
    {
        private DownLoader downloader;
        private VersionCollectionData remote;
        public VersionCollectionData Versions => remote;
        public override float progress => isDone ? 1 : downloader.progress;
        public LoadRemoteVersionsOperation()
        {
            Done();
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
                remote = VersionHelper.ReadAssetsVersionCollection(downloader.data);
            }
            AssetsHelper.Log($"Check Version Complete");
            InvokeComplete();
        }
    }

}
