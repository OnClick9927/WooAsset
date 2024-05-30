using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    [System.Serializable]
    public class EditorBundlePackage
    {
        public bool build;
        public string name;
        public string description;
        public List<string> tags = new List<string>();
        public List<string> paths = new List<string>();

        public bool HasSamePath() => paths.Distinct().Count() != paths.Count();
        public bool HasSamePath(EditorBundlePackage other) => paths.Intersect(other.paths).Count() > 0;

        public PackageData ToPackageData()
        {
            return new PackageData()
            {
                description = description,
                name = name,
                tags = tags
            };
        }
    }
}
