using UnityEditor.U2D;
using UnityEngine;

namespace WooAsset
{
    [System.Serializable]
    public class TextureSetting
    {
        public bool readable = true;
        public bool generateMipMaps = false;
        public bool sRGB = true;
        public FilterMode filterMode = FilterMode.Bilinear;
        public int anisoLevel = 1;

        public static implicit operator SpriteAtlasTextureSettings(TextureSetting textureSetting)
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
