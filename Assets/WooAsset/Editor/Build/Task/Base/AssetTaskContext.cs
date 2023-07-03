using UnityEditor;
using System.Collections.Generic;
using UnityEditor.U2D;
using System.Linq;
using static WooAsset.AssetsBuildOption;

namespace WooAsset
{
    public class AssetTaskContext
    {
        public TaskPipelineType Pipeline;
        public string outputPath;
        public string historyPath;
        public BuildTarget buildTarget;
        public BuildAssetBundleOptions BuildOption;
        public IAssetBuild assetBuild;
        public IAssetStreamEncrypt encrypt;
        public string remoteHashName;
        public string localHashName;

        public string buildTargetName;
        public CompressType compress;
        public string serverDirectory;
        public bool ignoreTypeTreeChanges;
        public bool forceRebuild;
        public long bundleSize;
        public List<string> ignoreFileExtend;
        public List<BuildGroup> buildGroups;
        public BuildGroup buildGroup;


        public AssetTagCollection tags;
        public AssetsTree tree;
        public string version;
        public List<EditorAssetData> needBuildAssets;
        public List<BundleGroup> allBundleGroups;
        public List<FileData> files;
        public ManifestData manifest;
        public AssetsVersionCollection versions;
        public FileChange fileChange;

        public List<string> useful;


        public string shaderVariantDirectory;
        public TextureImporterPlatformSettings PlatformSetting;
        public SpriteAtlasTextureSettings TextureSetting;
        public SpriteAtlasPackingSettings PackingSetting;
        public string[] atlasPaths;
        public List<AssetTask> pipelineFinishTasks;
        public List<string> rawAssets;
    }

}
