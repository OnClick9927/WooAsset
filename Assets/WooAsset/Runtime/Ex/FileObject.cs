using System.IO;
using UnityEngine;
namespace WooAsset
{
    public class FileObject : ScriptableObject
    {
        public string path;
        public byte[] bytes;
        public FileInfo fileInfo;
    }

}
