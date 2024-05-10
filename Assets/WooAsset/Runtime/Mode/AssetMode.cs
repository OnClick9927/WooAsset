using static WooAsset.AssetsVersionCollection.VersionData;
using static WooAsset.AssetsVersionCollection;
using System.Collections.Generic;
using System;
using System.Xml.Linq;

namespace WooAsset
{
    public abstract class AssetMode : IAssetMode
    {
        bool IAssetMode.Initialized() => Initialized();
        UnzipRawFileOperation IAssetMode.UnzipRawFile() => new UnzipRawFileOperation(manifest.rawAssets_copy);


        CopyStreamBundlesOperation IAssetMode.CopyToSandBox(string from, string to) => CopyToSandBox(from, to);
        AssetHandle IAssetMode.CreateAsset(AssetLoadArgs arg)
        {
            string path = arg.path;
            if (!path.StartsWith("Assets") || path.Contains("Resources"))
            {
                if (path.Contains("Resources"))
                {
                    var index = arg.path.LastIndexOf("Resources");
                    arg.path = arg.path.Remove(0, index + "Resources".Length + 1);
                }
                return new ResourcesAsset(arg);
            }


            if (!arg.direct)
            {
                string bundleName = AssetsInternal.GetAssetBundleName(path);
                if (string.IsNullOrEmpty(bundleName))
                    AssetsHelper.LogError($"Not Found  {path}");
                arg.bundleName = bundleName;
            }
            if (arg.scene)
                return new SceneAsset(arg);
            return new Asset(arg);

        }
        Operation IAssetMode.InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs) => InitAsync(version, again, getPkgs);
        CheckBundleVersionOperation IAssetMode.VersionCheck() => VersionCheck();
        Bundle IAssetMode.CreateBundle(string bundleName, BundleLoadArgs args) => CreateBundle(bundleName, args);

        public abstract ManifestData manifest { get; }
        protected abstract bool Initialized();
        protected abstract CopyStreamBundlesOperation CopyToSandBox(string from, string to);
        protected abstract Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs);
        protected abstract CheckBundleVersionOperation VersionCheck();
        protected abstract Bundle CreateBundle(string bundleName, BundleLoadArgs args);

    }

}
