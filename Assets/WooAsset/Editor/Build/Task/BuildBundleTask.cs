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
            protected async override void OnExecute(AssetTaskContext context)
            {
                var source = context.allBundleGroups;
                if (source.Count != 0)
                {
                    BuildPipeline.BuildAssetBundles(context.historyPath,
                       source.ConvertAll(x => x.ToAssetBundleBuild()).ToArray(), context.BuildOption, context.buildTarget);
                }
                var manifest = FastModeManifestTask.BuildManifest(source,context.tree);
                context.manifest = manifest;
                foreach (var bundleName in source.ConvertAll(x => x.hash))
                {
                    var reader = await AssetsHelper.ReadFile(AssetsHelper.CombinePath(context.historyPath, bundleName), true);
                    await AssetsHelper.WriteFile(
                          EncryptBuffer.Encode(bundleName, reader.bytes, context.encrypt),
                          AssetsHelper.CombinePath(context.outputPath, bundleName),
                          true
                          );
                }
                var bVer = new BundlesVersion()
                {
                    version = context.version,
                };
                foreach (var bundle in manifest.allBundle)
                {
                    string path = AssetsHelper.CombinePath(context.outputPath, bundle);
                    if (AssetsHelper.ExistsFile(path))
                        bVer.bundles.Add(FileData.CreateByFile(path));
                    else
                    {
                        this.SetErr($"can't find last bundle version {bundle}");
                        InvokeComplete();
                        return;
                    }
                }
                await VersionBuffer.WriteManifest(manifest,
                        AssetsHelper.CombinePath(context.outputPath,
                        context.buildPkg.GetManifestFileName(context.version)),
                        context.encrypt
                        );
                await VersionBuffer.WriteBundlesVersion(bVer,
                        AssetsHelper.CombinePath(context.outputPath,
                        context.buildPkg.GetBundleFileName(context.version)),
                        context.encrypt
                        );
                await VersionBuffer.WriteManifest(manifest,
                        AssetsHelper.CombinePath(context.historyPath,
                        context.buildPkg.GetManifestFileName(context.version)),
                        new NoneAssetStreamEncrypt()
                );
                await VersionBuffer.WriteBundlesVersion(bVer,
                        AssetsHelper.CombinePath(context.historyPath,
                        context.buildPkg.GetBundleFileName(context.version)),
                        new NoneAssetStreamEncrypt()
                        );
                InvokeComplete();
            }
        }



        private List<AssetTask> tasks = new List<AssetTask>()
        {
            new CollectAssetsTask(),
            new CollectHashBundleGroupTask(),
            new BuildTask(),
        };


        protected override async void OnExecute(AssetTaskContext context)
        {
            context.files = AssetsHelper.GetDirectoryFiles(context.outputPath).ToList().ConvertAll(x => FileData.CreateByFile(x));
            List<string> useful = new List<string>();
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

                context.exports.Add(new GroupExportData()
                {
                    buildPkg = context.buildPkg,
                    manifest = context.manifest,
                });

                useful.AddRange(context.manifest.allBundle);
            }

            var versions = context.versions;
            if (versions.FindVersion(context.version) == null)
            {
                var versionData = new AssetsVersionCollection.VersionData()
                {
                    version = context.version,
                };
                versionData.SetPkgs(context.buildPkgs.ConvertAll(x => new AssetsVersionCollection.VersionData.PackageData()
                {
                    bundleFileName = x.GetBundleFileName(context.version),
                    manifestFileName = x.GetManifestFileName(context.version),
                    name = x.name,
                    description = x.description,
                    tags = x.tags,
                }));
                versions.AddVersion(versionData);
            }
            await VersionBuffer.WriteAssetsVersionCollection(
                     versions,
                     context.historyVersionFilePath,
                     new NoneAssetStreamEncrypt());
            var outputVersions = JsonUtility.FromJson<AssetsVersionCollection>(JsonUtility.ToJson(versions));
            outputVersions.RemoveFirstIFTooLarge(context.MaxCacheVersionCount);
       

            context.outputVersions = outputVersions;
            await VersionBuffer.WriteAssetsVersionCollection(
                      outputVersions,
                      AssetsHelper.CombinePath(context.outputPath, context.remoteHashName),
                      context.encrypt);

            useful.Add(context.remoteHashName);
            var groups = context.versions.NewestVersion().GetAllPkgs();
            useful.AddRange(groups.ConvertAll(x => x.manifestFileName));
            useful.AddRange(groups.ConvertAll(x => x.bundleFileName));
            context.useful = useful;




            InvokeComplete();
        }
    }
}
