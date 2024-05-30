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
            context.assetBuild = assetBuild;
            context.encrypt = encrypt;
            context.historyVersions = new VersionCollectionData() { };
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
                    if (!AssetsEditorTool.ExistsDirectory(paths[j]))
                    {
                        SetErr($"buildGroup path not exist {paths[j]}");
                        InvokeComplete();
                        return;
                    }
                }

            }

            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;


            if (context.typeTreeOption == TypeTreeOption.DisableWriteTypeTree)
                opt |= BuildAssetBundleOptions.DisableWriteTypeTree;
            if (context.typeTreeOption == TypeTreeOption.IgnoreTypeTreeChanges)
                opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;

            if (context.buildMode == BuildMode.ForceRebuild)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
            if (context.compress == CompressType.LZ4)
                opt |= BuildAssetBundleOptions.ChunkBasedCompression;
            if (context.compress == CompressType.Uncompressed)
                opt |= BuildAssetBundleOptions.UncompressedAssetBundle;

            if (context.buildMode == BuildMode.Dry)
                opt = BuildAssetBundleOptions.DryRunBuild;
            if (context.bundleNameType == BundleNameType.NameWithHash || context.bundleNameType == BundleNameType.Hash)
                opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName;

            context.BuildOption = opt;

            string versionPath = AssetsHelper.CombinePath(context.historyPath, context.VersionCollectionName);
            context.historyVersionPath = versionPath;
            if (AssetsHelper.ExistsFile(versionPath))
            {
                var reader = AssetsHelper.ReadFile(versionPath, true);
                await reader;
                context.historyVersions = VersionHelper.ReadAssetsVersionCollection(reader.bytes);
            }
            context.version = assetBuild.GetVersion(option.version, context);
            context.pipelineStartTasks = assetBuild.GetPipelineStartTasks(context);
            context.pipelineEndTasks = assetBuild.GetPipelineEndTasks(context);


            InvokeComplete();
        }

    }
}
