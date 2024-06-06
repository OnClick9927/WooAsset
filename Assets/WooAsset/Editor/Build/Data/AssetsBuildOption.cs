using System.Collections.Generic;
using System;
using UnityEditor.U2D;
using UnityEditor;
using Object = UnityEngine.Object;

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
        public List<string> recordIgnore = new List<string>();
        public TypeSelect build = new TypeSelect();
        public TypeSelect mode = new TypeSelect();
        public TypeSelect encrypt = new TypeSelect();
        public List<TagAssets> tags = new List<TagAssets>();
        public string shaderVariantDirectory;
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









        public void OnEnable()
        {
            if (encrypt.baseType == null)
            {
                encrypt.baseType = typeof(IAssetStreamEncrypt);
                encrypt.Enable();
            }
            if (build.baseType == null)
            {
                build.baseType = typeof(IAssetBuild);
                build.Enable();
            }
            if (mode.baseType == null)
            {
                mode.baseType = typeof(IAssetMode);
                mode.Enable();
            }
            recordIgnore.RemoveAll(x => !AssetsHelper.ExistsFile(x) && !AssetsEditorTool.ExistsDirectory(x));
            tags.ForEach(x => x.assets.RemoveAll(y => !AssetsHelper.ExistsFile(y)));
        }




        public Type GetAssetBuildType()
        {
            return build.GetSelectType();
        }
        public Type GetStreamEncryptType()
        {
            return encrypt.GetSelectType();
        }
        public Type GetAssetModeType()
        {
            return mode.GetSelectType();
        }

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

        public void AddToRecordIgnore(string path)
        {
            if (recordIgnore.Contains(path)) return;
            recordIgnore.Add(path);
        }
        public void RemoveFromRecordIgnore(string path) => recordIgnore.Remove(path);
        public List<string> GetAllTags() => tags.ConvertAll(x => x.tag);
        public void AddAssetTag(string path, string tag)
        {
            if (tags == null) tags = new List<TagAssets>();
            TagAssets assets = tags.Find(x => x.tag == tag);
            if (assets == null)
            {
                assets = new TagAssets();
                tags.Add(assets);
            }
            if (!assets.assets.Contains(path))
            {
                assets.assets.Add(path);
            }

        }
        public void RemoveAssetTag(string path, string tag)
        {
            if (tags == null) return;
            TagAssets assets = tags.Find(x => x.tag == tag);
            if (assets == null) return;
            assets.assets.Remove(path);

        }

        public SpriteAtlasPackingSettings GetPackingSetting()
        {
            return new SpriteAtlasPackingSettings()
            {
                blockOffset = packSetting.blockOffset,
                enableRotation = packSetting.enableRotation,
                enableTightPacking = packSetting.enableTightPacking,
                padding = packSetting.padding,
            };
        }
        public SpriteAtlasTextureSettings GetTextureSetting()
        {
            return new SpriteAtlasTextureSettings()
            {
                readable = textureSetting.readable,
                generateMipMaps = textureSetting.generateMipMaps,
                filterMode = textureSetting.filterMode,
                anisoLevel = textureSetting.anisoLevel,
                sRGB = textureSetting.sRGB,
            };
        }

    }
}
