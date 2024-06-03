using System.Collections.Generic;
using System;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class AssetDatabaseMode : AssetMode
        {
            private class AssetDataBaseCheck : CheckBundleVersionOperation
            {
                protected override void Done() => InvokeComplete();
            }
            private class AssetDatabaseCompare : VersionCompareOperation
            {
                public AssetDatabaseCompare(VersionData version, List<PackageData> pkgs, VersionCompareType compareType) : base(version, pkgs, compareType) { }

                protected override void Compare()
                {
                    delete = new List<string>();
                    add = change = new List<BundleData>();
                    InvokeComplete();
                }
            }

            private AssetTask _task;

            public override ManifestData manifest => Initialized() ? cache.manifest : null;
            protected override bool Initialized() => _task != null && _task.isDone;
            protected override Operation CopyToSandBox(string from, string to) => Operation.empty;
            protected override Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs)
            {
                if (_task == null)
                    _task = AssetTaskRunner.PreviewAllBundles();
                return _task;
            }
            protected override CheckBundleVersionOperation LoadRemoteVersions() => new AssetDataBaseCheck();

            protected override Bundle CreateBundle(string bundleName, BundleLoadArgs args) => new EditorBundle(args);

            protected override VersionCompareOperation CompareVersion(VersionData version, List<PackageData> pkgs, VersionCompareType compareType) => new AssetDatabaseCompare(version, pkgs, compareType);
        }
    }

}
