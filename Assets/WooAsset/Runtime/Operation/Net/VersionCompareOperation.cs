using System.Collections.Generic;


namespace WooAsset
{
    public class VersionCompareOperation : Operation
    {
        private static List<FileData> GetLocalBundles()
        {
            var files = AssetsHelper.GetDirectoryFiles(AssetsInternal.GetLocalSaveDir());
            List<FileData> result = new List<FileData>();
            for (int i = 0; i < files.Length; i++)
            {
                FileData data = FileData.CreateByFile(files[i]);
                result.Add(data);
            }
            return result;
        }

        private VersionData version;
        private List<PackageData> pkgs;
        public override float progress => isDone ? 1 : _progress;
        private float _progress;

        public List<BundleFileData> change;
        public List<FileData> delete;
        public List<BundleFileData> add;
        public VersionCompareOperation(VersionData version, List<PackageData> pkgs)
        {
            this.pkgs = pkgs;
            this.version = version;
            Compare();
        }


        protected virtual async void Compare()
        {
            List<BundleFileData> remoteBundles = new List<BundleFileData>();
            for (int i = 0; i < pkgs.Count; i++)
            {
                var pkg = pkgs[i];
                _progress = i / (float)pkgs.Count;
                {
                    string fileName = pkg.bundleFileName;

                    Downloader downloader = AssetsInternal.DownloadVersion(version.version, fileName);
                    await downloader;
                    if (downloader.isErr)
                    {
                        SetErr(downloader.error);
                        break;
                    }
                    else
                    {
                        BundlesVersionData v = VersionHelper.ReadBundleVersion(downloader.data);
                        remoteBundles.AddRange(v.bundles);
                    }
                }
                {
                    string fileName = pkg.manifestFileName;
                    Downloader downloader = AssetsInternal.DownloadVersion(version.version, fileName);
                    await downloader;
                    if (downloader.isErr)
                    {
                        SetErr(downloader.error);
                        break;
                    }
                    else
                    {
                        ManifestData v = VersionHelper.ReadManifest(downloader.data);
                        await VersionHelper.WriteManifest(v, AssetsInternal.GetBundleLocalPath(fileName));
                    }
                }
            }
            List<FileData> local = GetLocalBundles();
            FileData.Compare(local, remoteBundles, AssetsInternal.GetFileCheckType(), out change, out delete, out add);
            await VersionHelper.WriteVersionData(version,
                  AssetsInternal.GetBundleLocalPath(VersionHelper.VersionDataName)
                  );
            InvokeComplete();
        }
    }

}
