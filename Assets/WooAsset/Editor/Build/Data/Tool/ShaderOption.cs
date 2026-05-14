using System.Collections.Generic;

namespace WooAsset
{
    [System.Serializable]
    public class ShaderOption : AssetsScriptableObject
    {
        public List<string> InputDirectory;
        public string OutputDirectory;
    }
}
