using static WooAsset.ManifestData;

namespace WooAsset
{
    public struct BundleLoadArgs : IAssetArgs
    {
        public bool async;
        public IAssetStreamEncrypt encrypt;
        public BundleData data;
        public Bundle[] dependence;
        public string bundleName => data.bundleName;
        public string uid => data.bundleName;

        public BundleLoadArgs(BundleData data,bool async, IAssetStreamEncrypt en, Bundle[] dependence)
        {
            this.data = data;
            this.async = async;
            this.encrypt = en;
            this.dependence = dependence;
        }

    }

}
