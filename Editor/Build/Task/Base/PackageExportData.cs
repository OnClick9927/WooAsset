using System.Collections.Generic;

namespace WooAsset
{
    [System.Serializable]
    public class PackageExportData
    {
        public PackageExportData(string pkgName)
        {
            this.pkgName = pkgName;
        }
        public string pkgName;
        public string encrypt;
        public string version;
        public string compress;
        public TypeTreeOption typeTreeOption;
        public PackageData pkg;
        public ManifestData manifest;
        public List<EditorBundleData> bundles = new List<EditorBundleData>();
        public EditorAssetCollection tree = new EditorAssetCollection();
    }
}
