namespace WooAsset
{
    class CopyStreamBundlesOperation : Operation
    {
        private readonly string srcPath;
        private readonly string destPath;
 
        private float _progress;
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                return _progress;
            }
        }
        public CopyStreamBundlesOperation(string srcPath, string destPath)
        {
            this.srcPath = srcPath;
            this.destPath = destPath;
            Copy();
        }
        protected virtual async void Copy()
        {
            string destlistPath = AssetsHelper.CombinePath(destPath, StreamBundlesData.fileName);
            string srclistPath = AssetsHelper.CombinePath(srcPath, StreamBundlesData.fileName);
            if (AssetsInternal.NeedCopyStreamBundles() && AssetsHelper.ExistsFile(srclistPath) && !AssetsHelper.ExistsFile(destlistPath))
            {
                DownLoader downloader = AssetsInternal.DownloadBytes(AssetsInternal.GetStreamingFileUrl(srclistPath));
                await downloader;
                _progress = 0.2f;
                if (!downloader.isErr)
                {
                    StreamBundlesData list = AssetsHelper.ReadBufferObject<StreamBundlesData>(downloader.data);
                    float seg = 0.8f / list.fileNames.Length;
                    foreach (var fileName in list.fileNames)
                    {
                        _progress += seg;
                        string dest = AssetsHelper.CombinePath(destPath, fileName).Replace(StreamBundlesData.fileExt, "");
                        if (AssetsHelper.ExistsFile(dest)) continue;
                        string src = AssetsHelper.CombinePath(srcPath, fileName);
                        await AssetsInternal.DownLoadFile(AssetsInternal.GetStreamingFileUrl(src), dest);
                    }
                    var writer = AssetsHelper.WriteBufferObject(list);
                    await AssetsHelper.WriteFile(writer.buffer, destlistPath, 0, writer.length);
                }
            }
            InvokeComplete();

        }
    }

}
