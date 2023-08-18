namespace WooAsset
{
    class WriteVersionBufferOperation<T> : WriteObjectOperation<T>
    {
        private CopyFileOperation op;
        private IAssetStreamEncrypt en;
        private bool go;
        public WriteVersionBufferOperation(T version, string path, IAssetStreamEncrypt en, bool go) : base(version, path, true)
        {
            this.en = en;
            this.go = go;
            Done();
        }
        protected override byte[] GetBytes(byte[] bytes)
        {
            return EncryptBuffer.Encode(VersionBuffer.remoteHashName, bytes, en);
        }
        protected override void Done()
        {
            if (!go)
                InvokeComplete();
            else
                base.Done();
        }
    }
}
