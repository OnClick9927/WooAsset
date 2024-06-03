using System.Collections.Generic;
using static WooAsset.FileData;


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
        private readonly VersionCompareType compareType;

        public override float progress => isDone ? 1 : _progress;
        private float _progress;

        public List<BundleData> change;
        public List<string> delete;
        public List<BundleData> add;
        public VersionCompareOperation(VersionData version, List<PackageData> pkgs, VersionCompareType compareType)
        {
            this.pkgs = pkgs;
            this.compareType = compareType;
            this.version = version;
            Compare();
        }



        protected virtual async void Compare()
        {
            List<BundleData> remoteBundles = null;
            for (int i = 0; i < pkgs.Count; i++)
            {
                var pkg = pkgs[i];
                _progress = i / (float)pkgs.Count;
                string fileName = pkg.manifestFileName;
                string local_path = AssetsInternal.GetBundleLocalPath(fileName);

                ManifestData remote_main, local_main = null;
                Downloader downloader = AssetsInternal.DownloadVersion(version.version, fileName);
                await downloader;
                if (downloader.isErr)
                {
                    SetErr(downloader.error);
                    break;
                }
                else
                {
                    remote_main = VersionHelper.ReadManifest(downloader.data);

                    if (compareType != VersionCompareType.Manifest)
                    {
                        if (remoteBundles == null)
                            remoteBundles = new List<BundleData>();
                        remoteBundles.AddRange(remote_main.GetAllBundleData());
                    }
                }
                if (compareType == VersionCompareType.Manifest)
                {

                    if (AssetsHelper.ExistsFile(local_path))
                    {
                        var reader = AssetsHelper.ReadFile(local_path, true);
                        await reader;
                        local_main = VersionHelper.ReadManifest(reader.bytes);
                    }
                    if (add == null)
                        add = new List<BundleData>();
                    if (delete == null) delete = new List<string>();
                    if (change == null) change = new List<BundleData>();
                    if (local_main == null)
                    {
                        add.AddRange(remote_main.GetAllBundleData());
                    }
                    else
                    {
                        if (remote_main.GetVersion() != local_main.GetVersion())
                        {
                            List<BundleData> change;
                            List<BundleData> delete;
                            List<BundleData> add;
                            BundleData.Compare(local_main.GetAllBundleData(), remote_main.GetAllBundleData(), out change, out delete, out add);
                            this.change.AddRange(change);
                            this.add.AddRange(add);
                            for (int j = 0; j < delete.Count; j++)
                                this.delete.Add(delete[j].bundleName);
                        }
                    }
                }
                await VersionHelper.WriteManifest(remote_main, AssetsInternal.GetBundleLocalPath(fileName));

            }
            if (compareType != VersionCompareType.Manifest)
            {
                List<FileData> delete;
                List<FileData> local = GetLocalBundles();
                FileCompareType fileCompareType = compareType == VersionCompareType.FileLength ? FileCompareType.Length : FileCompareType.Hash;
                FileData.Compare(local, remoteBundles, fileCompareType, out change, out delete, out add);
                for (int j = 0; j < delete.Count; j++)
                    this.delete.Add(delete[j].name);
            }

            await VersionHelper.WriteVersionData(version,
                  AssetsInternal.GetBundleLocalPath(VersionHelper.VersionDataName)
                  );
            InvokeComplete();
        }
    }

}
