using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class FastAssetMode : CheckBundleVersionOperation, IAssetMode
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


            public override float progress => 1;
            public bool Initialized()
            {
                return true;
            }

            public AssetOperation InitAsync(string version, bool again, string[] tags)
            {
                if (!isDone)
                    InvokeComplete();
                return this;
            }
            List<AssetsVersionCollection.VersionData> _versions = new List<AssetsVersionCollection.VersionData>();
            public override List<AssetsVersionCollection.VersionData> versions => _versions;
            public CheckBundleVersionOperation VersionCheck()
            {
                return this;
            }
            protected override void Done()
            {
                AssetsInternal.Log($"Check Version Complete");
                InvokeComplete();
            }

            public AssetHandle CreateAsset(string assetPath, AssetLoadArgs arg)
            {
                if (arg.scene)
                    return new EditorSceneAsset(arg);
                return new EditorAsset(arg);
            }

            public IReadOnlyList<string> GetAllAssetPaths()
            {
                return cache.tree.GetAllAssets().FindAll(x => x.type != AssetType.Directory).ConvertAll(asset => asset.path);
            }
            public IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result)
            {
                result = GetAllAssetPaths().Where(x => Path.GetFileName(name).Contains(name)).ToList();
                return result;
            }

            public IReadOnlyList<string> GetTagAssetPaths(string tag)
            {
                return cache.tags.GetTagAssetPaths(tag);
            }

            public IReadOnlyList<string> GetAssetTags(string assetPath)
            {
                return cache.tags.GetAssetTags(assetPath);
            }

            public IReadOnlyList<string> GetAllTags()
            {
                return cache.tags.GetAllTags();
            }

            public IReadOnlyList<string> GetAssetDependencies(string assetPath)
            {
                return cache.tree.GetAssetData(assetPath)?.dps;
            }

            public IReadOnlyList<string> GetAllAssetPaths(string bundleName)
            {
                return cache.previewBundles.Find(x => x.hash == bundleName)?.GetAssets();
            }



            public CopyBundleOperation CopyToSandBox(string from, string to, bool cover)
            {
                return new FastCopy(from, to, cover);
            }
            public override VersionCompareOperation Compare(int versionIndex, params string[] tags)
            {
                return new FastCompare(null, versionIndex, tags);
            }


            public AssetType GetAssetType(string assetPath)
            {
                return cache.tree.GetAssetData(assetPath).type;
            }

       
        }
    }

}
