
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static WooAsset.AssetsSetting;

namespace WooAsset
{
    partial class AssetsInternal
    {
        public class CheckBundleVersionOperation : AssetOperation
        {
            private float _progress;
            public override float progress
            {
                get
                {
                    if (isDone) return 1;
                    return _progress * 0.5f + downloader.progress * 0.5f;
                }
            }
            private string url;
            public List<AssetsVersion.VersionData> downLoadOnes { get; private set; }
            public List<string> unUseBundles { get; private set; }

            public CheckBundleVersionOperation()
            {
                this.url = AssetsInternal.GetVersionUrl();
                downLoadOnes = new List<AssetsVersion.VersionData>();
                Done();
            }

            private Downloader downloader;
            private async void Done()
            {
                downloader = new Downloader(url);
                await downloader.Start();
                if (downloader.isError)
                {
                    SetErr(downloader.error);
                }
                else
                {
                    unUseBundles = new List<string>(AssetsInternal.GetLocalBundles());
                    var bytes = GetEncrypt().DeCode(AssetsInternal.GetNameHash(AssetsVersion.versionName), downloader.data);
                    AssetsVersion remote = JsonUtility.FromJson<AssetsVersion>(AssetsVersion.encoding.GetString(bytes));
                    for (int i = 0; i < remote.data_list.Count; i++)
                    {
                        _progress = i / (float)remote.data_list.Count;
                        var item = remote.data_list[i];
                        var bundleName = item.bundleName;
                        var localPath = ToRegularPath(AssetsInternal.GetBundleLocalPath(bundleName));
                        if (!File.Exists(localPath)) downLoadOnes.Add(item);
                        else
                        {
                            if (unUseBundles.Contains(localPath))
                                unUseBundles.Remove(localPath);
                            FileCheckType type = AssetsInternal.GetFileCheckType();
                            if (type == FileCheckType.MD5)
                            {
                                var md5 = item.md5;
                                var localMD5 = AssetsInternal.GetFileHash(localPath);
                                if (localMD5 != md5)
                                    downLoadOnes.Add(item);
                            }
                            else
                            {
                                var length = item.length;
                                FileInfo file = new FileInfo(localPath);
                                var localLen = file.Length;
                                if (length != localLen)
                                    downLoadOnes.Add(item);
                            }

                        }
                    }
                }
                InvokeComplete();
            }
        }
    }
}
