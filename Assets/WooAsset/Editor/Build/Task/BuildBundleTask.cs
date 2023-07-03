using System.Collections.Generic;
using System.IO;
using System.Linq;
using static WooAsset.ManifestData;
using UnityEditor;

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
                            dps = context.tree.GetAssetData(assetPath).dps,
                            tags = context.tags.GetAssetTags(assetPath).ToList(),
                            type = context.tree.GetAssetData(assetPath).type,

                        });
                    }
                }


                ManifestData manifest = new ManifestData();
                manifest.Read(_assets, context.rawAssets);
                manifest.Prepare();
                context.manifest = manifest;

                InvokeComplete();
            }
        }

        public class EncryptTask : AssetTask
        {
            protected override void OnExecute(AssetTaskContext context)
            {

                foreach (var bundleName in context.allBundleGroups.ConvertAll(x => x.hash))
                {
                    byte[] buffer = File.ReadAllBytes(AssetsInternal.CombinePath(context.historyPath, bundleName));
                    File.WriteAllBytes(AssetsInternal.CombinePath(context.outputPath, bundleName), EncryptBuffer.Encode(bundleName, buffer, context.encrypt));
                }
                InvokeComplete();
            }
        }
        public class BuildVersionTask : AssetTask
        {
            private void WriteVersion(AssetTaskContext context)
            {
                var bVer = new BundlesVersion()
                {
                    version = context.version,
                };
                foreach (var bundle in context.manifest.allBundle)
                {
                    string path = AssetsInternal.CombinePath(context.outputPath, bundle);
                    if (File.Exists(path))
                        bVer.bundles.Add(FileData.CreateByFile(path));
                    else
                    {
                        this.SetErr($"can't find last bundle version {bundle}");
                        InvokeComplete();
                        return;
                    }
                }
                VersionBuffer.WriteManifest(context.manifest,
                      AssetsInternal.CombinePath(context.outputPath, 
                      context.buildGroup.GetManifestFileName(context.version)),
                      context.encrypt
                      );
                VersionBuffer.WriteBundlesVersion(bVer,
                      AssetsInternal.CombinePath(context.outputPath, 
                      context.buildGroup.GetBundleFileName(context.version)),
                      context.encrypt
                      );
            }
            protected override void OnExecute(AssetTaskContext context)
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
                VersionBuffer.WriteAssetsVersionCollection(
                     context.versions,
                     AssetsInternal.CombinePath(context.historyPath, context.remoteHashName),
                     new NoneAssetStreamEncrypt());
                VersionBuffer.WriteAssetsVersionCollection(
                     context.versions,
                     AssetsInternal.CombinePath(context.outputPath, context.remoteHashName),
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
            context.files = Directory.GetFiles(context.outputPath).ToList().ConvertAll(x => FileData.CreateByFile(x));
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
