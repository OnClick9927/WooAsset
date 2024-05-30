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
    }
}
