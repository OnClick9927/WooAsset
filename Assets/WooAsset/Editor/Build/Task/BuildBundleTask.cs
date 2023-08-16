using System.Collections.Generic;
using System.Linq;
using static WooAsset.ManifestData;
using UnityEditor;
using static WooAsset.AssetsHelper;

namespace WooAsset
{
    public class BuildBundleTask : AssetTask
    {

        public class BuildTask : AssetTask
        {
            protected override void OnExecute(AssetTaskContext context)
            {
                var source = context.allBundleGroups;

                if (source.Count != 0)
                {
                    BuildPipeline.BuildAssetBundles(context.historyPath,
                       source.ConvertAll(x => x.ToAssetBundleBuild()).ToArray(), context.BuildOption, context.buildTarget);
                }

                List<AssetData> _assets = new List<AssetData>();
                foreach (var build in source)
                {
                    string bundleName = build.hash;
                    foreach (var assetPath in build.GetAssets())
                    {
                        _assets.Add(new AssetData()
                        {
                            path = assetPath,
                            bundleName = bundleName,
                            dps = context.tree.GetAssetData(assetPath).dependence,
                            tags = context.tags.GetAssetTags(assetPath).ToList(),
                            type = context.tree.GetAssetData(assetPath).type,

                        });
                    }
                }


                ManifestData manifest = new ManifestData();
                manifest.Read(_assets, context.rawAssets, context.rawAssets_copy);
                manifest.Prepare();
                context.manifest = manifest;

                InvokeComplete();
            }
        }

        public class EncryptTask : AssetTask
        {
            protected override async void OnExecute(AssetTaskContext context)
            {

                foreach (var bundleName in context.allBundleGroups.ConvertAll(x => x.hash))
                {
                    var reader = await AssetsHelper.ReadFile(AssetsHelper.CombinePath(context.historyPath, bundleName), true);
                    await AssetsHelper.WriteFile(
                          EncryptBuffer.Encode(bundleName, reader.bytes, context.encrypt),
                          AssetsHelper.CombinePath(context.outputPath, bundleName),
                          true
                          );



                }
                InvokeComplete();
            }
        }
        public class BuildVersionTask : AssetTask
        {
            private async void WriteVersion(AssetTaskContext context)
            {
                var bVer = new BundlesVersion()
                {
                    version = context.version,
                };
                foreach (var bundle in context.manifest.allBundle)
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
                await VersionBuffer.WriteManifest(context.manifest,
                        AssetsHelper.CombinePath(context.outputPath,
                        context.buildGroup.GetManifestFileName(context.version)),
                        context.encrypt
                        );
                await VersionBuffer.WriteBundlesVersion(bVer,
                        AssetsHelper.CombinePath(context.outputPath,
                        context.buildGroup.GetBundleFileName(context.version)),
                        context.encrypt
                        );
            }
            protected override async void OnExecute(AssetTaskContext context)
            {
                if (context.versions.versions.Find(x => x.version == context.version) == null)
                {
                    context.versions.versions.Add(new AssetsVersionCollection.VersionData()
                    {
                        version = context.version,
                        groups = context.buildGroups.ConvertAll(x => new AssetsVersionCollection.VersionData.Group()
                        {
                            bundleFileName = x.GetBundleFileName(context.version),
                            manifestFileName = x.GetManifestFileName(context.version),
                            name = x.name,
                            description = x.description,
                            tags = x.tags,
                        })
                    });
                }
                await VersionBuffer.WriteAssetsVersionCollection(
                         context.versions,
                         AssetsHelper.CombinePath(context.historyPath, context.remoteHashName),
                         new NoneAssetStreamEncrypt());
                await VersionBuffer.WriteAssetsVersionCollection(
                          context.versions,
                          AssetsHelper.CombinePath(context.outputPath, context.remoteHashName),
                          context.encrypt);
                WriteVersion(context);
                InvokeComplete();
            }
        }


        private List<AssetTask> tasks = new List<AssetTask>()
        {
            new CollectAssetsTask(),
            new CollectHashBundleGroupTask(),
            new BuildTask(),
            new EncryptTask(),
            new BuildVersionTask(),
        };

        protected override async void OnExecute(AssetTaskContext context)
        {
            context.files = AssetsHelper.GetDirectoryFiles(context.outputPath).ToList().ConvertAll(x => FileData.CreateByFile(x));
            List<string> allBundle = new List<string>();
            var builds = context.buildGroups.FindAll(x => x.build);
            if (builds.Count == 0)
            {
                SetErr("Nothing To Build");
                InvokeComplete();
                return;
            }


            for (int i = 0; i < builds.Count; i++)
            {
                var group = builds[i];
                context.buildGroup = group;
                for (int j = 0; j < tasks.Count; j++)
                    await Execute(tasks[j], context);
                allBundle.AddRange(context.manifest.allBundle);
            }
            allBundle.Add(context.remoteHashName);
            allBundle.AddRange(context.versions.versions.Last().groups.ConvertAll(x => x.manifestFileName));

            allBundle.AddRange(context.versions.versions.Last().groups.ConvertAll(x => x.bundleFileName));

            context.useful = allBundle;

            InvokeComplete();
        }
    }
}
