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
        public string VersionCollectionName => AssetsEditorTool.VersionCollectionName;
        public string VersionDataName => AssetsEditorTool.VersionDataName;
        public string buildTargetName => AssetsEditorTool.BuildTargetName;

        public string streamBundleDirectory => AssetsEditorTool.StreamBundlePath;

        public string serverDirectory => AssetsEditorTool.ServerDirectory;
        public CompressType compress => Params.compress;
        public List<EditorPackageData> buildPkgs => Params.buildPkgs;
        public TypeTreeOption typeTreeOption => Params.typeTreeOption;
        //public bool cleanHistory => Params.cleanHistory;
        public BundleNameType bundleNameType => Params.bundleNameType;
        public BundleNameCalculateType bundleNameCalculateType => Params.bundleNameCalculateType;

        public List<string> buildInAssets => Params.buildInAssets;
        public BuildMode buildMode => Params.buildMode;
        public bool copyToStream => Params.copyToStream;
        public TaskPipelineType Pipeline => Params.Pipeline;
        public IAssetsBuild assetBuild => Params.assetBuild;
        public IAssetEncrypt encrypt => Params.encrypt;
        //public List<AssetTask> pipelineStartTasks => Params.pipelineStartTasks;
        //public List<AssetTask> pipelineEndTasks => Params.pipelineEndTasks;
        public IBuildInBundleSelector buildInBundleSelector => Params.buildInBundleSelector;
        public IBuildPipeLine buildPipe => Params.buildPipe;
        public IBundleOptimizer bundleOptimiser => Params.bundleOptimizer;
        public int optimizationCount => Params.optimizationCount;

        public bool fuzzySearch => Params.fuzzySearch;
        public FileNameSearchType fileNameSearchType => Params.fileNameSearchType;


        public string version;
        public BuildAssetBundleOptions BuildOption;
        public EditorAssetCollection assetsCollection;
        public EditorPackageData buildPkg;
        public List<EditorAssetData> needBuildAssets;
        public Dictionary<string, EditorAssetCollection> allAssetCollections = new Dictionary<string, EditorAssetCollection>();

        public Dictionary<string, List<EditorBundleData>> allBundleBuilds = new Dictionary<string, List<EditorBundleData>>();
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
