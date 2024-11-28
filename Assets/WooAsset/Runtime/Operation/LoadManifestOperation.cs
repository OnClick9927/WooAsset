using System;
using System.Collections.Generic;


namespace WooAsset
{
    class LoadManifestOperation : Operation
    {
        public override float progress => isDone ? 1 : _progress;
        private float _progress;
        public ManifestData manifest;
        private string initVersion = "";


        private bool fuzzySearch;
        private Func<VersionData, List<PackageData>> getPkgs;
        private List<string> loadedBundles;

        private void SetResult(ManifestData manifest, string version)
        {
            this.initVersion = version;
            this.manifest = manifest;
            AssetsHelper.Log($"init by version {version}");
            InvokeComplete();
        }
        private void ExitErr(string version, string err)
        {
            this.initVersion = version;
            SetErr(err);
            InvokeComplete();
        }
        public string GetVersion() => initVersion;
        public LoadManifestOperation(List<string> loadedBundles, string version, bool fuzzySearch, Func<VersionData, List<PackageData>> getPkgs)
        {
            this.fuzzySearch = fuzzySearch;
            this.getPkgs = getPkgs;
            this.loadedBundles = loadedBundles;
            Done(version);
        }


        private async void DoPkgs(VersionData version, bool AlwaysFromWebRequest)
        {
            _progress = 0.3f;
            var pkgs = this.getPkgs == null ? version.GetAllPkgs() : this.getPkgs.Invoke(version);

            var _manifest = new ManifestData();
            for (int i = 0; i < pkgs.Count; i++)
            {
                var pkg = pkgs[i];
                string fileName = pkg.manifestFileName;
                string localPath = AssetsInternal.GetBundleLocalPath(fileName);
                bool _download = AlwaysFromWebRequest || !AssetsHelper.ExistsFile(localPath);
                ManifestData v;
                if (_download)
                {
                    var downloader = await AssetsInternal.DownloadVersion(version.version, fileName) as DownLoader;
                    if (downloader.isErr)
                    {
                        SetErr(downloader.error);
                        break;
                    }
                    v = AssetsHelper.ReadBufferObject<ManifestData>(downloader.data);
                    if (AssetsInternal.GetSaveBytesWhenPlaying())
                        await AssetsHelper.WriteBufferObject(v, localPath);
                }
                else
                {
                    var reader = await AssetsHelper.ReadFile(localPath, true) as ReadFileOperation;
                    v = AssetsHelper.ReadBufferObject<ManifestData>(reader.bytes);
                }
                ManifestData.Merge(v, _manifest, this.loadedBundles);
                _progress = 0.5f + i / pkgs.Count / 2f;
            }

            _manifest.Prepare(fuzzySearch);
            SetResult(_manifest, version.version);
        }

        private async void LoadVersion(string localVersionPath, string targetVersion, bool AlwaysFromWebRequest)
        {
            var _op = await AssetsInternal.DownloadVersionData(targetVersion);
            if (_op.isErr)
                ExitErr(targetVersion, $"can not download VersionData with {targetVersion}");
            else
            {
                var version = _op.GetVersion();
                if (AssetsInternal.GetSaveBytesWhenPlaying())
                    await AssetsHelper.WriteBufferObject(version, localVersionPath);
                DoPkgs(version, AlwaysFromWebRequest);
            }
        }

        private async void CheckVersionLegal(string localVersionPath, string targetVersion, bool AlwaysFromWebRequest)
        {
            var remote = await AssetsInternal.LoadRemoteVersions();
            if (remote.isErr)
            {
                ExitErr(targetVersion, " can not load VersionCollection");
            }
            else
            {

                VersionCollectionData collection = remote.Versions;
                targetVersion = collection.FindVersion(targetVersion);
                if (string.IsNullOrEmpty(targetVersion))
                {
                    AssetsHelper.Log($"target version:{targetVersion} not exist ,so newest:{collection.NewestVersion()}");
                    targetVersion = collection.NewestVersion();
                }

                LoadVersion(localVersionPath, targetVersion, AlwaysFromWebRequest);
            }
        }
        private void DownLoad(string localVersionPath, string targetVersion, bool AlwaysFromWebRequest)
        {
            if (string.IsNullOrEmpty(targetVersion))
            {
                CheckVersionLegal(localVersionPath, targetVersion, AlwaysFromWebRequest);
            }
            else if (AssetsInternal.CheckVersionByVersionCollection())
            {
                CheckVersionLegal(localVersionPath, targetVersion, AlwaysFromWebRequest);
            }
            else
            {
                LoadVersion(localVersionPath, targetVersion, AlwaysFromWebRequest);
            }
        }
        private async void FromLocal(string localVersionPath, string targetVersion, bool AlwaysFromWebRequest)
        {
            var op = await AssetsHelper.ReadFile(localVersionPath, true);
            var version = AssetsHelper.ReadBufferObject<VersionData>(op.bytes);
            if (!string.IsNullOrEmpty(targetVersion) && version.version != targetVersion)
            {
                AssetsHelper.Log($"Local version wrong local:{version.version} target:{targetVersion}");
                DownLoad(localVersionPath, targetVersion, AlwaysFromWebRequest);
            }
            else
                DoPkgs(version, AlwaysFromWebRequest);
        }
        private void Done(string targetVersion)
        {
            _progress = 0f;
            string localVersionPath = AssetsInternal.GetBundleLocalPath(AssetsHelper.VersionDataName);
            bool AlwaysFromWebRequest = AssetsInternal.GetBundleAlwaysFromWebRequest();
            bool download = AlwaysFromWebRequest;
            if (!download)
                if (!AssetsHelper.ExistsFile(localVersionPath))
                    download = true;

            AssetsHelper.Log($"LoadManifest AlwaysFromWebRequest:{AlwaysFromWebRequest}  download:{download}");
            _progress = 0.1f;
            if (download)
                DownLoad(localVersionPath, targetVersion, AlwaysFromWebRequest);
            else
                FromLocal(localVersionPath, targetVersion, AlwaysFromWebRequest);
        }
    }


}
