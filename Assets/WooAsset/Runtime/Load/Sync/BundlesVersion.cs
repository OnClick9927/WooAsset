
using System.Collections.Generic;

namespace WooAsset
{
    [System.Serializable]
    public class BundlesVersion
    {

        public string version;
        public List<FileData> bundles = new List<FileData>();
    }

}
