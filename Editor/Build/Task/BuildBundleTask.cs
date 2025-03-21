using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public class BuildBundleTask : AssetTask
    {

        public class BuildTask : AssetTask
        {
            private static void UpdateHash(List<EditorBundleData> builds, IBuildPipeLine _main, BundleNameType nameType, string bundledir)
            {
                var bundles = _main.GetAllAssetBundles(nameType);
                for (int i = 0; i < builds.Count; i++)
                {
                    var build = builds[i];
                    var find = bundles.FirstOrDefault(a => a.StartsWith(build.hash));
                    build.SyncRealHash(find);
                    build.bundleHash = _main.GetBundleHash(bundledir, build.hash, nameType);
                    build.bundleCrc = _main.GetBundleCrc(bundledir, build.hash, nameType);
                }


                foreach (EditorBundleData build in builds)
                    build.SetDependence(_main.GetAllDependencies(build.hash, nameType));
                foreach (EditorBundleData build in builds)
                    build.FindUsage(builds);
            }
            public static ManifestData BuildManifest(string version, List<EditorBundleData> groups, EditorAssetCollection tree, CompressType compress)
            {
                List<AssetData> _assets = new List<AssetData>();
                List<BundleData> _bundles = new List<BundleData>();

                foreach (var build in groups)
                {
                    _bundles.Add(build.CreateBundleData(compress));
                    foreach (var assetPath in build.GetAssets())
                    {
                        EditorAssetData data = tree.GetAssetData(assetPath);
                        if (data.record)
                            _assets.Add(data.CreateAssetData(build.hash));
                    }
                }
                ManifestData manifest = new ManifestData();
                manifest.Read(version, _assets, _bundles);
                manifest.Prepare(false, FileNameSearchType.FileName);
                return manifest;
            }

            protected override void OnExecute(AssetTaskContext context)
            {
                var source = context.allBundleBuilds[context.buildPkg.name];
                var normal = source.FindAll(x => !x.raw);
                var raws = source.FindAll(x => x.raw);



                Dictionary<string, string> needRenameFiles = new Dictionary<string, string>();
                if (context.Pipeline == TaskPipelineType.BuildBundle || context.Pipeline == TaskPipelineType.DryBuild)
                {
                    if (normal.Count != 0)
                    {
                        var assetbuilds = normal.ConvertAll(x => x.ToAssetBundleBuild()).ToArray();

                        bool succ = context.buildPipe.BuildAssetBundles(context.historyPath,
                                assetbuilds, context.BuildOption, context.buildTarget);
                        if (!succ)
                        {
                            InvokeComplete();
                            return;
                        }

                        UpdateHash(normal, context.buildPipe, context.bundleNameType, context.historyPath);
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
                    if (context.bundleNameType == BundleNameType.Hash)
                    {
                        foreach (var bundle in source)
                        {
                            if (!bundle.raw)
                            {
                                var bundleName = bundle.hash;
                                string hash = bundleName.Split('_')[1];
                                foreach (var item in source)
                                    item.ReplaceDependenceHash(bundleName, hash);
                                bundle.SyncRealHash(hash);
                                needRenameFiles.Add(bundleName, hash);
                            }
                        }
                    }
                }



                var manifest = BuildManifest(context.version, source, context.assetsCollection, context.compress);


                context.manifest = manifest;
                if (context.Pipeline == TaskPipelineType.BuildBundle)
                {
                    //把raw存到history
                    foreach (var item in raws)
                    {
                        string src_path = item.GetAssets()[0];
                        string bundleName = item.hash;
                        string dest = AssetsEditorTool.CombinePath(context.historyPath, bundleName);
                        AssetsEditorTool.CopyFile(src_path, dest);
                    }
                    if (needRenameFiles.Count > 0)
                    {
                        foreach (var src in needRenameFiles.Keys)
                        {
                            var dest = needRenameFiles[src];
                            var destpath = AssetsEditorTool.CombinePath(context.historyPath, dest);
                            if (AssetsEditorTool.ExistsFile(destpath))
                                AssetsEditorTool.DeleteFile(destpath);
                            AssetsEditorTool.MoveFile(
                                AssetsEditorTool.CombinePath(context.historyPath, src),
                              destpath);
                        }
                    }

                    //拷贝打爆出来的到输出目录
                    foreach (var bundle in source)
                    {
                        var bundleName = bundle.hash;
                        var bytes_src = AssetsEditorTool.ReadFileSync(AssetsEditorTool.CombinePath(context.historyPath, bundleName));
                        var en = context.assetBuild.GetEncryptByCode(bundle.GetEncryptCode());
                        var bytes = en.Encode(bundleName, bytes_src);
                        AssetsEditorTool.WriteFileSync(AssetsEditorTool.CombinePath(context.outputPath, bundleName), bytes);
                    }


                    foreach (var bundleName in manifest.allBundle)
                    {
                        string path = AssetsEditorTool.CombinePath(context.outputPath, bundleName);
                        BundleData bundleData = manifest.GetBundleData(bundleName);



                        if (AssetsEditorTool.ExistsFile(path))
                        {
                            var data = FileData.CreateByFile(path);
                            bundleData.length = data.length;
                            bundleData.hash = data.hash;

                        }
                        else
                        {
                            this.SetErr($"can't find last bundle version {bundleName}");
                            InvokeComplete();
                            return;
                        }
                    }

                    var mainfestName = context.buildPkg.manifestFileName;

                    AssetsEditorTool.WriteBufferObjectSync(manifest,
                           AssetsEditorTool.CombinePath(context.outputPath,
                           mainfestName));
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


        protected override async void OnExecute(AssetTaskContext context)
        {
            AssetsEditorTool.DeleteDirectory(context.outputPath);
            AssetsEditorTool.CreateDirectory(context.outputPath);

            var builds = context.buildPkgs.FindAll(x => x.build);
            if (context.Pipeline == TaskPipelineType.EditorSimulate)
                builds = context.buildPkgs;
            if (builds.Count == 0)
            {
                SetErr("Nothing To Build");
                InvokeComplete();
                return;
            }
            List<PackageExportData> exports = new List<PackageExportData>();

            for (int i = 0; i < builds.Count; i++)
            {
                var group = builds[i];
                context.buildPkg = group;
                for (int j = 0; j < tasks.Count; j++)
                    await Execute(tasks[j], context);

                exports.Add(new PackageExportData()
                {
                    pkg = context.buildPkg.ToPackageData(),
                    manifest = context.manifest,
                    encrypt = context.encrypt.ToString(),
                    version = context.version,
                    compress = context.compress.ToString(),
                    typeTreeOption = context.typeTreeOption,
                });
            }
            var manifests = exports.ConvertAll(x => x.manifest);
            ManifestData manifest = new ManifestData();
            foreach (var item in manifests)
                ManifestData.Merge(item, manifest, null);
            manifest.Prepare(context.fuzzySearch,context.fileNameSearchType);
            context.mergedManifest = manifest;
            context.exports = exports;
            if (context.Pipeline == TaskPipelineType.BuildBundle)
            {
                var versions = context.historyVersions;

                if (versions.FindVersion(context.version) == null)
                    versions.AddVersion(context.version);

                AssetsEditorTool.WriteJson(
                        versions,
                        context.historyVersionPath);


                var outputVersions = versions.DeepCopy();
                outputVersions.RemoveFirstIFTooLarge(context.MaxCacheVersionCount);


                AssetsEditorTool.WriteBufferObjectSync(
                         outputVersions,
                         AssetsEditorTool.CombinePath(context.outputPath, context.VersionCollectionName));
                VersionData versionData = versionData = new VersionData()
                {
                    version = context.version,
                };
                versionData.SetPkgs(context.buildPkgs.ConvertAll(x => x.ToPackageData()));

                AssetsEditorTool.WriteBufferObjectSync(
                    versionData,
                    AssetsEditorTool.CombinePath(context.outputPath, context.VersionDataName));
            }

            InvokeComplete();
        }
    }
}
