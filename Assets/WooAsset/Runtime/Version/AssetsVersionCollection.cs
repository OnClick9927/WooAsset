using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    [System.Serializable]
    public class AssetsVersionCollection : IBufferObject
    {

        [System.Serializable]
        public class VersionData : IBufferObject
        {

            [System.Serializable]
            public class PackageData : IBufferObject
            {
                public string name;
                public string description;
                public string manifestFileName => VersionHelper.GetManifestFileName(name);
                public string bundleFileName => VersionHelper.GetBundleFileName(name);

                public List<string> tags = new List<string>();

                void IBufferObject.ReadData(BufferReader reader)
                {
                    name = reader.ReadUTF8();
                    description = reader.ReadUTF8();
                    tags = reader.ReadUTF8List();
                }

                void IBufferObject.WriteData(BufferWriter writer)
                {
                    writer.WriteUTF8(name);
                    writer.WriteUTF8(description);
                    writer.WriteUTF8List(tags);
                }
            }

            public string version;
            [UnityEngine.SerializeField] private List<PackageData> pkgs = new List<PackageData>();


            void IBufferObject.ReadData(BufferReader reader)
            {
                version = reader.ReadUTF8();
                pkgs = reader.ReadObjectList<PackageData>();

            }

            void IBufferObject.WriteData(BufferWriter writer)
            {
                writer.WriteUTF8(version);
                writer.WriteObjectList<PackageData>(pkgs);
            }
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


        void IBufferObject.ReadData(BufferReader reader)
        {
            versions = reader.ReadObjectList<VersionData>();
        }

        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteObjectList(versions);
        }
        public VersionData FindVersion(string version) => versions.Find(x => x.version == version);
        public VersionData NewestVersion() => versions.Last();
        public List<VersionData> GetVersions() => versions;
        public void AddVersion(VersionData version)
        {
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
