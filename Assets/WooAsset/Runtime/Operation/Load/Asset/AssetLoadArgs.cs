using System;
using System.Collections.Generic;
namespace WooAsset
{
    public struct AssetLoadArgs : IAssetArgs
    {
        public string bundleName;
        public string path;
        public bool direct;
        public bool scene;
        public List<AssetHandle> dps;
        public bool async;
        public Type type;

        public string uid => path;

        public AssetLoadArgs(string path, bool direct, bool scene, List<AssetHandle> dps, string bundleName, bool async, Type type)
        {
            this.dps = dps;
            this.path = path;
            this.direct = direct;
            this.scene = scene;
            this.bundleName = bundleName;
            this.async = async;
            this.type = type;
        }
    }

}
