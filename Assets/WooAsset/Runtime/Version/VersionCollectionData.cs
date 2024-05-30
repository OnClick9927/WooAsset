using System.Collections.Generic;

namespace WooAsset
{
    [System.Serializable]
    public class VersionCollectionData : IBufferObject
    {


        [UnityEngine.SerializeField] private List<string> versions = new List<string>();

        void IBufferObject.ReadData(BufferReader reader)
        {
            versions = reader.ReadUTF8List();
        }
        void IBufferObject.WriteData(BufferWriter writer)
        {
            writer.WriteUTF8List(versions);
        }
        public string FindVersion(string version) => versions.Find(x => x == version);
        public string NewestVersion() => versions[versions.Count - 1];
        public List<string> GetVersions() => versions;
        public void AddVersion(string version)
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
