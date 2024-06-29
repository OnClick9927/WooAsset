using UnityEditor;
using System.Collections.Generic;

namespace WooAsset
{

    public class AssetTaskContext
    {
        public readonly AssetTaskParams Params;
        public int MaxCacheVersionCount => Params.MaxCacheVersionCount;
        public string outputPath => AssetsEditorTool.OutputPath;
        public string historyPath => AssetsEditorTool.HistoryPath;
        public BuildTarget buildTarget => AssetsEditorTool.BuildTarget;
        public string VersionCollectionName => VersionHelper.VersionCollectionName;
        public string VersionDataName => VersionHelper.VersionDataName;
        public string buildTargetName => AssetsEditorTool.BuildTargetName;

        public string streamBundleDirectory => AssetsHelper.StreamBundlePath;

        public string serverDirectory => AssetsEditorTool.ServerDirectory;
        public CompressType compress => Params.compress;
        public List<EditorPackageData> buildPkgs => Params.buildPkgs;
        public TypeTreeOption typeTreeOption => Params.typeTreeOption;
        public bool cleanHistory => Params.cleanHistory;
        public BundleNameType bundleNameType => Params.bundleNameType;
        public List<string> buildInAssets => Params.buildInAssets;
        public BuildMode buildMode => Params.buildMode;
        public bool copyToStream => Params.copyToStream;
        public bool isNormalBuildMode => Pipeline != TaskPipelineType.DryBuild && Pipeline != TaskPipelineType.EditorSimulate;
        public TaskPipelineType Pipeline => Params.Pipeline;
        public IAssetsBuild assetBuild => Params.assetBuild;
        public IAssetEncrypt encrypt => Params.encrypt;
        public List<AssetTask> pipelineStartTasks => Params.pipelineStartTasks;
        public List<AssetTask> pipelineEndTasks => Params.pipelineEndTasks;



        public string version;
        public BuildAssetBundleOptions BuildOption;
        public AssetCollection assetsCollection;
        public EditorPackageData buildPkg;
        public List<EditorAssetData> needBuildAssets;
        public List<EditorBundleData> allBundleBuilds;
        public VersionCollectionData historyVersions;
        public ManifestData manifest;
        public List<PackageExportData> exports;
        public string historyVersionPath;
        public ManifestData mergedManifest;

        public AssetTaskContext(AssetTaskParams @params)
        {
            Params = @params;
        }
    }

}
