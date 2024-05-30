using System.Collections.Generic;
using System;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class FastAssetMode : AssetMode
        {
            private class FastCopy : CopyStreamBundlesOperation
            {
                public FastCopy(string srcPath, string destPath) : base(srcPath, destPath) { }
                protected override void Copy() => InvokeComplete();
            }

            private class FastCheck : CheckBundleVersionOperation
            {
                protected override void Done() => InvokeComplete();
            }
            private class FastCompare : VersionCompareOperation
            {
                public FastCompare(VersionData version, List<PackageData> pkgs) : base(version, pkgs) { }

                protected override void Compare()
                {
                    delete = new List<FileData>();
                    add = change = new List<BundleFileData>();
                    InvokeComplete();
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
            protected override CheckBundleVersionOperation LoadRemoteVersions() => new FastCheck();

            protected override Bundle CreateBundle(string bundleName, BundleLoadArgs args) => new EditorBundle(args);

            protected override VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs) => new FastCompare(version, pkgs);
        }
    }

}
