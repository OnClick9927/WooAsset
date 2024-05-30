using UnityEditor;
using System.Collections.Generic;
using static WooAsset.AssetsBuildOption;
using static WooAsset.AssetsEditorTool;
using UnityEngine;

namespace WooAsset
{
    [System.Serializable]
    public class AssetTaskContext
    {

        public int MaxCacheVersionCount => option.MaxCacheVersionCount;
        public string outputPath => AssetsEditorTool.outputPath;
        public string historyPath => AssetsEditorTool.historyPath;
        public BuildTarget buildTarget => AssetsEditorTool.buildTarget;
        public string VersionCollectionName => VersionHelper.VersionCollectionName;
        public string VersionDataName => VersionHelper.VersionDataName;
        public string buildTargetName => AssetsEditorTool.buildTargetName;

        public string streamBundleDirectory => AssetsHelper.streamBundleDirectory;

        public string serverDirectory => AssetsEditorTool.ServerDirectory;
        public CompressType compress => option.compress;
        public List<EditorBundlePackage> buildPkgs => option.pkgs;
        public TypeTreeOption typeTreeOption => option.typeTreeOption;
        public bool cleanHistory => option.cleanHistory;
        public BundleNameType bundleNameType => option.bundleNameType;
        public List<Object> buildInAssets => option.buildInAssets;
        public BuildMode buildMode => option.buildMode;
        public bool copyToStream => option.copyToStream;

        
        public bool isNormalBuildMode => buildMode != BuildMode.Dry;

        public EditorBundlePackage buildPkg;
        public AssetCollection assetsCollection;
        public TaskPipelineType Pipeline;
        public BuildAssetBundleOptions BuildOption;
        public IAssetBuild assetBuild;
        public IAssetStreamEncrypt encrypt;


        public List<EditorAssetData> needBuildAssets;
        public List<EditorBundleData> allBundleBuilds;
        public string version;
        public VersionCollectionData historyVersions;
        public VersionCollectionData outputVersions;

        public ManifestData manifest;
        public List<AssetTask> pipelineStartTasks;
        public List<AssetTask> pipelineEndTasks;
        public List<PackageExportData> exports = new List<PackageExportData>();

        public string historyVersionPath;
    }

}
