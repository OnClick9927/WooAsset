using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        internal class CopyToStreamCMD : CopyDirectoryCMD
        {
            private readonly List<string> buildInBundles;
            private readonly List<string> buildInConfigs;

            public CopyToStreamCMD(string srcPath, string targetPath, List<string> buildInBundles, List<string> buildInConfigs)
                : base(srcPath, targetPath)
            {
                this.buildInBundles = buildInBundles;
                this.buildInConfigs = buildInConfigs;
            }
            protected override void Done()
            {
                if (buildInBundles != null && buildInBundles.Count > 0)
                {
                    foreach (var bundleName in buildInBundles)
                    {
                        if (files.Where(x => AssetsHelper.GetFileName(x) == bundleName).Count() == 0)
                        {
                            AssetsHelper.LogError($"the bundle want copy not build {bundleName}");
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
                var list = this.files.Select(x => GetTargetFileName(x)).ToList();
                AssetsEditorTool.WriteObjectSync(new StreamBundlesData()
                {
                    fileNames = list.ToArray(),
                },
                AssetsHelper.CombinePath(targetPath, StreamBundlesData.fileName));
                base.Done();
            }

            protected override string GetTargetFileName(string src)
            {
                return $"{base.GetTargetFileName(src)}{StreamBundlesData.fileExt}";
            }
        }
    }
}
