using System;
using System.Linq;
using UnityEditor;

namespace WooAsset
{
    public class PrepareTask : AssetTask
    {
        protected override async void OnExecute(AssetTaskContext context)
        {
            AssetsBuildOption option = AssetsEditorTool.option;


            IAssetStreamEncrypt encrypt = Activator.CreateInstance(option.GetStreamEncryptType()) as IAssetStreamEncrypt;
            IAssetBuild assetBuild = Activator.CreateInstance(option.GetAssetBuildType()) as IAssetBuild;
            context.ignoreTypeTreeChanges = option.ignoreTypeTreeChanges;
            context.forceRebuild = option.forceRebuild;
            context.compress = option.compress;



            context.assetBuild = assetBuild;
            context.encrypt = encrypt;
            context.buildTarget = AssetsEditorTool.buildTarget;
            context.outputPath = AssetsEditorTool.outputPath;
            context.historyPath = AssetsEditorTool.historyPath;

            context.remoteHashName = VersionBuffer.remoteHashName;
            context.localHashName = VersionBuffer.localHashName;

            context.buildTargetName = AssetsEditorTool.buildTargetName;
            context.streamBundleDirectory = AssetsHelper.streamBundleDirectory;

            context.versions = new AssetsVersionCollection() { };
            context.DisableWriteTypeTree = option.DisableWriteTypeTree;
            context.AppendHashToAssetBundleName = option.AppendHashToAssetBundleName;
            context.MaxCacheVersionCount = option.MaxCacheVersionCount;
            context.shaderVariantDirectory = option.shaderVariantDirectory;
            context.PlatformSetting = option.PlatformSetting;
            context.TextureSetting = option.GetTextureSetting();
            context.PackingSetting = option.GetPackingSetting();
            context.atlasPaths = option.atlasPaths.ToArray();
            context.serverDirectory = option.serverDirectory;
            context.buildPkgs = option.buildPkgs;
            context.historyVersionFilePath = AssetsHelper.CombinePath(context.historyPath, context.historyVersionFileName);
            context.cleanHistory = option.cleanHistory;
            context.buildInAssets = option.buildInAssets;
            if (context.MaxCacheVersionCount < 1)
                context.MaxCacheVersionCount = 1;
            for (int i = 0; i < context.buildPkgs.Count; i++)
            {
                var item = context.buildPkgs[i];
                if (string.IsNullOrEmpty(item.name))
                {
                    SetErr("buildGroup name can not be null");
                    InvokeComplete();
                    return;
                }
                if (context.buildPkgs.FindAll(x => x.name == item.name).Count > 1)
                {
                    SetErr("same name build Group");
                    InvokeComplete();
                    return;
                };
                if (item.HasSamePath() || context.buildPkgs.Any(x => x != item && item.HasSamePath(x)))
                {
                    SetErr("same path in build Group");
                    InvokeComplete();
                    return;
                };

                var paths = item.paths;

                for (int j = 0; j < paths.Count; j++)
                {
                    if (!AssetsHelper.ExistsDirectory(paths[j]))
                    {
                        SetErr($"buildGroup path not exist {paths[j]}");
                        InvokeComplete();
                        return;
                    }
                }

            }

            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode;
            if (context.DisableWriteTypeTree)
                opt |= BuildAssetBundleOptions.DisableWriteTypeTree;
            if (context.AppendHashToAssetBundleName)
                opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
            if (context.forceRebuild)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            if (context.ignoreTypeTreeChanges)
                opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
            if (context.compress == CompressType.LZ4)
                opt |= BuildAssetBundleOptions.ChunkBasedCompression;
            if (context.compress == CompressType.None)
                opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
            context.BuildOption = opt;

            string versionPath = context.historyVersionFilePath;
            if (AssetsHelper.ExistsFile(versionPath))
            {
                var reader = await AssetsHelper.ReadFile(versionPath, true);
                context.versions = VersionBuffer.ReadAssetsVersionCollection(reader.bytes, new NoneAssetStreamEncrypt());
            }
            context.version = assetBuild.GetVersion(option.version, context);
            context.pipelineStartTasks = assetBuild.GetPipelineStartTasks(context);
            context.pipelineEndTasks = assetBuild.GetPipelineEndTasks(context);


            InvokeComplete();
        }

    }
}
