using System;

namespace WooAsset
{
    [Flags]
    public enum AssetSelectorParamType
    {
        None = 1,
        AssetType = 1 << 1,
        Path = 1 << 2,
        Tag = 1 << 3,
        UserData = 1 << 4,
    }
}
