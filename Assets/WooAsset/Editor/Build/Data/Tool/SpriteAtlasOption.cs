using System.Collections.Generic;
using UnityEditor;

namespace WooAsset
{
    public class SpriteAtlasOption : AssetsScriptableObject
    {
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

    }
}
