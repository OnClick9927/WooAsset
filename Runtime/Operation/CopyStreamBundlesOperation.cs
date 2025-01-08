using System.Collections.Generic;

namespace WooAsset
{
    class CopyStreamBundlesOperation : GroupOperation<DownLoader>
    {
        private readonly string srcPath;
        private readonly string destPath;
        string tarFilePath, srcFilePath;
        StreamBundlesData streamBundlesData;
        public CopyStreamBundlesOperation(string srcPath, string targetPath, bool again)
        {
            this.srcPath = srcPath;
            this.destPath = targetPath;
            tarFilePath = AssetsHelper.CombinePath(targetPath, StreamBundlesData.fileName);
            srcFilePath = AssetsHelper.CombinePath(srcPath, StreamBundlesData.fileName);
            Copy(again);
        }

        protected async override void BeforeInvokeComplete()
        {
            if (streamBundlesData != null)
            {
                var writer = AssetsHelper.WriteBufferObject(streamBundlesData);
                await AssetsHelper.WriteFile(writer.buffer, tarFilePath, 0, writer.length);
            }
        }
        protected virtual async void Copy(bool again)
        {
            if (AssetsInternal.NeedCopyStreamBundles())
            {
                if (again || !AssetsHelper.ExistsFile(tarFilePath))
                {

                    BytesDownLoader downloader = await AssetsInternal.DownloadBytes(AssetsInternal.GetStreamingFileUrl(srcFilePath));
                    if (!downloader.isErr)
                    {
                        streamBundlesData = AssetsHelper.ReadBufferObject<StreamBundlesData>(downloader.data);
                        List<DownLoader> downloaders = new List<DownLoader>();
                        foreach (var fileName in streamBundlesData.fileNames)
                        {
                            string dest = AssetsHelper.CombinePath(destPath, fileName).Replace(StreamBundlesData.fileExt, "");
                            if (AssetsHelper.ExistsFile(dest)) continue;
                            string src = AssetsHelper.CombinePath(srcPath, fileName);
                            downloaders.Add(AssetsInternal.DownLoadFile(AssetsInternal.GetStreamingFileUrl(src), dest));
                        }

                        base.Done(downloaders);

                    }
                    else
                    {
                        SetErr(downloader.error);
                        InvokeComplete();
                    }
                }
                else
                    InvokeComplete();
            }
            else
                InvokeComplete();
        }
    }

}
