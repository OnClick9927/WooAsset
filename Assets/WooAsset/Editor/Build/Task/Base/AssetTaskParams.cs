using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

namespace WooAsset
{
    public class AssetTaskParams
    {
        public int MaxCacheVersionCount;
        public CompressType compress;
        public List<EditorPackageData> buildPkgs;
        public TypeTreeOption typeTreeOption;
        //public bool cleanHistory;
        public BundleNameCalculateType bundleNameCalculateType;
        public BundleNameType bundleNameType;
        public List<string> buildInAssets;
        public BuildMode buildMode;
        public bool copyToStream;
        public IAssetsBuild assetBuild;
        public IAssetEncrypt encrypt;
        public IBuildInBundleSelector buildInBundleSelector;
        public IBuildPipeLine buildPipe;
        public IBundleOptimizer bundleOptimizer;
        public int optimizationCount;

        public string version;

        public List<TagAssets> tags;

        //public List<AssetTask> pipelineStartTasks;
        //public List<AssetTask> pipelineEndTasks;
        public List<FileRecordData> recordIgnore;
        public bool fuzzySearch;
        public FileNameSearchType fileNameSearchType;
        public readonly TaskPipelineType Pipeline;

        public List<string> GetAssetTags(string path)
        {
            List<string> result = new List<string>();
            var result_a = assetBuild.GetAssetTags(path);
            if (result_a != null)
                result.AddRange(result_a);
            if (tags != null)
                result.AddRange(tags.FindAll(x => x.FitAssetTag(path)).ConvertAll(x => x.tag));
            return result.Distinct().ToList();
        }
        public bool GetIsRecord(string path)
        {
            if (!assetBuild.GetIsRecord(path)) return false;
            for (int i = 0; i < recordIgnore.Count; i++)
            {
                var ignore = recordIgnore[i];
                if (ignore.Fit(path)) return false;
            }
            return true;
        }

        public AssetTaskParams(TaskPipelineType Pipeline)
        {
            var option = AssetsEditorTool.option;
            this.Pipeline = Pipeline;
            MaxCacheVersionCount = option.MaxCacheVersionCount;
            compress = option.compress;
            buildPkgs = option.pkgs;
            typeTreeOption = option.typeTreeOption;
            //cleanHistory = option.cleanHistory;
            bundleNameType = option.bundleNameType;
            bundleNameCalculateType = option.bundleNameCalculate;
            buildInAssets = option.buildIn.assets.ConvertAll(x => AssetDatabase.GetAssetPath(x));
            copyToStream = option.buildIn.copyToStream;
            buildMode = option.buildMode;
            encrypt = Activator.CreateInstance(option.GetStreamEncryptType()) as IAssetEncrypt;
            assetBuild = Activator.CreateInstance(option.GetAssetBuildType()) as IAssetsBuild;
            buildPipe = Activator.CreateInstance(option.GetBuildPipelineType()) as IBuildPipeLine;
            bundleOptimizer = Activator.CreateInstance(option.GetBundleOptimizerType()) as IBundleOptimizer;
            buildInBundleSelector = Activator.CreateInstance(option.GetBuildInBundleSelectorType()) as IBuildInBundleSelector;
            version = option.version;
            tags = option.tags;
            recordIgnore = option.recordIgnore;
            optimizationCount = option.bundleOptimize.count;
        }

        public BuildAssetBundleOptions GetBundleOption(out string err) => buildPipe.GetBundleOption(this, out err);

        public string CheckLegal()
        {
            for (int i = 0; i < buildPkgs.Count; i++)
            {
                var pkg = buildPkgs[i];
                if (string.IsNullOrEmpty(pkg.name))
                    return "Pkg name can not be null";
                if (buildPkgs.FindAll(x => x.name == pkg.name).Count > 1)
                    return "same name pkg";
                if (pkg.HasSamePath() || buildPkgs.Any(x => x != pkg && pkg.HasSamePath(x)))
                    return "same path in Pkg";
                var paths = pkg.paths;
                for (int j = 0; j < paths.Count; j++)
                {
                    if (!AssetsEditorTool.ExistsDirectory(paths[j]))
                        return $"Pkg path not exist {paths[j]}";
                }
                if (pkg.builds != null && pkg.builds.Count > 0)
                {
                    for (int j = 0; j < pkg.builds.Count; j++)
                    {
                        var build = pkg.builds[j];
                        if (build.selectors.Count(x => x.type == AssetSelectorParam.SelectType.Union) == 0)
                        {
                            return $"Pkg-->{pkg.name} Build at-->{j} :  at least one Union ";
                        }
                    }
                }
            }
            return string.Empty;
        }
    }

}
