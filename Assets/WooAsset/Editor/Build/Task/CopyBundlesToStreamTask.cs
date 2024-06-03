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
                string local_ver_path = AssetsHelper.CombinePath(streamPath, context.VersionDataName + ".bytes");
                string remote_ver_path = AssetsHelper.CombinePath(context.outputPath, context.VersionCollectionName);
                var buildInAssets = context.buildInAssets.ConvertAll(x => AssetDatabase.GetAssetPath(x));
                var manifests = context.exports.ConvertAll(x => x.manifest);
                ManifestData manifest = new ManifestData();
                foreach (var item in manifests)
                    ManifestData.Merge(item, manifest, null);



                manifest.Prepare();
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
                    VersionHelper.VersionDataName
                };
                foreach (var item in context.buildPkgs)
                {
                    buildInConfigs.Add(VersionHelper.GetManifestFileName(item.name));
                    //buildInConfigs.Add(VersionHelper.GetBundleFileName(item.name));
                }


                await new CopyToStream(context.outputPath, streamPath, true, AssetsHelper.GetFileName(local_ver_path), buildInBundles, buildInConfigs);
                AssetDatabase.Refresh();


                var reader = AssetsHelper.ReadFile(remote_ver_path, true);
                await reader;
                //var c = VersionHelper.ReadAssetsVersionCollection(reader.bytes);
                //var data = c.NewestVersion();
                //await VersionHelper.WriteVersionData(data, local_ver_path);
                AssetDatabase.Refresh();
            }

            InvokeComplete();
        }
    }
}
