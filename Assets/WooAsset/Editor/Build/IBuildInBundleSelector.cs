using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    public interface IBuildInBundleSelector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="files">打包出的所有文件</param>
        /// <param name="buildInAssets">编辑器配置</param>
        /// <param name="buildInConfig">必须拷贝的文件</param>
        /// <param name="manifest">打包出来的配置文件（Merged）</param>
        /// <param name="exports">打包报告（Merged）</param>
        /// <returns></returns>
        string[] Select(string[] files, List<string> buildInAssets, List<string> buildInConfig, ManifestData manifest, List<PackageExportData> exports);
    }
    public class DefaultBuildInBundleSelector : IBuildInBundleSelector
    {
        public string[] Select(string[] files, List<string> buildInAssets, List<string> buildInConfig, ManifestData manifest, List<PackageExportData> exports)
        {

            var buildInBundles = new List<string>();

            foreach (var assetPath in buildInAssets)
            {
                var assetData = manifest.GetAssetData(assetPath);
                if (assetData == null)
                {
                    AssetsEditorTool.LogError($"build-in asset are not build in bundles {assetPath}");
                    return files;
                }
                else
                {
                    buildInBundles.Add(assetData.bundleName);
                    var _dps = manifest.GetBundleData(assetData.bundleName).dependence;
                    if (_dps != null)
                        buildInBundles.AddRange(_dps);
                }
            }


            if (buildInBundles != null && buildInBundles.Count > 0)
            {
                foreach (var bundleName in buildInBundles)
                {
                    if (files.Where(x => AssetsEditorTool.GetFileName(x) == bundleName).Count() == 0)
                    {
                        AssetsEditorTool.LogError($"the bundle want copy not build {bundleName}");
                        return files;
                    }
                }
            }

            if (buildInBundles != null && buildInBundles.Count > 0)
            {
                return files
                     .Where(x => buildInBundles.Contains(AssetsEditorTool.GetFileName(x))
                     || buildInConfig.Contains(AssetsEditorTool.GetFileName(x)))
                     .ToArray();
            }
            return files;
        }
    }
}
