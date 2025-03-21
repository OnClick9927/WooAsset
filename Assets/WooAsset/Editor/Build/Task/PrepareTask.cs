namespace WooAsset
{
    public class PrepareTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            string err = context.Params.CheckLegal();
            if (!string.IsNullOrEmpty(err))
            {
                SetErr(err);
                InvokeComplete();
                return;
            }
            string versionPath = AssetsEditorTool.CombinePath(context.historyPath, context.VersionCollectionName);
            context.historyVersions = new VersionCollectionData() { };
            context.historyVersionPath = versionPath;
            if (AssetsEditorTool.ExistsFile(versionPath))
            {
                //var bytes = AssetsEditorTool.ReadFileSync(versionPath);
                context.historyVersions = AssetsEditorTool.ReadJson<VersionCollectionData>(versionPath);
            }
            context.BuildOption = context.Params.GetBundleOption(out err);
            if (!string.IsNullOrEmpty(err))
            {
                SetErr(err);
                InvokeComplete();
                return;
            }
            context.version = context.assetBuild.GetVersion(context.Params.version, context);


            InvokeComplete();
        }

    }
}
