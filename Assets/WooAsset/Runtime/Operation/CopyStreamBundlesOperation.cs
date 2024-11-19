using System.Collections.Generic;

namespace WooAsset
{
    class CopyStreamBundlesOperation : GroupOperation<DownLoader>
    {
        private readonly string srcPath;
        private readonly string destPath;
        string destlistPath, srclistPath;
        StreamBundlesData streamBundlesData;
        public CopyStreamBundlesOperation(string srcPath, string destPath)
        {
            this.srcPath = srcPath;
            this.destPath = destPath;
            destlistPath = AssetsHelper.CombinePath(destPath, StreamBundlesData.fileName);
            srclistPath = AssetsHelper.CombinePath(srcPath, StreamBundlesData.fileName);
            Copy();
        }

        protected async override void BeforeInvokeComplete()
        {
            if (streamBundlesData != null)
            {
                var writer = AssetsHelper.WriteBufferObject(streamBundlesData);
                await AssetsHelper.WriteFile(writer.buffer, destlistPath, 0, writer.length);
            }
        }
        protected virtual async void Copy()
        {
            if (AssetsInternal.NeedCopyStreamBundles() && AssetsHelper.ExistsFile(srclistPath) && !AssetsHelper.ExistsFile(destlistPath))
            {
                DownLoader downloader = await AssetsInternal.DownloadBytes(AssetsInternal.GetStreamingFileUrl(srclistPath));
                if (!downloader.isErr)
                {
                    streamBundlesData = AssetsHelper.ReadBufferObject<StreamBundlesData>(downloader.data);
                    List<DownLoader> ds = new List<DownLoader>();
                    foreach (var fileName in streamBundlesData.fileNames)
                    {
                        string dest = AssetsHelper.CombinePath(destPath, fileName).Replace(StreamBundlesData.fileExt, "");
                        if (AssetsHelper.ExistsFile(dest)) continue;
                        string src = AssetsHelper.CombinePath(srcPath, fileName);
                        ds.Add(AssetsInternal.DownLoadFile(AssetsInternal.GetStreamingFileUrl(src), dest));
                    }

                    base.Done(ds);

                }
                else
                {
                    base.Done(null);
                }
            }
            else
            {
                base.Done(null);
            }

        }
    }

}
