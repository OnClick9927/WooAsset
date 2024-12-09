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
        public bool cleanHistory;
        public BundleNameType bundleNameType;
        public List<string> buildInAssets;
        public BuildMode buildMode;
        public bool copyToStream;
        public IAssetsBuild assetBuild;
        public IAssetEncrypt encrypt;
        public IBuildInBundleSelector buildInBundleSelector;
        public IBuildPipeLine buildPipe;
        public IBundleOptimizer bundleOptimiser;
        public string version;

        public List<TagAssets> tags;

        public List<AssetTask> pipelineStartTasks;
        public List<AssetTask> pipelineEndTasks;
        public List<AssetIgnoreData> recordIgnore;
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
            cleanHistory = option.cleanHistory;
            bundleNameType = option.bundleNameType;
            buildInAssets = option.buildInAssets.ConvertAll(x => AssetDatabase.GetAssetPath(x));
            buildMode = option.buildMode;
            copyToStream = option.copyToStream;
            encrypt = Activator.CreateInstance(option.GetStreamEncryptType()) as IAssetEncrypt;
            assetBuild = Activator.CreateInstance(option.GetAssetBuildType()) as IAssetsBuild;
            buildPipe = Activator.CreateInstance(option.GetBuildPipelineType()) as IBuildPipeLine;
            bundleOptimiser = Activator.CreateInstance(option.GetBundleOptimiserType()) as IBundleOptimizer;
            buildInBundleSelector = Activator.CreateInstance(option.GetBuildInBundleSelectorType()) as IBuildInBundleSelector;
            version = option.version;
            tags = option.tags;
            recordIgnore = option.recordIgnore;
        }

        public BuildAssetBundleOptions GetBundleOption(out string err) => buildPipe.GetBundleOption(this, out err);

        public string CheckLeagal()
        {
            for (int i = 0; i < buildPkgs.Count; i++)
            {
                var item = buildPkgs[i];
                if (string.IsNullOrEmpty(item.name))
                    return "Pkg name can not be null";
                if (buildPkgs.FindAll(x => x.name == item.name).Count > 1)
                    return "same name pkg";
                if (item.HasSamePath() || buildPkgs.Any(x => x != item && item.HasSamePath(x)))
                    return "same path in Pkg";
                var paths = item.paths;
                for (int j = 0; j < paths.Count; j++)
                {
                    if (!AssetsEditorTool.ExistsDirectory(paths[j]))
                        return $"Pkg path not exist {paths[j]}";
                }

            }
            return string.Empty;
        }
    }

}
