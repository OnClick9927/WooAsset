using System.Collections.Generic;
using System.Linq;

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
                    public FastCompare(CheckBundleVersionOperation _check, int index, params string[] names) : base(_check, index, names)
                    {
                    }
                    protected override void Compare()
                    {
                        change = new List<FileData>();
                        delete = new List<FileData>();
                        add = new List<FileData>();
                        InvokeComplete();
                    }
                }
                List<AssetsVersionCollection.VersionData> _versions = new List<AssetsVersionCollection.VersionData>();
                public override List<AssetsVersionCollection.VersionData> versions => _versions;
                protected override void Done()
                {
                    AssetsHelper.Log($"Check Version Complete");
                    InvokeComplete();
                }
                public override VersionCompareOperation Compare(int versionIndex, params string[] tags)
                {
                    return new FastCompare(null, versionIndex, tags);
                }
            }

            private AssetTask _task;

            protected override ManifestData manifest => Initialized() ? cache.manifest : null;
            protected override bool Initialized() => _task != null && _task.isDone;
            protected override CopyStreamBundlesOperation CopyToSandBox(string from, string to) => new FastCopy(from, to);
            protected override AssetHandle CreateAsset(string assetPath, AssetLoadArgs arg) => arg.scene == true ? new EditorSceneAsset(arg) as AssetHandle : new EditorAsset(arg);
            protected override Operation InitAsync(string version, bool again, string[] tags)
            {
                if (_task == null)
                    _task = AssetTaskRunner.PreviewBundles();
                return _task;
            }
            protected override CheckBundleVersionOperation VersionCheck() => new FastCheck();
        }
    }

}
