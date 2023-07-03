using System.IO;

namespace WooAsset
{
    public partial class NormalAssetMode
    {
        class CopyToSandBox : CopyBundleOperation
        {
            public CopyToSandBox(string srcPath, string destPath, bool cover) : base(srcPath, destPath, cover)
            {
            }
            protected override string GetDestFileName(FileInfo src)
            {
                return src.Name.Replace(".bytes", "");
            }
            protected override bool NeedCopy(FileInfo src)
            {
                return !src.Name.EndsWith(".meta");
            }
        }

    }

}
