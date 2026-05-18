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
            var _downloader = await AssetsInternal.DownloadVersion(version, AssetsHelper.VersionDataName);
            if (_downloader.isErr)
                SetErr(_downloader.error);
            else
                this.version = AssetsHelper.ReadBufferObject<VersionData>(_downloader.data);

            InvokeComplete();
        }
    }


}
