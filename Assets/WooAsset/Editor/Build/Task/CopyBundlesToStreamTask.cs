using System.Collections.Generic;
using UnityEditor;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    public class CopyBundlesToStreamTask : AssetTask
    {
        protected async override void OnExecute(AssetTaskContext context)
        {
            if (context.isNormalBuildMode && context.copyToStream)
            {
                string streamPath = context.streamBundleDirectory;
                var buildInAssets = context.buildInAssets;
                ManifestData manifest = context.mergedManifest;

                List<string> dps = new List<string>();
                foreach (var item in buildInAssets)
                {
                    var _dps = context.assetsCollection.GetAssetData(item).dependence;
                    if (_dps != null)
                        dps.AddRange(_dps);
                }
                List<string> buildInBundles = new List<string>();
                buildInAssets.AddRange(dps);
                foreach (var assetPath in buildInAssets)
                {
                    var assetData = manifest.GetAssetData(assetPath);
                    if (assetData == null)
                    {
                        SetErr($"could not find asset in this build {assetPath}");
                        InvokeComplete();
                        return;
                    }

                    var bundleName = assetData.bundleName;
                    if (buildInBundles.Contains(bundleName)) continue;
                    buildInBundles.Add(bundleName);
                }
                List<string> buildInConfigs = new List<string>
                {
                    VersionHelper.VersionDataName,
                    VersionHelper.VersionCollectionName,
                };
                foreach (var item in context.buildPkgs)
                {
                    buildInConfigs.Add(VersionHelper.GetManifestFileName(item.name));
                }
                if (AssetsEditorTool.ExistsDirectory(streamPath))
                    AssetsEditorTool.DeleteDirectory(streamPath);

                await new CopyToStream(context.outputPath, streamPath, buildInBundles, buildInConfigs);
                AssetDatabase.Refresh();


                AssetDatabase.Refresh();
            }

            InvokeComplete();
        }
    }
}
