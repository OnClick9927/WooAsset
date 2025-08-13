

# SBP支持

* [官方的文档](https://docs.unity3d.com/Packages/com.unity.scriptablebuildpipeline@1.20/manual/GettingStarted.html)
* 使用unity package manager 安装  Scriptable Build Pipeline包
* 把下面的代码贴到项目内的某个Editor文件夹下
* 把Option页签 build 下的 build pipeline 选择为 SBPPipeline

``` csharp

    public class SBPPipeline : IBuildPipeLine
    {
        UnityEngine.Build.Pipeline.CompatibilityAssetBundleManifest _main;
        public bool BuildAssetBundles(string outputPath, AssetBundleBuild[] builds, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform)
        {
            try
            {
                _main = CompatibilityBuildPipeline.BuildAssetBundles(outputPath, builds, assetBundleOptions, targetPlatform);

                var bundles = _main.GetAllAssetBundles();
                foreach (var bundle in bundles)
                {
                    var hash = _main.GetAssetBundleHash(bundle).ToString();
                    name2Hash[bundle] = hash;
                }
                return true;
            }
            catch (System.Exception)
            {
                return false;
                throw;
            }
        }
        Dictionary<string, string> name2Hash = new Dictionary<string, string>();

        public List<string> GetAllAssetBundles(BundleNameType nameType)
        {
            var bundles = _main.GetAllAssetBundles();
            switch (nameType)
            {
                case BundleNameType.Name: return bundles.ToList();
                case BundleNameType.NameWithHash:
                case BundleNameType.Hash:
                    return bundles.Select(name => $"{name}_{name2Hash[name]}").ToList();
                default:
                    return null;
            }
        }

        public List<string> GetAllDependencies(string assetBundleName, BundleNameType nameType)
        {
            switch (nameType)
            {
                case BundleNameType.Name:
                    return _main.GetAllDependencies(assetBundleName).ToList();
                case BundleNameType.NameWithHash:
                case BundleNameType.Hash:
                    {
                        assetBundleName = assetBundleName.Split("_")[0];
                        return _main.GetAllDependencies(assetBundleName).Select(name => $"{name}_{name2Hash[name]}").ToList();
                    }
                default:
                    return null;
            }
        }

        public BuildAssetBundleOptions GetBundleOption(AssetTaskParams param, out string err)
        {
            err = string.Empty;
            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            //opt |= BuildAssetBundleOptions.StrictMode;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;


            if (param.typeTreeOption == TypeTreeOption.DisableWriteTypeTree)
                opt |= BuildAssetBundleOptions.DisableWriteTypeTree;
            if (param.typeTreeOption == TypeTreeOption.IgnoreTypeTreeChanges)
            {
                //opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
                err = "SBP Not Support IgnoreTypeTreeChanges";

            }

            if (param.buildMode == BuildMode.ForceRebuild)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
            if (param.compress == CompressType.LZ4)
                opt |= BuildAssetBundleOptions.ChunkBasedCompression;
            if (param.compress == CompressType.Uncompressed)
                opt |= BuildAssetBundleOptions.UncompressedAssetBundle;

            if (param.Pipeline == TaskPipelineType.DryBuild)
            {
                //opt = BuildAssetBundleOptions.DryRunBuild;
                err = "SBP Not Support DryBuild";
            }
            if (param.bundleNameType == BundleNameType.NameWithHash || param.bundleNameType == BundleNameType.Hash)
                opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName;

            return opt;
        }

        public uint GetBundleCrc(string directory, string bundleName, BundleNameType nameType)
        {
            switch (nameType)
            {
                case BundleNameType.Name:
                    return _main.GetAssetBundleCrc(bundleName);
                case BundleNameType.NameWithHash:
                case BundleNameType.Hash:
                    {
                        bundleName = bundleName.Split("_")[0];
                        return _main.GetAssetBundleCrc(bundleName);
                    }
                default:
                    return 0;
            }
        }

        public string GetBundleHash(string directory, string bundleName, BundleNameType nameType)
        {
            switch (nameType)
            {
                case BundleNameType.Name:
                    return _main.GetAssetBundleHash(bundleName).ToString();
                case BundleNameType.NameWithHash:
                case BundleNameType.Hash:
                    {
                        bundleName = bundleName.Split("_")[0];
                        return _main.GetAssetBundleHash(bundleName).ToString();
                    }
                default:
                    return string.Empty;
            }
        }
    }


```