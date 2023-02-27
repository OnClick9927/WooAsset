
using System.Collections.Generic;

namespace WooAsset
{
    [System.Serializable]
    public class AssetsVersion
    {
        public const string versionName = "#########version";
        [System.Serializable]
        public class VersionData
        {
            public string bundleName;
            public long length;
            public string md5;
        }
        public string version;
        public List<VersionData> datas = new List<VersionData>();
    }
}
