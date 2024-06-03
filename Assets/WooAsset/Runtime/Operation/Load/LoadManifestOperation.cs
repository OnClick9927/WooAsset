using System;
using System.Collections.Generic;


namespace WooAsset
{
    public class LoadManifestOperation : Operation
    {
        public override float progress => isDone ? 1 : _progress;
        private float _progress;
        private Downloader downloader;
        private VersionData version;
        public ManifestData manifest;
        public List<FileData> bundles;

        private string _version = "";
        private Func<VersionData, List<PackageData>> getPkgs;
        private List<string> loadedBundles;

        public string GetVersion() => version.version;
        public LoadManifestOperation(List<string> loadedBundles, string version, Func<VersionData, List<PackageData>> getPkgs)
        {
            _version = version;
            this.getPkgs = getPkgs;
            this.loadedBundles = loadedBundles;
            Done();
        }




        private async void Done()
        {
            _progress = 0f;
            string localVersionPath = AssetsInternal.GetBundleLocalPath(VersionHelper.VersionDataName);
            bool download = false;
            if (!AssetsHelper.ExistsFile(localVersionPath))
                download = true;
            else
            {
                var reader = AssetsHelper.ReadFile(localVersionPath, true);
                await reader;
                version = VersionHelper.ReadVersionData(reader.bytes);

                if (!string.IsNullOrEmpty(_version))
                    if (version.version != _version)
                        download = true;
            }
            _progress = 0.2f;


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
                    VersionCollectionData collection = VersionHelper.ReadAssetsVersionCollection(downloader.data);
                    _version = string.IsNullOrEmpty(_version) ? collection.NewestVersion() : collection.FindVersion(_version);
                    var _op = AssetsInternal.DonloadVersionData(_version);
                    await _op;
                    version = _op.GetVersion();
                    await VersionHelper.WriteVersionData(version, localVersionPath);
                }
            }
            _progress = 0.5f;
            var pkgs = this.getPkgs == null ? version.GetAllPkgs() : this.getPkgs.Invoke(version);

            manifest = new ManifestData();
            for (int i = 0; i < pkgs.Count; i++)
            {
                var pkg = pkgs[i];
                string fileName = pkg.manifestFileName;

                string _path = AssetsInternal.GetBundleLocalPath(fileName);
                ManifestData v;
                if (AssetsHelper.ExistsFile(_path))
                {
                    var reader = AssetsHelper.ReadFile(_path, true);
                    await reader;
                    v = VersionHelper.ReadManifest(reader.bytes);
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
                    v = VersionHelper.ReadManifest(downloader.data);
                    await VersionHelper.WriteManifest(v, _path);
                }

                ManifestData.Merge(v, manifest, this.loadedBundles);
                _progress = 0.5f + i / pkgs.Count / 2f;
            }

            manifest.Prepare();
            InvokeComplete();
        }
    }


}
