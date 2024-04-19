using System.Collections.Generic;
using static WooAsset.AssetsBuildOption;

namespace WooAsset
{
    [System.Serializable]
    public class BuildBundleExprotData
    {
        public List<EditorBundlePackage> buildPkgs;
        public string version;
        public string encrypt;
        public string compress;
        public bool forceRebuild;
        public TypeTreeOption typeTreeOption;
        public FileChange fileChange;
        public AssetsVersionCollection versions;
    }
}
