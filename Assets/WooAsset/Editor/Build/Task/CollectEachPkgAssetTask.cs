using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace WooAsset
{
    public class CollectEachPkgAssetTask : AssetTask
    {
        public enum Errcode
        {
            NothingToBuild
        }

        private List<AssetTask> tasks = new List<AssetTask>()
        {
            new CollectAssetsTask(),
        };


        protected override async void OnExecute(AssetTaskContext context)
        {
            var builds = context.buildPkgs;
            if (builds.Count == 0)
            {
                SetErr(Errcode.NothingToBuild, "Nothing To Build");
                InvokeComplete();
                return;
            }

            List<PackageExportData> exports = new List<PackageExportData>();

            for (int i = 0; i < builds.Count; i++)
            {
                var group = builds[i];
                context.buildPkg = group;
                for (int j = 0; j < tasks.Count; j++)
                    await Execute(tasks[j], context);
                exports.Add(new PackageExportData(context.buildPkg.name)
                {
                    pkg = context.buildPkg.ToPackageData(),
                    manifest = context.manifest,
                    encrypt = context.encrypt.ToString(),
                    version = context.version,
                    compress = context.compress.ToString(),
                    typeTreeOption = context.typeTreeOption,
                });
            }
            context.exports = exports;
            InvokeComplete();
        }
    }
}
