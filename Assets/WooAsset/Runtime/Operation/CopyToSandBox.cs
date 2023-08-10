namespace WooAsset
{
    class CopyToSandBox : CopyDirectoryOperation
    {
        public CopyToSandBox(string srcPath, string destPath, bool cover) : base(srcPath, destPath, cover)
        {
        }
        protected override string GetDestFileName(string src)
        {
            return base.GetDestFileName(src).Replace(".bytes", "");
        }
        protected override bool NeedCopy(string src)
        {
            return !src.EndsWith(".meta");
        }
    }

}
