using UnityEditor.U2D;

namespace WooAsset
{
    [System.Serializable]

    public class PackingSetting
    {
        public int blockOffset = 1;
        public bool enableRotation = false;
        public bool enableTightPacking = false;
        public int padding = 2;

        public static implicit operator SpriteAtlasPackingSettings(PackingSetting packSetting)
        {
            return new SpriteAtlasPackingSettings()
            {
                blockOffset = packSetting.blockOffset,
                enableRotation = packSetting.enableRotation,
                enableTightPacking = packSetting.enableTightPacking,
                padding = packSetting.padding,
            };
        }
    }
}
