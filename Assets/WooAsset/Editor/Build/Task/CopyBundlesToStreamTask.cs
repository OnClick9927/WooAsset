using System.Linq;
using UnityEditor;

namespace WooAsset
{
    public class CopyBundlesToStreamTask : AssetTask
    {
        public class CopyToStream : CopyDirectoryOperation
        {
            string add;
            public CopyToStream(string srcPath, string destPath, bool cover, string add) : base(srcPath, destPath, cover)
            {
                this.add = add;
            }
            protected override async void Done()
            {
                await new YieldOperation();
                var list = this.files.Select(x => GetDestFileName(x)).ToList();
                list.Add(GetDestFileName(add));
                await AssetsHelper.WriteObject(new StreamBundleList()
                {
                    fileNames = list.ToArray(),
                },
                  AssetsHelper.CombinePath(destPath, StreamBundleList.fileName),
                  true
                  );
            }

            protected override string GetDestFileName(string src)
            {
                return base.GetDestFileName(src) + ".bytes";
            }
        }
        protected async override void OnExecute(AssetTaskContext context)
        {
            string streamPath = AssetsHelper.CombinePath(UnityEngine.Application.streamingAssetsPath, context.buildTargetName);
            string local_ver_path = AssetsHelper.CombinePath(streamPath, context.localHashName + ".bytes");
            string remote_ver_path = AssetsHelper.CombinePath(streamPath, context.remoteHashName + ".bytes");
            await new CopyToStream(context.outputPath, streamPath, true, local_ver_path);
            AssetDatabase.Refresh();


            var reader = await AssetsHelper.ReadFile(remote_ver_path, true);
            var c = VersionBuffer.ReadAssetsVersionCollection(reader.bytes, context.encrypt);
            var data = c.versions.Last();
            await VersionBuffer.WriteVersionData(data, local_ver_path, context.encrypt);
            AssetDatabase.Refresh();
            InvokeComplete();
        }
    }
}
