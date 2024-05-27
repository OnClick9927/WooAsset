using System;
using System.Collections.Generic;
using static WooAsset.AssetsVersionCollection;
using static WooAsset.AssetsVersionCollection.VersionData;


namespace WooAsset
{
    public class LoadManifestOperation : Operation
    {
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                if (downloader == null)
                    return 0;
                return downloader.progress;
            }
        }
        private Downloader downloader;
        private VersionData version;
        public ManifestData manifest;
        public List<FileData> bundles;

        private string _version = "";
        private Func<VersionData, List<PackageData>> getPkgs;
        private List<string> loadedBundles;
        private IAssetStreamEncrypt encrypt;
        public LoadManifestOperation(List<string> loadedBundles, string version, Func<VersionData, List<PackageData>> getPkgs, IAssetStreamEncrypt encrypt)
        {
            _version = version;
            this.getPkgs = getPkgs;
            this.loadedBundles = loadedBundles;
            this.encrypt = encrypt;
            Done();
        }




        private async void Done()
        {
            IAssetStreamEncrypt en = encrypt;
            string localVersionPath = AssetsInternal.GetBundleLocalPath(VersionHelper.localHashName);
            bool download = false;
            if (!AssetsHelper.ExistsFile(localVersionPath))
                download = true;
            else
            {
                var reader = AssetsHelper.ReadFile(localVersionPath, true);
                await reader;
                version = VersionHelper.ReadVersionData(reader.bytes, en);

                if (!string.IsNullOrEmpty(_version))
                    if (version.version != _version)
                        download = true;
            }
            if (download)
            {
                downloader = AssetsInternal.DownloadRemoteVersion();

                await downloader;
                if (downloader.isErr)
                {
                    SetErr(downloader.error);
                }
                else
                {
                    AssetsVersionCollection collection = VersionHelper.ReadAssetsVersionCollection(downloader.data, en);
                    AssetsVersionCollection.VersionData find = string.IsNullOrEmpty(_version) ? collection.NewestVersion() : collection.FindVersion(_version);
                    version = find != null ? find : collection.NewestVersion();
                    await VersionHelper.WriteVersionData(version, localVersionPath, en);
                }
            }
            var pkgs = this.getPkgs == null ? version.GetAllPkgs() : this.getPkgs.Invoke(version);
            manifest = new ManifestData();
            for (int i = 0; i < pkgs.Count; i++)
            {
                var pkg = pkgs[i];
                {
                    string fileName = pkg.manifestFileName;

                    string _path = AssetsInternal.GetBundleLocalPath(fileName);
                    ManifestData v;
                    if (AssetsHelper.ExistsFile(_path))
                    {
                        var reader =  AssetsHelper.ReadFile(_path, true);
                        await reader;
                        v = VersionHelper.ReadManifest(reader.bytes, en);
                    }
                    else
                    {
                        downloader = AssetsInternal.DownloadVersion(version.version, fileName);
                        await downloader;
                        if (downloader.isErr)
                        {
                            SetErr(downloader.error);
                            break;
                        }
                        v = VersionHelper.ReadManifest(downloader.data, en);
                        await VersionHelper.WriteManifest(v, _path, en);
                    }
                    ManifestData.Merge(v, manifest, this.loadedBundles);
                }
            }
            manifest.Prepare();

            InvokeComplete();
        }
    }


}
