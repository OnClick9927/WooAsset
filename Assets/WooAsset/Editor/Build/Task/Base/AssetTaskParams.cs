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
        public IAssetBuild assetBuild;
        public IAssetStreamEncrypt encrypt;
        public string version;

        public List<TagAssets> tags;

        public List<AssetTask> pipelineStartTasks;
        public List<AssetTask> pipelineEndTasks;
        public List<string> recordIgnore;

        public readonly TaskPipelineType Pipeline;
        public List<string> GetAssetTags(string path)
        {
            List<string> result = new List<string>();
            var result_a = assetBuild.GetAssetTags(path);
            if (result_a != null)
                result.AddRange(result_a);
            if (tags != null)
                result.AddRange(tags.FindAll(x => x.assets.Contains(path)).ConvertAll(x => x.tag));
            return result.Distinct().ToList();
        }
        public bool GetIsRecord(string path) => !recordIgnore.Any(x => path.StartsWith(x)) && assetBuild.GetIsRecord(path);

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
            encrypt = Activator.CreateInstance(option.GetStreamEncryptType()) as IAssetStreamEncrypt;
            assetBuild = Activator.CreateInstance(option.GetAssetBuildType()) as IAssetBuild;
            version = option.version;
            tags = option.tags;
            recordIgnore = option.recordIgnore;
        }

        public BuildAssetBundleOptions GetBundleOption()
        {
            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;


            if (typeTreeOption == TypeTreeOption.DisableWriteTypeTree)
                opt |= BuildAssetBundleOptions.DisableWriteTypeTree;
            if (typeTreeOption == TypeTreeOption.IgnoreTypeTreeChanges)
                opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;

            if (buildMode == BuildMode.ForceRebuild)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
            if (compress == CompressType.LZ4)
                opt |= BuildAssetBundleOptions.ChunkBasedCompression;
            if (compress == CompressType.Uncompressed)
                opt |= BuildAssetBundleOptions.UncompressedAssetBundle;

            if (Pipeline == TaskPipelineType.DryBuild)
                opt = BuildAssetBundleOptions.DryRunBuild;
            if (bundleNameType == BundleNameType.NameWithHash || bundleNameType == BundleNameType.Hash)
                opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName;

            return opt;
        }

        public string CheckLeagal()
        {
            for (int i = 0; i < buildPkgs.Count; i++)
            {
                var item = buildPkgs[i];
                if (string.IsNullOrEmpty(item.name))
                    return "Pkg name can not be null";
                if (buildPkgs.FindAll(x => x.name == item.name).Count > 1)
                    return "same name Kpg";
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
