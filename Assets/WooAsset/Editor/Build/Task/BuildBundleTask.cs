using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    public class BuildBundleTask : AssetTask
    {

        public class BuildTask : AssetTask
        {
            private static void UpdateHash(List<EditorBundleData> builds, AssetBundleManifest _main)
            {
                var bundles = _main.GetAllAssetBundles().ToList();
                for (int i = 0; i < builds.Count; i++)
                {
                    var build = builds[i];
                    var find = bundles.FirstOrDefault(a => a.StartsWith(build.hash));
                    build.SyncRealHash(find);
                }


                foreach (EditorBundleData build in builds)
                    build.SetDependence(_main.GetAllDependencies(build.hash).ToList());
                foreach (EditorBundleData build in builds)
                    build.FindUsage(builds);
            }
            protected async override void OnExecute(AssetTaskContext context)
            {
                var source = context.allBundleBuilds;
                var normal = source.FindAll(x => !x.raw);
                var raws = source.FindAll(x => x.raw);


                if (normal.Count != 0)
                {
                    var assetbuilds = normal.ConvertAll(x => x.ToAssetBundleBuild()).ToArray();
                    AssetBundleManifest _main = BuildPipeline.BuildAssetBundles(context.historyPath,
                        assetbuilds, context.BuildOption, context.buildTarget);
                    UpdateHash(normal, _main);
                }
                if (context.bundleNameType == BundleNameType.NameWithHash)
                {
                    for (int i = 0; i < raws.Count; i++)
                    {
                        var bundle = raws[i];
                        var hash = bundle.hash;
                        bundle.SyncRealHash($"{hash}_{hash}");
                    }
                }
                Dictionary<string, string> needRenameFiles = new Dictionary<string, string>();
                if (context.bundleNameType == BundleNameType.Hash)
                {
                    foreach (var bundle in source)
                    {
                        if (!bundle.raw)
                        {
                            var bundleName = bundle.hash;
                            string hash = bundleName.Split('_')[1];
                            foreach (var item in source)
                                item.ReplaceDpendenceHash(bundleName, hash);
                            bundle.SyncRealHash(hash);
                            needRenameFiles.Add(bundleName, hash);
                        }
                    }
                }


                var manifest = FastModeManifestTask.BuildManifest(source, context.assetsCollection, context.assetBuild);
                context.manifest = manifest;
                if (context.isNormalBuildMode)
                {
                    //把raw存到history
                    foreach (var item in raws)
                    {
                        string src_path = item.GetAssets()[0];
                        string bundleName = item.hash;
                        var reader = AssetsHelper.ReadFile(src_path, true);
                        await reader;
                        string dest = AssetsHelper.CombinePath(context.historyPath, bundleName);
                        await AssetsHelper.WriteFile(reader.bytes, dest, true);

                    }
                    if (needRenameFiles.Count > 0)
                    {
                        foreach (var src in needRenameFiles.Keys)
                        {
                            var dest = needRenameFiles[src];
                            //System.IO.File.Copy(AssetsHelper.CombinePath(context.historyPath, src), AssetsHelper.CombinePath(context.historyPath, dest));
                            var reader = AssetsHelper.ReadFile(AssetsHelper.CombinePath(context.historyPath, src), true);
                            await reader;
                            await AssetsHelper.WriteFile(reader.bytes, AssetsHelper.CombinePath(context.historyPath, dest), true);
                        }
                    }

                    //拷贝打爆出来的到输出目录
                    foreach (var bundle in source)
                    {
                        var bundleName = bundle.hash;
                        var reader = AssetsHelper.ReadFile(AssetsHelper.CombinePath(context.historyPath, bundleName), true);
                        await reader;
                        await AssetsHelper.WriteFile(
                              EncryptBuffer.Encode(bundleName, reader.bytes, context.assetBuild.GetEncryptByCode(bundle.GetEncryptCode())),
                              AssetsHelper.CombinePath(context.outputPath, bundleName),
                              true
                              );
                    }

                    var bVer = new BundlesVersionData()
                    {
                        version = context.version,
                    };
                    foreach (var bundleName in manifest.allBundle)
                    {
                        string path = AssetsHelper.CombinePath(context.outputPath, bundleName);
                        if (AssetsHelper.ExistsFile(path))
                        {
                            var data = FileData.CreateByFile(path);
                            bVer.bundles.Add(data.ToBundleFileData());
                        }
                        else
                        {
                            this.SetErr($"can't find last bundle version {bundleName}");
                            InvokeComplete();
                            return;
                        }
                    }

                    var mainfestName = VersionHelper.GetManifestFileName(context.buildPkg.name);
                    var bundleFileName = VersionHelper.GetBundleFileName(context.buildPkg.name);

                    await VersionHelper.WriteManifest(manifest,
                            AssetsHelper.CombinePath(context.outputPath,
                            mainfestName));
                    await VersionHelper.WriteBundlesVersion(bVer,
                            AssetsHelper.CombinePath(context.outputPath, bundleFileName));
                }

                InvokeComplete();
            }
        }



        private List<AssetTask> tasks = new List<AssetTask>()
        {
            new CollectAssetsTask(),
            new CollectHashBundleGroupTask(),
            new BuildTask(),
        };

        private static VersionCollectionData DeepCopy(VersionCollectionData src)
        {
            return AssetsHelper.ReadFromBytes<VersionCollectionData>(AssetsHelper.ObjectToBytes(src));
        }
        protected override async void OnExecute(AssetTaskContext context)
        {
            AssetsEditorTool.DeleteDirectory(context.outputPath);
            AssetsHelper.CreateDirectory(context.outputPath);

            var builds = context.buildPkgs.FindAll(x => x.build);
            if (builds.Count == 0)
            {
                SetErr("Nothing To Build");
                InvokeComplete();
                return;
            }


            for (int i = 0; i < builds.Count; i++)
            {
                var group = builds[i];
                context.buildPkg = group;
                for (int j = 0; j < tasks.Count; j++)
                    await Execute(tasks[j], context);

                context.exports.Add(new PackageExportData()
                {
                    pkg = context.buildPkg.ToPackageData(),
                    manifest = context.manifest,
                    encrypt = context.encrypt.ToString(),
                    version = context.version,
                    compress = context.compress.ToString(),
                    typeTreeOption = context.typeTreeOption,
                });
            }

            if (context.isNormalBuildMode)
            {
                var versions = context.historyVersions;

                if (versions.FindVersion(context.version) == null)
                    versions.AddVersion(context.version);

                await VersionHelper.WriteAssetsVersionCollection(
                         versions,
                         context.historyVersionPath);


                var outputVersions = DeepCopy(versions);
                outputVersions.RemoveFirstIFTooLarge(context.MaxCacheVersionCount);


                context.outputVersions = outputVersions;
                await VersionHelper.WriteAssetsVersionCollection(
                          outputVersions,
                          AssetsHelper.CombinePath(context.outputPath, context.VersionCollectionName));
                VersionData versionData = versionData = new VersionData()
                {
                    version = context.version,
                };
                versionData.SetPkgs(context.buildPkgs.ConvertAll(x => x.ToPackageData()));

                await VersionHelper.WriteVersionData(
                     versionData,
                     AssetsHelper.CombinePath(context.outputPath, context.VersionDataName));
            }

            InvokeComplete();
        }
    }
}
