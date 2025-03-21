using System.Collections.Generic;
using System;
using UnityEditor.U2D;
using UnityEditor;
using Object = UnityEngine.Object;
using System.Linq;

namespace WooAsset
{

    public class AssetsBuildOption : AssetsScriptableObject
    {
        public string version = "0.0.1";
        public bool copyToStream = false;
        public BuildMode buildMode = BuildMode.Increase;

        public TypeTreeOption typeTreeOption = TypeTreeOption.IgnoreTypeTreeChanges;
        public BundleNameType bundleNameType = BundleNameType.Hash;
        public CompressType compress = CompressType.LZ4;
        public int MaxCacheVersionCount = 8;
        public bool cleanHistory = true;
        public List<Object> buildInAssets = new List<Object>();

        public bool enableServer;
        public int serverPort = 8080;
        public List<EditorPackageData> pkgs = new List<EditorPackageData>();



        public List<FileRecordData> recordIgnore = new List<FileRecordData>();
        public TypeSelect build = new TypeSelect();
        public TypeSelect mode = new TypeSelect();
        public TypeSelect encrypt = new TypeSelect();
        public TypeSelect buildInBundleSelector = new TypeSelect();
        public TypeSelect buildPipeline = new TypeSelect();
        public TypeSelect bundleOptimizer = new TypeSelect();
        public int optimizationCount;


        public List<TagAssets> tags = new List<TagAssets>();
        public List<string> shaderVariantInputDirectory;
        public string shaderVariantOutputDirectory;
        public List<string> atlasPaths = new List<string>();
        public PackingSetting packSetting = new PackingSetting();
        public TextureSetting textureSetting = new TextureSetting();
        public TextureImporterPlatformSettings PlatformSetting = new TextureImporterPlatformSettings()
        {
            maxTextureSize = 2048,
            format = TextureImporterFormat.Automatic,
            crunchedCompression = true,
            textureCompression = TextureImporterCompression.Compressed,
            compressionQuality = 50,
        };







        protected override void OnLoad()
        {
            OnEnable();
        }

        private void OnEnable()
        {
            if (encrypt.baseType == null)
            {
                encrypt.baseType = typeof(IAssetEncrypt);
                encrypt.Enable();
            }
            if (build.baseType == null)
            {
                build.baseType = typeof(IAssetsBuild);
                build.Enable();
            }
            if (mode.baseType == null)
            {
                mode.baseType = typeof(IAssetsMode);
                mode.Enable();
            }
            if (buildInBundleSelector.baseType == null)
            {
                buildInBundleSelector.baseType = typeof(IBuildInBundleSelector);
                buildInBundleSelector.Enable();
            }
            if (buildPipeline.baseType == null)
            {
                buildPipeline.baseType = typeof(IBuildPipeLine);
                buildPipeline.Enable();
            }
            if (bundleOptimizer.baseType == null)
            {
                bundleOptimizer.baseType = typeof(IBundleOptimizer);
                bundleOptimizer.Enable();
            }
            recordIgnore.RemoveAll(x =>

       (x.type == FileType.File && !AssetsEditorTool.ExistsFile(x.path)) ||
       (x.type == FileType.Directory && !AssetsEditorTool.ExistsDirectory(x.path)));
            tags.ForEach(z => z.assets.RemoveAll(x => (x.type == FileType.File && !AssetsEditorTool.ExistsFile(x.path)) ||
       (x.type == FileType.Directory && !AssetsEditorTool.ExistsDirectory(x.path))));
        }




        public Type GetAssetBuildType() => build.GetSelectType();
        public Type GetStreamEncryptType() => encrypt.GetSelectType();
        public Type GetAssetModeType() => mode.GetSelectType();
        public Type GetBuildInBundleSelectorType() => buildInBundleSelector.GetSelectType();
        public Type GetBuildPipelineType() => buildPipeline.GetSelectType();
        public Type GetBundleOptimizerType() => bundleOptimizer.GetSelectType();


        public bool SetAssetBuildType(Type type)
        {
            if (build.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }
        public bool SetStreamEncryptType(Type type)
        {
            if (encrypt.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }
        public bool SetBuildInBundleSelectorType(Type type)
        {
            if (buildInBundleSelector.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }
        public bool SetBuildPipelineType(Type type)
        {
            if (buildPipeline.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }
        public bool SetBundleOptimizerType(Type type)
        {
            if (bundleOptimizer.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }



        public void AddToRecordIgnore(string path, FileType type)
        {
            if (recordIgnore.Any(x => x.path == path && x.type == type)) return;
            recordIgnore.Add(new FileRecordData() { type = type, path = path });
        }
        public void RemoveFromRecordIgnore(string path, FileType type) => recordIgnore.RemoveAll(x => x.path == path && x.type == type);




        public List<string> GetAllTags() => tags.ConvertAll(x => x.tag);
        public void AddAssetTag(string path, FileType type, string tag)
        {
            if (tags == null) tags = new List<TagAssets>();
            TagAssets assets = tags.Find(x => x.tag == tag);
            if (assets == null)
            {
                assets = new TagAssets();
                tags.Add(assets);
            }
            assets.Add(type, path);

        }
        public void RemoveAssetTag(string path, FileType type, string tag)
        {
            if (tags == null) return;
            TagAssets assets = tags.Find(x => x.tag == tag);
            if (assets == null) return;
            assets.Remove(type, path);

        }
    }
}
