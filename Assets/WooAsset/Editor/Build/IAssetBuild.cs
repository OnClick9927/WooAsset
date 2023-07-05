﻿using System.Collections.Generic;

namespace WooAsset
{
    public interface IAssetBuild
    {
        string GetVersion(string settingVersion, AssetTaskContext context);

        AssetType GetAssetType(string path);
        IReadOnlyList<string> GetTags(EditorAssetData info);
        void Create(AssetTagCollection tags,List<EditorAssetData> assets, Dictionary<EditorAssetData, List<EditorAssetData>> dpsDic, List<BundleGroup> result);

        List<AssetTask> GetPipelineFinishTasks(AssetTaskContext context);


    }
}