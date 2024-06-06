using System;

namespace WooAsset
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AssetSelectorAttribute : Attribute
    {
        public readonly AssetSelectorParamType type;

        public AssetSelectorAttribute(AssetSelectorParamType include)
        {
            this.type = include;
        }
    }
}
