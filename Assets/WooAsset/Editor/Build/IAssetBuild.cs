using System.Collections.Generic;

namespace WooAsset
{
    public interface IAssetBuild
    {
        string GetVersion(string settingVersion, AssetTaskContext context);
        AssetType GetAssetType(string path);
        IReadOnlyList<string> GetTags(EditorAssetData info);
        void Create(List<EditorAssetData> assets, List<BundleGroup> result);

        List<AssetTask> GetPipelineEndTasks(AssetTaskContext context);
        List<AssetTask> GetPipelineStartTasks(AssetTaskContext context);
    }
}
