using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    public interface IBuildPipeLine
    {
        bool BuildAssetBundles(string outputPath, AssetBundleBuild[] builds, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform);
        List<string> GetAllAssetBundles(BundleNameType nameType);
        List<string> GetAllDependencies(string assetBundleName, BundleNameType nameType);
        uint GetBundleCrc(string directory, string bundleName, BundleNameType nameType);
        string GetBundleHash(string directory, string bundleName, BundleNameType nameType);
        BuildAssetBundleOptions GetBundleOption(AssetTaskParams param, out string err);
    }
    public class BuildInPipeline : IBuildPipeLine
    {
        AssetBundleManifest _main;
        public bool BuildAssetBundles(string outputPath, AssetBundleBuild[] builds, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform)
        {
            try
            {
                _main = BuildPipeline.BuildAssetBundles(outputPath,
                builds, assetBundleOptions, targetPlatform);
                return true;
            }
            catch (System.Exception)
            {
                return false;
                throw;
            }
        }

        public List<string> GetAllAssetBundles(BundleNameType nameType) => _main.GetAllAssetBundles().ToList();

        public List<string> GetAllDependencies(string assetBundleName, BundleNameType nameType) => _main.GetAllDependencies(assetBundleName).ToList();

        public uint GetBundleCrc(string directory, string bundleName, BundleNameType nameType)
        {
            var filePath = AssetsEditorTool.CombinePath(directory, bundleName.Split("_").First());
            if (BuildPipeline.GetCRCForAssetBundle(filePath, out uint crc))
            {
                return crc;
            }
            return 0;
        }

        public string GetBundleHash(string directory, string bundleName, BundleNameType nameType)
        {
            return _main.GetAssetBundleHash(bundleName).ToString();
        }

        public BuildAssetBundleOptions GetBundleOption(AssetTaskParams param, out string err)
        {
            err = string.Empty;
            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;


            if (param.typeTreeOption == TypeTreeOption.DisableWriteTypeTree)
                opt |= BuildAssetBundleOptions.DisableWriteTypeTree;
            if (param.typeTreeOption == TypeTreeOption.IgnoreTypeTreeChanges)
                opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;

            if (param.buildMode == BuildMode.ForceRebuild)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
            if (param.compress == CompressType.LZ4)
                opt |= BuildAssetBundleOptions.ChunkBasedCompression;
            if (param.compress == CompressType.Uncompressed)
                opt |= BuildAssetBundleOptions.UncompressedAssetBundle;

            if (param.Pipeline == TaskPipelineType.DryBuild)
                opt = BuildAssetBundleOptions.DryRunBuild;
            if (param.bundleNameType == BundleNameType.NameWithHash || param.bundleNameType == BundleNameType.Hash)
                opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName;

            return opt;
        }
    }
}
