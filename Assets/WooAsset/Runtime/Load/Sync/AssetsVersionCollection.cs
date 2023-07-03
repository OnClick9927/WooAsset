
using System.Collections.Generic;

namespace WooAsset
{
    [System.Serializable]
    public class AssetsVersionCollection
    {

        [System.Serializable]
        public class VersionData
        {
            public string version;
            [System.Serializable]
            public class Group
            {
                public string name;
                public string manifestFileName;
                public string bundleFileName;

                public string description;
                public List<string> tags = new List<string>();
            }
            public List<Group> groups = new List<Group>();
        }
        public List<VersionData> versions = new List<VersionData>();
    }
}
