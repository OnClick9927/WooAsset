using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        internal class CopyToStream : CopyDirectoryOperation
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
                await Task.Delay(1);
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

                await WriteObject(new StreamBundleList()
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
                return base.GetDestFileName(src) + CopyStreamBundlesOperation.fileExt;
            }
        }
    }
}
