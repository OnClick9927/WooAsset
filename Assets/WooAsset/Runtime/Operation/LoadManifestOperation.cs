using System;
using System.Collections.Generic;


namespace WooAsset
{
    class LoadManifestOperation : Operation
    {
        public override float progress => isDone ? 1 : _progress;
        private float _progress;
        private DownLoader downloader;
        private VersionData version;
        public ManifestData manifest;
        public List<FileData> bundles;
        private bool fuzzySearch;
        private string _version = "";
        private Func<VersionData, List<PackageData>> getPkgs;
        private List<string> loadedBundles;

        public string GetVersion() => version.version;
        public LoadManifestOperation(List<string> loadedBundles, string version, bool fuzzySearch, Func<VersionData, List<PackageData>> getPkgs)
        {
            this.fuzzySearch = fuzzySearch;
            _version = version;
            this.getPkgs = getPkgs;
            this.loadedBundles = loadedBundles;
            Done();
        }




        private async void Done()
        {
            _progress = 0f;
            string localVersionPath = AssetsInternal.GetBundleLocalPath(VersionHelper.VersionDataName);
            bool download = AssetsInternal.GetBundleAlwaysFromWebRequest() || !AssetsHelper.ExistsFile(localVersionPath);
            if (!download && !string.IsNullOrEmpty(_version))
            {
                var reader = await AssetsHelper.ReadFile(localVersionPath, true) as ReadFileOperation;
                if (VersionHelper.ReadVersionData(reader.bytes).version != _version)
                    download = true;
            }
            _progress = 0.1f;
            if (download)
            {
                downloader = await AssetsInternal.DownloadRemoteVersion() as DownLoader;
                if (downloader.isErr)
                {
                    SetErr(downloader.error);
                    version = new VersionData() { version = _version };
                }
                else
                {
                    VersionCollectionData collection = VersionHelper.ReadAssetsVersionCollection(downloader.data);
                    _version = collection.FindVersion(_version);
                    if (string.IsNullOrEmpty(_version)) _version = collection.NewestVersion();
                    var _op = await AssetsInternal.DownloadVersionData(_version);
                    version = _op.GetVersion();
                    if (AssetsInternal.GetSaveBundlesWhenPlaying())
                        await VersionHelper.WriteVersionData(version, localVersionPath);
                }
            }
            else
            {
                var op = await AssetsHelper.ReadFile(localVersionPath, true) as ReadFileOperation;
                version = VersionHelper.ReadVersionData(op.bytes);
            }
            _progress = 0.3f;
            var pkgs = this.getPkgs == null ? version.GetAllPkgs() : this.getPkgs.Invoke(version);

            manifest = new ManifestData();
            for (int i = 0; i < pkgs.Count; i++)
            {
                var pkg = pkgs[i];
                string fileName = pkg.manifestFileName;
                string localPath = AssetsInternal.GetBundleLocalPath(fileName);
                bool _download = AssetsInternal.GetBundleAlwaysFromWebRequest() || !AssetsHelper.ExistsFile(localPath);
                ManifestData v;
                if (_download)
                {
                    downloader = await AssetsInternal.DownloadVersion(version.version, fileName) as DownLoader;
                    if (downloader.isErr)
                    {
                        SetErr(downloader.error);
                        break;
                    }
                    v = VersionHelper.ReadManifest(downloader.data);
                    if (AssetsInternal.GetSaveBundlesWhenPlaying())
                        await VersionHelper.WriteManifest(v, localPath);
                }
                else
                {
                    var reader = await AssetsHelper.ReadFile(localPath, true) as ReadFileOperation;
                    v = VersionHelper.ReadManifest(reader.bytes);
                }
                ManifestData.Merge(v, manifest, this.loadedBundles);
                _progress = 0.5f + i / pkgs.Count / 2f;
            }

            manifest.Prepare(fuzzySearch);
            InvokeComplete();
        }
    }


}
