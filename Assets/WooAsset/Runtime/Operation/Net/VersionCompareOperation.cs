using System.Collections.Generic;
using static WooAsset.AssetsVersionCollection;
using static WooAsset.AssetsVersionCollection.VersionData;


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
                FileData data = FileData.CreateByFile(AssetsHelper.ToRegularPath(files[i]));
                result.Add(data);
            }
            return result;
        }

        private CheckBundleVersionOperation _check;
        private VersionData version;
        private List<PackageData> pkgs;
        public override float progress => isDone ? 1 : _progress;
        private float _progress;

        public List<BundleFileData> change;
        public List<FileData> delete;
        public List<BundleFileData> add;
        IAssetStreamEncrypt en;
        public VersionCompareOperation(CheckBundleVersionOperation _check, VersionData version, List<PackageData> pkgs, IAssetStreamEncrypt en)
        {
            this._check = _check;
            this.pkgs = pkgs;
            this.version = version;
            this.en = en;
            Compare();
        }


        protected virtual async void Compare()
        {
            if (!_check.isErr)
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
                            BundlesVersion v = VersionHelper.ReadBundleVersion(downloader.data, en);
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
                            ManifestData v = VersionHelper.ReadManifest(downloader.data, en);
                            await VersionHelper.WriteManifest(v, AssetsInternal.GetBundleLocalPath(fileName), en);
                        }
                    }
                }
                List<FileData> local = GetLocalBundles();
                FileData.Compare(local, remoteBundles, AssetsInternal.GetFileCheckType(), out change, out delete, out add);
                await VersionHelper.WriteVersionData(version,
                      AssetsInternal.GetBundleLocalPath(VersionHelper.localHashName),
                      en
                      );

            }
            InvokeComplete();
        }
    }

}
