using System.Collections.Generic;
using System.Linq;


namespace WooAsset
{
    [System.Serializable]
    public class VersionData : IBufferObject
    {

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
            return pkgs.FindAll(p => tags.Any(y => p.tags.Contains(y)));
        }

        public List<PackageData> GetAllPkgs()
        {
            return pkgs;
        }
    }
}
