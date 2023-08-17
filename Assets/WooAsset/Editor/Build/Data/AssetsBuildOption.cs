using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEditor.U2D;
using UnityEditor;

namespace WooAsset
{
    public class AssetsBuildOption : AssetsScriptableObject
    {
        [System.Serializable]
        public class BuildGroup
        {
            public bool build;
            public string name;
            public string path;
            public string description;
            public List<string> tags = new List<string>();
            public string GetManifestFileName(string version)
            {
                return AssetsHelper.GetStringHash("m" + path + version);
            }
            public string GetBundleFileName(string version)
            {
                return AssetsHelper.GetStringHash("b" + path + version);
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
        [Tooltip("ShaderVariant")]
        [Space(20)]
        public string shaderVariantDirectory;
        [Space(20)]
        public List<string> atlasPaths = new List<string>();
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

        [Space(20)]
        public string version = "0.0.1";
        public long bundleSize = 8 * 1024 * 1024;
        public bool forceRebuild = false;

        public bool ignoreTypeTreeChanges = true;
        public CompressType compress;
        public int MaxCacheVersionCount = 8;
        public bool cleanHistory;

        [SerializeField] public List<BuildGroup> buildGroups = new List<BuildGroup>();

        [HideInInspector] public TypeSelect build = new TypeSelect();
        [HideInInspector] public TypeSelect mode = new TypeSelect();
        [HideInInspector] public TypeSelect encrypt = new TypeSelect();



        public bool enableServer;
        public string serverDirectory = "DLC/Server";
        public int serverPort = 8080;

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


    }
}
