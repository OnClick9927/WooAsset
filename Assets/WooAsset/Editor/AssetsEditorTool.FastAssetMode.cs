using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class FastAssetMode : IAssetMode
        {
            private class FastCopy : CopyBundleOperation
            {
                public FastCopy(string srcPath, string destPath, bool cover) : base(srcPath, destPath, cover)
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
                    AssetsInternal.Log($"Check Version Complete");
                    InvokeComplete();
                }
                public override VersionCompareOperation Compare(int versionIndex, params string[] tags)
                {
                    return new FastCompare(null, versionIndex, tags);
                }
            }

            bool IAssetMode.Initialized() => _task != null && _task.isDone;
            private AssetTask _task;

            AssetOperation IAssetMode.InitAsync(string version, bool again, string[] tags)
            {
                if (_task == null)
                    _task = AssetTaskRunner.PreviewBundles();
                return _task;
            }

            CheckBundleVersionOperation IAssetMode.VersionCheck() => new FastCheck();


            AssetHandle IAssetMode.CreateAsset(string assetPath, AssetLoadArgs arg) => arg.scene == true ? new EditorSceneAsset(arg) as AssetHandle : new EditorAsset(arg);

            IReadOnlyList<string> IAssetMode.GetAllAssetPaths() => cache.tree.GetAllAssets().FindAll(x => x.type != AssetType.Directory).ConvertAll(asset => asset.path);
            IReadOnlyList<string> IAssetMode.GetAssetsByAssetName(string name, List<string> result) => ((IAssetMode)this).GetAllAssetPaths().Where(x => Path.GetFileName(name).Contains(name)).ToList();

            IReadOnlyList<string> IAssetMode.GetTagAssetPaths(string tag) => cache.tags.GetTagAssetPaths(tag);

            IReadOnlyList<string> IAssetMode.GetAssetTags(string assetPath) => cache.tags.GetAssetTags(assetPath);

            IReadOnlyList<string> IAssetMode.GetAllTags() => cache.tags.GetAllTags();

            IReadOnlyList<string> IAssetMode.GetAssetDependencies(string assetPath) => cache.tree.GetAssetData(assetPath)?.dependence;

            IReadOnlyList<string> IAssetMode.GetAllAssetPaths(string bundleName) => cache.previewBundles.Find(x => x.hash == bundleName)?.GetAssets();



            CopyBundleOperation IAssetMode.CopyToSandBox(string from, string to, bool cover) => new FastCopy(from, to, cover);



            AssetType IAssetMode.GetAssetType(string assetPath) => cache.tree.GetAssetType(assetPath);

            bool IAssetMode.ContainsAsset(string assetPath) => cache.tree.ContainsAsset(assetPath);

            public UnzipRawFileOperation UnzipRawFile()
            {
                if (!((IAssetMode)this).Initialized())
                {
                    AssetsInternal.LogError("InitAsync Filrst ");
                    return null;
                }
                return new UnzipRawFileOperation(cache.tree.GetRawAssets_Copy());
            }
        }
    }

}
