using UnityEngine;

namespace WooAsset
{
    public class RawObject : ScriptableObject
    {
        public string rawPath;
        public byte[] bytes;


        public static RawObject Create(string path)
        {
            RawObject obj = ScriptableObject.CreateInstance<RawObject>();
            obj.rawPath = path;
            return obj;
        }
    }

}
