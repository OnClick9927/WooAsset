namespace WooAsset
{
    public class CopyStreamBundlesOperation : Operation
    {
        private readonly string srcPath;
        private readonly string destPath;
        private string[] files;
        private int step = 0;
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                return step / (float)files.Length;
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
            string destlistPath = AssetsHelper.CombinePath(destPath, StreamBundleList.fileName);
            if (AssetsInternal.NeedCopyStreamBundles() && !AssetsHelper.ExistsFile(destlistPath))
            {
                string srclistPath = AssetsHelper.CombinePath(srcPath, StreamBundleList.fileName);
                Downloader downloader = new Downloader(srclistPath, 10, 1);
                await downloader;
                if (!downloader.isErr)
                {

                    StreamBundleList list = AssetsHelper.ReadObject<StreamBundleList>(downloader.data);
                    foreach (var fileName in list.fileNames)
                    {
                        string dest = AssetsHelper.CombinePath(destPath, fileName).Replace(".bytes", "");
                        if (AssetsHelper.ExistsFile(dest)) continue;
                        string src = AssetsHelper.CombinePath(srcPath, fileName);

                        Downloader _d = new Downloader(src, 10, 1);
                        await _d;
                        if (_d.isErr) continue;
                        await AssetsHelper.WriteFile(_d.data, dest, true);
                    }
                    await AssetsHelper.WriteObject(
                              list,
                              destlistPath,
                             true);
                }
            }
            InvokeComplete();

        }
    }

}
