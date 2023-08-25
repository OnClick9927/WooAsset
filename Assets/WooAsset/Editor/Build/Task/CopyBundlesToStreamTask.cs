using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace WooAsset
{
    public class CopyBundlesToStreamTask : AssetTask
    {
        public class CopyToStream : CopyDirectoryOperation
        {
            string local_v_name;
            private readonly List<string> buildInBundles;
            private readonly List<string> buildInConfigs;

            public CopyToStream(string srcPath, string destPath, bool cover, string local_v_name, List<string> buildInBundles, List<string> buildInConfigs) : base(srcPath, destPath, cover)
            {
                this.local_v_name = local_v_name;
                this.buildInBundles = buildInBundles;
                this.buildInConfigs = buildInConfigs;
            }
            protected override async void Done()
            {
                await new YieldOperation();
                if (buildInBundles != null && buildInBundles.Count > 0)
                {
                    foreach (var bundleName in buildInBundles)
                    {
                        if (files.Where(x => AssetsHelper.GetFileName(x) == bundleName).Count() == 0)
                        {
                            SetErr($"the bundle want copy not build {bundleName}");
                            InvokeComplete();
                            return;
                        }
                    }
                }

                if (buildInBundles != null && buildInBundles.Count > 0)
                {
                    this.files = this.files
                        .ToList()
                        .Where(x => buildInBundles.Contains(AssetsHelper.GetFileName(x)) || buildInConfigs.Contains(AssetsHelper.GetFileName(x)))
                        .ToArray();
                }
                var list = this.files.Select(x => GetDestFileName(x)).ToList();
                list.Add(local_v_name);

                await AssetsHelper.WriteObject(new StreamBundleList()
                {
                    fileNames = list.ToArray(),
                },
                  AssetsHelper.CombinePath(destPath, StreamBundleList.fileName),
                  true
                  );
                base.Done();
            }

            protected override string GetDestFileName(string src)
            {
                return base.GetDestFileName(src) + ".bytes";
            }
        }
        protected async override void OnExecute(AssetTaskContext context)
        {
            string streamPath = context.streamBundleDirectory;
            string local_ver_path = AssetsHelper.CombinePath(streamPath, context.localHashName + ".bytes");
            string remote_ver_path = AssetsHelper.CombinePath(context.outputPath, context.remoteHashName);
            var buildInAssets = context.buildInAssets.ConvertAll(x => AssetDatabase.GetAssetPath(x));
            var manifests = context.exports.ConvertAll(x => x.manifest);
            ManifestData manifest = ManifestData.Merge(manifests, new List<string>());
            manifest.Prepare();
            List<string> dps = new List<string>();
            foreach (var item in buildInAssets)
            {
                var _dps = context.tree.GetAssetData(item).dependence;
                if (_dps != null)
                    dps.AddRange(_dps);
            }
            List<string> buildInBundles = new List<string>();
            buildInAssets.AddRange(dps);
            foreach (var item in buildInAssets)
            {
                if (manifest.GetAssetData(item) == null)
                {
                    SetErr($"could not find asset in this build {item}");
                    InvokeComplete();
                    return;
                }
                var b = manifest.GetAssetBundleName(item);
                if (buildInBundles.Contains(b)) continue;
                buildInBundles.Add(b);
            }
            List<string> buildInConfigs = new List<string>();

            foreach (var item in context.buildGroups)
            {
                buildInConfigs.Add(item.GetManifestFileName(context.version));
                buildInConfigs.Add(item.GetBundleFileName(context.version));
            }


            await new CopyToStream(context.outputPath, streamPath, true, AssetsHelper.GetFileName(local_ver_path), buildInBundles, buildInConfigs);
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
