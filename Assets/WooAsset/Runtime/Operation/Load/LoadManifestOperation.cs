using System.Collections.Generic;
using System.Linq;


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
        private AssetsVersionCollection.VersionData version;
        public ManifestData manifest;
        public List<FileData> bundles;

        private string _version = "";
        private string[] tags;
        private List<string> loadedBundles;
        public LoadManifestOperation(List<string> loadedBundles, string version, string[] tags)
        {
            _version = version;
            this.tags = tags;
            this.loadedBundles = loadedBundles;
            Done();
        }




        private async void Done()
        {
            IAssetStreamEncrypt en = AssetsInternal.GetEncrypt();
            string localVersionPath = AssetsInternal.GetBundleLocalPath(VersionBuffer.localHashName);
            bool download = false;
            if (!AssetsHelper.ExistsFile(localVersionPath))
                download = true;
            else
            {
                var reader = await AssetsHelper.ReadFile(localVersionPath, true);
                version = VersionBuffer.ReadVersionData(reader.bytes, en);

                if (!string.IsNullOrEmpty(_version))
                    if (version.version != _version)
                        download = true;
            }
            if (download)
            {
                downloader = AssetsInternal.DownloadVersion(VersionBuffer.remoteHashName);

                await downloader;
                if (downloader.isErr)
                {
                    SetErr(downloader.error);
                }
                else
                {
                    AssetsVersionCollection collection = VersionBuffer.ReadAssetsVersionCollection(downloader.data, en);
                    AssetsVersionCollection.VersionData find = null;
                    if (!string.IsNullOrEmpty(_version))
                        find = collection.versions.Find(x => x.version == _version);
                    if (find == null)
                        find = collection.versions.Last();
                    version = find;
                    await VersionBuffer.WriteVersionData(version, localVersionPath, en);
                }
            }
            List<ManifestData> manifests = new List<ManifestData>();
            for (int i = 0; i < version.groups.Count; i++)
            {
                var group = version.groups[i];
                bool go = false;
                if (tags != null && tags.Length != 0)
                {
                    for (int j = 0; j < tags.Length; j++)
                    {
                        if (group.tags.Contains(tags[j]))
                        {
                            go = true;
                            break;
                        }
                    }
                }
                else
                {
                    go = true;
                }
                if (!go) continue;

                {
                    string fileName = group.manifestFileName;

                    string _path = AssetsInternal.GetBundleLocalPath(fileName);
                    ManifestData v = null;
                    if (AssetsHelper.ExistsFile(_path))
                    {
                        var reader = await AssetsHelper.ReadFile(_path, true);
                        v = VersionBuffer.ReadManifest(reader.bytes, en);
                    }
                    else
                    {
                        downloader = AssetsInternal.DownloadVersion(fileName);
                        await downloader;
                        if (downloader.isErr)
                        {
                            SetErr(downloader.error);
                            break;
                        }
                        v = VersionBuffer.ReadManifest(downloader.data, en);
                        await VersionBuffer.WriteManifest(v, _path, en);
                    }
                    manifests.Add(v);
                }
            }

            manifest = ManifestData.Merge(manifests, this.loadedBundles);
            manifest.Prepare();

            InvokeComplete();
        }
    }


}
