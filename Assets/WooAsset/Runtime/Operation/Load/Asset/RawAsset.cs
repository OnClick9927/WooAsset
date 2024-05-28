namespace WooAsset
{
    public class RawAsset : AssetHandle<RawObject>
    {
        public RawAsset(AssetLoadArgs loadArgs) : base(loadArgs)
        {
        }
        public RawObject GetAsset() => isDone ? value : null;

        public override float progress
        {
            get
            {
                if (isDone) return 1;
                return bundle.progress;
            }
        }
        protected override void InternalLoad()
        {
            if (bundle.isErr)
            {
                this.SetErr(bundle.error);
                InvokeComplete();
                return;
            }

            var raw = bundle.LoadRawObject(path);
            SetResult(raw);
        }
    }

}
