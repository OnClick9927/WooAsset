using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEditor.U2D;
using UnityEditor;
using Object = UnityEngine.Object;

namespace WooAsset
{
    public class AssetsBuildOption : AssetsScriptableObject
    {
        [System.Serializable]
        public class TagAssets
        {
            public string tag;
            public List<string> assets;
        }
        public enum TypeTreeOption
        {
            None,
            IgnoreTypeTreeChanges,
            DisableWriteTypeTree,

        }
        [System.Serializable]
        public class EditorBundlePackage
        {
            public bool build;
            public string name;
            public string description;
            public List<string> tags = new List<string>();
            public List<string> paths = new List<string>();

            public bool HasSamePath() => paths.Distinct().Count() != paths.Count();
            public bool HasSamePath(EditorBundlePackage other) => paths.Intersect(other.paths).Count() > 0;

            public AssetsVersionCollection.VersionData.PackageData ToPackageData()
            {
                return new AssetsVersionCollection.VersionData.PackageData()
                {
                    description = description,
                    name = name,
                    tags = tags
                };
            }
        }

        [System.Serializable]
        public class TypeSelect
        {
            public string[] types;
            public string[] shortTypes;
            public int typeIndex;
            public Type baseType;
            public void Enable()
            {
                var list = GetSubTypesInAssemblies(baseType)
               .Where(type => !type.IsAbstract);
                types = list.Select(type => type.FullName).ToArray();
                shortTypes = list.Select(type => type.Name).ToArray();
            }
            public Type GetSelectType()
            {
                var type_str = types[typeIndex];
                Type type = GetSubTypesInAssemblies(baseType)
                   .Where(type => !type.IsAbstract)
                   .ToList()
                   .Find(x => x.FullName == type_str);

                return type;
            }

            public bool SetType(Type type)
            {
                string name = type.FullName;
                if (type.IsAbstract || !baseType.IsAssignableFrom(type)) return false;
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] == name)
                    {
                        typeIndex = i;
                        return true;
                    }
                }
                return false;
            }
        }
        [System.Serializable]
        public class PackingSetting
        {
            public int blockOffset = 1;
            public bool enableRotation = false;
            public bool enableTightPacking = false;
            public int padding = 2;
        }
        [System.Serializable]
        public class TextureSetting
        {
            public bool readable = true;
            public bool generateMipMaps = false;
            public bool sRGB = true;
            public FilterMode filterMode = FilterMode.Bilinear;
            public int anisoLevel = 1;
        }
        public enum BuildMode
        {
            Dry,
            Increase,
            ForceRebuild,
        }
        public enum BundleNameType
        {
            Name,
            NameWithHash
        }

        public string version = "0.0.1";
        public bool copyToStream = false;
        public BuildMode buildMode = BuildMode.Increase;

        public TypeTreeOption typeTreeOption;
        public BundleNameType bundleNameType;
        public CompressType compress;
        public int MaxCacheVersionCount = 8;
        public bool cleanHistory;
        public List<Object> buildInAssets = new List<Object>();

        public bool enableServer;
        public int serverPort = 8080;
        public List<EditorBundlePackage> pkgs = new List<EditorBundlePackage>();

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

        }

        static IEnumerable<Type> GetSubTypesInAssemblies(Type self)
        {
            if (self.IsInterface)
            {
                return from item in AppDomain.CurrentDomain.GetAssemblies().SelectMany((item) => item.GetTypes())
                       where item.GetInterfaces().Contains(self)
                       select item;
            }

            return from item in AppDomain.CurrentDomain.GetAssemblies().SelectMany((item) => item.GetTypes())
                   where item.IsSubclassOf(self)
                   select item;
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
        public List<string> GetAssetTags(string path)
        {
            if (tags == null)
                return null;
            return tags.FindAll(x => x.assets.Contains(path)).ConvertAll(x => x.tag);
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
