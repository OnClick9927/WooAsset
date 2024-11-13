using System.Collections.Generic;
using UnityEditor;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    public class CopyBundlesToStreamTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            if (context.Pipeline == TaskPipelineType.BuildBundle && context.copyToStream)
            {
                string streamPath = context.streamBundleDirectory;
                var buildInAssets = context.buildInAssets;
                ManifestData manifest = context.mergedManifest;

                List<string> dps = new List<string>();
                foreach (var assetPath in buildInAssets)
                {
                    var assetData = context.assetsCollection.GetAssetData(assetPath);
                    if (assetData == null)
                    {
                        SetErr($"build-in asset are not build in bundles {assetPath}");
                        InvokeComplete();
                        return;
                    }
                    else
                    {
                        var _dps = assetData.dependence;
                        if (_dps != null)
                            dps.AddRange(_dps);
                    }
                }
                List<string> buildInBundles = new List<string>();
                buildInAssets.AddRange(dps);
                foreach (var assetPath in buildInAssets)
                {
                    var assetData = manifest.GetAssetData(assetPath);
                    var bundleName = assetData.bundleName;
                    if (buildInBundles.Contains(bundleName)) continue;
                    buildInBundles.Add(bundleName);
                }
                List<string> buildInConfigs = new List<string>
                {
                    AssetsEditorTool.VersionDataName,
                    AssetsEditorTool.VersionCollectionName,
                };
                foreach (var item in context.buildPkgs)
                {
                    buildInConfigs.Add(AssetsEditorTool.GetManifestFileName(item.name));
                }
                new CopyToStreamCMD(context.outputPath, streamPath, buildInBundles, buildInConfigs);
                AssetDatabase.Refresh();


            }

            InvokeComplete();
        }
    }
}
