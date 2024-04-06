using UnityEditor;
using System.Collections.Generic;
using UnityEditor.U2D;
using static WooAsset.AssetsBuildOption;
using UnityEngine;

namespace WooAsset
{
    [System.Serializable]
    public class AssetTaskContext
    {
        public TaskPipelineType Pipeline;


        public string outputPath;
        public string historyPath;
        public int MaxCacheVersionCount;
        public BuildTarget buildTarget;
        public BuildAssetBundleOptions BuildOption;
        public IAssetBuild assetBuild;
        public IAssetStreamEncrypt encrypt;
        public string remoteHashName;
        public string localHashName;

        public string streamBundleDirectory;
        public string buildTargetName;
        public string serverDirectory;
        public List<EditorBundlePackage> buildPkgs;
        public CompressType compress;
        public bool ignoreTypeTreeChanges;
        public bool forceRebuild;
        public EditorBundlePackage buildPkg;


        public AssetsTree tree;
        public List<EditorAssetData> needBuildAssets;
        public List<BundleGroup> allBundleGroups;
        public List<FileData> files;
        public List<string> useful;
        public string version;
        public AssetsVersionCollection versions;
        public AssetsVersionCollection outputVersions;

        public ManifestData manifest;
        public FileChange fileChange;
        public List<Object> buildInAssets;


        public string shaderVariantDirectory;
        public TextureImporterPlatformSettings PlatformSetting;
        public SpriteAtlasTextureSettings TextureSetting;
        public SpriteAtlasPackingSettings PackingSetting;
        public string[] atlasPaths;
        public List<AssetTask> pipelineStartTasks;

        public List<AssetTask> pipelineEndTasks;


        public List<GroupExportData> exports = new List<GroupExportData>();
        public string historyVersionFilePath;
        public string historyVersionFileName = "Versions.json";
        public bool cleanHistory;
    }

}
