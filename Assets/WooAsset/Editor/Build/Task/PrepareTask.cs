namespace WooAsset
{
    public class PrepareTask : AssetTask
    {
        protected override async void OnExecute(AssetTaskContext context)
        {
            string err = context.Params.CheckLeagal();
            if (!string.IsNullOrEmpty(err))
            {
                SetErr(err);
                InvokeComplete();
                return;
            }
            string versionPath = AssetsHelper.CombinePath(context.historyPath, context.VersionCollectionName);
            context.historyVersions = new VersionCollectionData() { };
            context.historyVersionPath = versionPath;
            if (AssetsHelper.ExistsFile(versionPath))
            {
                var reader = AssetsHelper.ReadFile(versionPath, true);
                await reader;
                context.historyVersions = VersionHelper.ReadAssetsVersionCollection(reader.bytes);
            }
            context.BuildOption = context.Params.GetBundleOption();
            context.version = context.assetBuild.GetVersion(context.Params.version, context);


            InvokeComplete();
        }

    }
}
