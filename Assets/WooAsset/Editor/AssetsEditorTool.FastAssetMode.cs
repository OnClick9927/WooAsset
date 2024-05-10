using System.Collections.Generic;
using System.Linq;
using static WooAsset.AssetsVersionCollection.VersionData;
using static WooAsset.AssetsVersionCollection;
using System;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class FastAssetMode : AssetMode
        {
            private class FastCopy : CopyStreamBundlesOperation
            {
                public FastCopy(string srcPath, string destPath) : base(srcPath, destPath)
                {
                }
                protected override void Copy()
                {
                    InvokeComplete();
                }
            }

            private class FastCheck : CheckBundleVersionOperation
            {
                private class FastCompare : VersionCompareOperation
                {
                    public FastCompare(CheckBundleVersionOperation _check, VersionData version, List<PackageData> pkgs) : base(_check, version, pkgs, null)
                    {
                    }
                    protected override void Compare()
                    {
                        add = delete = change = new List<FileData>();
                        InvokeComplete();
                    }
                }
                protected override void Done()
                {
                    AssetsHelper.Log($"Check Version Complete");
                    InvokeComplete();
                }
                public override VersionCompareOperation Compare(VersionData version, List<PackageData> pkgs)
                {
                    return new FastCompare(null, version, pkgs);
                }

            }

            private AssetTask _task;

            public override ManifestData manifest => Initialized() ? cache.manifest : null;
            protected override bool Initialized() => _task != null && _task.isDone;
            protected override CopyStreamBundlesOperation CopyToSandBox(string from, string to) => new FastCopy(from, to);
            protected override Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs)
            {
                if (_task == null)
                    _task = AssetTaskRunner.PreviewAllBundles();
                return _task;
            }
            protected override CheckBundleVersionOperation VersionCheck() => new FastCheck();

            protected override Bundle CreateBundle(string bundleName, BundleLoadArgs args)
            {
                return new EditorBundle(args);
            }
        }
    }

}
