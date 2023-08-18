using System.Collections.Generic;
using UnityEngine;


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
        private int _index;
        private List<AssetsVersionCollection.VersionData> versions = new List<AssetsVersionCollection.VersionData>();

        public override float progress => isDone ? 1 : _progress;
        private float _progress;

        public List<FileData> change;
        public List<FileData> delete;
        public List<FileData> add;
        private string[] tags;
        public VersionCompareOperation(CheckBundleVersionOperation _check, int index, params string[] tags)
        {
            this._check = _check;
            this._index = index;
            this.tags = tags;
            Compare();
        }


        protected virtual async void Compare()
        {
            if (!_check.isErr)
            {
                IAssetStreamEncrypt en = AssetsInternal.GetEncrypt();
                versions = _check.versions;
                _index = Mathf.Clamp(_index, 0, versions.Count - 1);
                var version = versions[_index];

                List<FileData> remoteBundles = new List<FileData>();

                for (int i = 0; i < version.groups.Count; i++)
                {
                    var group = version.groups[i];
                    _progress = i / (float)version.groups.Count;
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
                        string fileName = group.bundleFileName;

                        Downloader downloader = AssetsInternal.DownloadVersion(fileName);
                        await downloader;
                        if (downloader.isErr)
                        {
                            SetErr(downloader.error);
                            break;
                        }
                        else
                        {
                            BundlesVersion v = VersionBuffer.ReadBundleVersion(downloader.data, en);
                            remoteBundles.AddRange(v.bundles);
                        }
                    }
                    {
                        string fileName = group.manifestFileName;
                        Downloader downloader = AssetsInternal.DownloadVersion(fileName);
                        await downloader;
                        if (downloader.isErr)
                        {
                            SetErr(downloader.error);
                            break;
                        }
                        else
                        {
                            ManifestData v = VersionBuffer.ReadManifest(downloader.data, en);
                            await VersionBuffer.WriteManifest(v, AssetsInternal.GetBundleLocalPath(fileName), en);
                        }
                    }

                }
                List<FileData> local = GetLocalBundles();
                FileData.Compare(local, remoteBundles, AssetsInternal.GetFileCheckType(), out change, out delete, out add);
                await VersionBuffer.WriteVersionData(version,
                      AssetsInternal.GetBundleLocalPath(VersionBuffer.localHashName),
                      en
                      );

            }
            InvokeComplete();
        }
    }

}
