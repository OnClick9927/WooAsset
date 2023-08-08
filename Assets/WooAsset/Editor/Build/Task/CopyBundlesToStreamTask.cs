using System.Linq;
using UnityEditor;
using static WooAsset.AssetsHelper;

namespace WooAsset
{
    public class CopyBundlesToStreamTask : AssetTask
    {
        public class CopyToStream : CopyBundleOperation
        {
            public CopyToStream(string srcPath, string destPath, bool cover) : base(srcPath, destPath, cover)
            {
            }
            protected override string GetDestFileName(string src)
            {
                return base.GetDestFileName(src) + ".bytes";
            }
        }
        protected async override void OnExecute(AssetTaskContext context)
        {
            string streamPath = AssetsHelper.CombinePath(UnityEngine.Application.streamingAssetsPath, context.buildTargetName);
            await new CopyToStream(context.outputPath, streamPath, true);
            AssetDatabase.Refresh();

            string local_ver_path = AssetsHelper.CombinePath(streamPath, context.localHashName + ".bytes");
            string remote_ver_path = AssetsHelper.CombinePath(streamPath, context.remoteHashName + ".bytes");

            var reader = await AssetsHelper.ReadFile(remote_ver_path, true);
            var c = VersionBuffer.ReadAssetsVersionCollection(reader.bytes, context.encrypt);
            var data = c.versions.Last();
            await VersionBuffer.WriteVersionData(data, local_ver_path, context.encrypt);
            AssetDatabase.Refresh();
            InvokeComplete();
        }
    }
}
