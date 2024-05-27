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
                if (context.bundleNameType == AssetsBuildOption.BundleNameType.NameWithHash)
                {
                    for (int i = 0; i < raws.Count; i++)
                    {
                        var bundle = raws[i];
                        var hash = bundle.hash;
                        bundle.SyncRealHash($"{hash}_{hash}");
                    }
                }


                var manifest = FastModeManifestTask.BuildManifest(source, context.assetsCollection);
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

                    var bVer = new BundlesVersion()
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
                            mainfestName),
                            context.encrypt
                            );
                    await VersionHelper.WriteBundlesVersion(bVer,
                            AssetsHelper.CombinePath(context.outputPath, bundleFileName),
                            context.encrypt
                            );
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

        private static AssetsVersionCollection DeepCopy(AssetsVersionCollection src)
        {
            return AssetsHelper.ReadFromBytes<AssetsVersionCollection>(AssetsHelper.ObjectToBytes(src));
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
                {
                    var versionData = new AssetsVersionCollection.VersionData()
                    {
                        version = context.version,
                    };
                    versionData.SetPkgs(context.buildPkgs.ConvertAll(x => x.ToPackageData()));
                    versions.AddVersion(versionData);
                }
                await VersionHelper.WriteAssetsVersionCollection(
                         versions,
                         context.historyVersionPath,
                         new NoneAssetStreamEncrypt());
                var outputVersions = DeepCopy(versions);
                outputVersions.RemoveFirstIFTooLarge(context.MaxCacheVersionCount);


                context.outputVersions = outputVersions;
                await VersionHelper.WriteAssetsVersionCollection(
                          outputVersions,
                          AssetsHelper.CombinePath(context.outputPath, context.remoteHashName),
                          context.encrypt);

            }

            InvokeComplete();
        }
    }
}
