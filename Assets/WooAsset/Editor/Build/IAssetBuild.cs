using System.Collections.Generic;

namespace WooAsset
{
    public interface IAssetBuild
    {
        string GetVersion(string settingVersion, AssetTaskContext context);
        bool IsIgnorePath(string path);
        AssetType GetAssetType(string path);
        IReadOnlyList<string> GetTags(EditorAssetData info);
        void Create(AssetTagCollection tags, List<EditorAssetData> assets, List<BundleGroup> result);

        List<AssetTask> GetPipelineFinishTasks(AssetTaskContext context);


    }
}
