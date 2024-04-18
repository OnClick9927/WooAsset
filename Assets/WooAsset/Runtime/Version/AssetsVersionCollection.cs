
using System;
using System.Collections.Generic;
using System.Linq;
using static WooAsset.AssetsVersionCollection.VersionData;

namespace WooAsset
{
    [System.Serializable]
    public class AssetsVersionCollection
    {

        [System.Serializable]
        public class VersionData
        {

            [System.Serializable]
            public class PackageData
            {
                public string name;
                public string description;
                public string manifestFileName;
                public string bundleFileName;

                public List<string> tags = new List<string>();
            }

            public string version;
            [UnityEngine.SerializeField] private List<PackageData> pkgs = new List<PackageData>();
            public void SetPkgs(List<PackageData> pkgs)
            {
#if UNITY_EDITOR
                this.pkgs = pkgs;
#endif
            }
            public PackageData FindPkg(string pkgName)
            {
                return pkgs.Find(x => x.name == pkgName);
            }
            public List<PackageData> FindPkgs(string[] tags)
            {
                return pkgs.FindAll(p => tags.Intersect(p.tags).Count() > 0);
            }
            public List<PackageData> GetAllPkgs()
            {
                return pkgs;
            }
        }

        [UnityEngine.SerializeField] private List<VersionData> versions = new List<VersionData>();
        public VersionData FindVersion(string version) => versions.Find(x => x.version == version);
        public VersionData NewestVersion() => versions.Last();
        public List<VersionData> GetVersions() => versions;
        public void AddVersion(VersionData version) {
#if UNITY_EDITOR
            versions.Add(version); 
#endif
        }


        public void RemoveFirstIFTooLarge(int large)
        {
#if UNITY_EDITOR
            while (versions.Count > large)
                versions.RemoveAt(0);
#endif
        }
    }
}
