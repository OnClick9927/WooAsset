using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        internal class CopyToStream : CopyDirectoryOperation
        {
            private readonly List<string> buildInBundles;
            private readonly List<string> buildInConfigs;

            public CopyToStream(string srcPath, string destPath, List<string> buildInBundles, List<string> buildInConfigs) : base(srcPath, destPath, true)
            {
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
                await WriteObject(new StreamBundlesData()
                {
                    fileNames = list.ToArray(),
                },
                  AssetsHelper.CombinePath(destPath, StreamBundlesData.fileName)
                  );
                base.Done();
            }

            protected override string GetDestFileName(string src)
            {
                return $"{base.GetDestFileName(src)}{StreamBundlesData.fileExt}";
            }
        }
    }
}
