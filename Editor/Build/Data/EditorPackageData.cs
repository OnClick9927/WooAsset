﻿using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    [System.Serializable]
    public class EditorPackageData : PackageData
    {
        //public string name;
        //public string description;
        //public List<string> tags = new List<string>();
        public List<string> paths = new List<string>();
        public List<EditorBundleDataBuild> builds = new List<EditorBundleDataBuild>();
        public bool build;

        public bool HasSamePath() => paths.Distinct().Count() != paths.Count();
        public bool HasSamePath(EditorPackageData other) => paths.Intersect(other.paths).Count() > 0;

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
