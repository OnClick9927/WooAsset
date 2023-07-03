using System.IO;
using System.Linq;
using UnityEditor;

namespace WooAsset
{
    public class CopyBundlesToStreamTask : AssetTask
    {
        public class CopyToStream : CopyBundleOperation
        {
            public CopyToStream(string srcPath, string destPath, bool cover) : base(srcPath, destPath, cover)
            {
            }
            protected override string GetDestFileName(FileInfo src)
            {
                return src.Name + ".bytes";
            }
        }
        protected async override void OnExecute(AssetTaskContext context)
        {
            string streamPath = AssetsInternal.CombinePath(UnityEngine.Application.streamingAssetsPath, context.buildTargetName);
            await new CopyToStream(context.outputPath, streamPath, true);
            AssetDatabase.Refresh();

            string local_ver_path = AssetsInternal.CombinePath(streamPath, context.localHashName + ".bytes");
            string remote_ver_path = AssetsInternal.CombinePath(streamPath, context.remoteHashName + ".bytes");

            var c = VersionBuffer.ReadAssetsVersionCollection(File.ReadAllBytes(remote_ver_path), context.encrypt);
            var data = c.versions.Last();
            VersionBuffer.WriteVersionData(data, local_ver_path, context.encrypt);
            AssetDatabase.Refresh();
            InvokeComplete();
        }
    }
}
