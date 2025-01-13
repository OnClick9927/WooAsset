using System.Collections.Generic;

namespace WooAsset
{
    class CopyStreamBundlesOperation : GroupOperation<DownLoader>
    {
        private readonly string srcPath;
        private readonly string destPath;
        string tarFilePath, srcFilePath;
        StreamBundlesData remote;
        public CopyStreamBundlesOperation(string srcPath, string targetPath)
        {
            this.srcPath = srcPath;
            this.destPath = targetPath;
            tarFilePath = AssetsHelper.CombinePath(targetPath, StreamBundlesData.fileName);
            srcFilePath = AssetsHelper.CombinePath(srcPath, StreamBundlesData.fileName);
            Copy();
        }

        protected async override void BeforeInvokeComplete()
        {
            if (remote != null)
            {
                var writer = AssetsHelper.WriteBufferObject(remote);
                await AssetsHelper.WriteFile(writer.buffer, tarFilePath, 0, writer.length);
            }
        }

        protected virtual async void Copy()
        {
            if (AssetsInternal.NeedCopyStreamBundles())
            {
                bool copy = !AssetsHelper.ExistsFile(tarFilePath);

                BytesDownLoader downloader = await AssetsInternal.DownloadBytes(AssetsInternal.GetStreamingFileUrl(srcFilePath));
                if (!downloader.isErr)
                {
                    remote = AssetsHelper.ReadBufferObject<StreamBundlesData>(downloader.data);
                }
                else
                {
                    SetErr(downloader.error);
                    InvokeComplete();
                    return;
                }
                if (!copy)
                {
                    var op = await AssetsHelper.ReadFile(tarFilePath, true);
                    StreamBundlesData local = AssetsHelper.ReadBufferObject<StreamBundlesData>(op.bytes);
                    if (local.version != remote.version)
                        copy = true;
                }
                if (!copy) InvokeComplete();
                else
                {
                    List<DownLoader> downloaders = new List<DownLoader>();
                    foreach (var fileName in remote.fileNames)
                    {
                        string dest = AssetsHelper.CombinePath(destPath, fileName).Replace(StreamBundlesData.fileExt, "");
                        string src = AssetsHelper.CombinePath(srcPath, fileName);
                        downloaders.Add(AssetsInternal.DownLoadFile(AssetsInternal.GetStreamingFileUrl(src), dest));
                    }

                    base.Done(downloaders);
                }
            }
            else
                InvokeComplete();
        }
    }

}
