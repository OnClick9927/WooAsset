namespace WooAsset
{
    class WriteVersionBufferOperation<T> : WriteObjectOperation<T>
    {
        private IAssetStreamEncrypt en;
        private bool go;
        public WriteVersionBufferOperation(T t, string path, IAssetStreamEncrypt en, bool go) : base(t, path, true)
        {
            this.en = en;
            this.go = go;
        }
        protected override byte[] GetBytes(byte[] bytes)
        {
            return EncryptBuffer.Encode(VersionBuffer.remoteHashName, bytes, en);
        }
        protected override async void Done()
        {
            await new YieldOperation();
            if (!go)
                InvokeComplete();
            else
                base.Done();
        }
    }
}
