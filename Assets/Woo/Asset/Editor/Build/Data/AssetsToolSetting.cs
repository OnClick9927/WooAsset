using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    class AssetsToolSetting : AssetsScriptableObject
    {
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
            public bool readable = false;
            public bool generateMipMaps = false;
            public bool sRGB = true;
            public FilterMode filterMode = FilterMode.Bilinear;
            public int anisoLevel = 1;
        }

        [Tooltip("Asset Database Load In Editor")]
        public bool fastMode = true;
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


    }
}
