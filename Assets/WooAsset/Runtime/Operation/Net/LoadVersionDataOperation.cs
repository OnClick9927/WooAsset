namespace WooAsset
{
    public class LoadVersionDataOperation : Operation
    {
        private VersionData version;
        Operation _downloader;
        public VersionData GetVersion() { return version; }
        public LoadVersionDataOperation(string version)
        {
            Done(version);
        }

        public override float progress => isDone ? 1f : _downloader.progress;

        private async void Done(string version)
        {
            var _downloader = await AssetsInternal.DownloadVersion(version, VersionHelper.VersionDataName);
            this.version = VersionHelper.ReadVersionData((_downloader as Downloader).data);
            InvokeComplete();
        }
    }


}
