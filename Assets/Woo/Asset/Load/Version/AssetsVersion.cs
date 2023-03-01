
using System.Collections.Generic;
using System.Text;

namespace WooAsset
{
    [System.Serializable]
    public class AssetsVersion
    {
        public const string versionName = "#########version";
        public static Encoding encoding = Encoding.Default;
        [System.Serializable]
        public class VersionData
        {
            public string bundleName;
            public long length;
            public string md5;
        }
        public string version;
        public List<VersionData> data_list = new List<VersionData>();
    }
}
