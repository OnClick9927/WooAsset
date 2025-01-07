using System;
using System.Collections.Generic;

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
    [System.Serializable]

    public class AssetSelectorParam
    {
        public enum SelectType
        {
            Intersect, Union
        }
        public int typeIndex;



        public SelectType type;

        public AssetType assetType;
        public string path;
        public string tag;
        public string userData;
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AssetSelectorAttribute : Attribute
    {
        public readonly AssetSelectorParamType type;

        public AssetSelectorAttribute(AssetSelectorParamType include)
        {
            this.type = include;
        }
    }
    public interface IAssetSelector
    {
        List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param);
    }
    [AssetSelector(AssetSelectorParamType.None)]
    public class AllAssetSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return new List<EditorAssetData>(assets);
        }
    }



    [AssetSelector(AssetSelectorParamType.Tag)]
    public class TagSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return assets.FindAll(x => x.tags != null && x.tags.Contains(param.tag));
        }
    }

    [AssetSelector(AssetSelectorParamType.Path)]
    public class DirectoryTopSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return assets.FindAll(x => x.directory == param.path);
        }
    }
    [AssetSelector(AssetSelectorParamType.Path)]

    public class DirectoryDeepSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return assets.FindAll(x => x.directory.StartsWith(param.path));
        }
    }



    [AssetSelector(AssetSelectorParamType.AssetType)]
    public class AssetTypeSelector : IAssetSelector
    {
        public List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param)
        {
            return assets.FindAll(x => x.type == param.assetType);
        }
    }

}
