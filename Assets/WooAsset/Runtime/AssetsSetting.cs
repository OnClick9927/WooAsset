using static WooAsset.FileData;

namespace WooAsset
{

    public abstract class AssetsSetting
    {
        public virtual string OverwriteBundlePath(string bundlePath) { return bundlePath; }
        public virtual bool NeedCopyStreamBundles() { return true; }
        public virtual long GetLoadingMaxTimeSlice() { return long.MaxValue; }
        protected virtual string GetBaseUrl() { return string.Empty; }
        public virtual string GetUrlByBundleName(string buildTarget, string bundleName)
        {
            return AssetsHelper.ToRegularPath(AssetsHelper.CombinePath(GetBaseUrl(), $"{buildTarget}/{bundleName}"));
        }
        public virtual bool GetSaveBundlesWhenPlaying()
        {
            return true;
        }
        public virtual bool GetBundleAwalysFromWebRequest() {  return true; }
        public virtual FileCompareType GetFileCheckType() { return FileCompareType.Hash; }
        public virtual int GetWebRequestTimeout() { return 30; }
        public virtual int GetWebRequestRetryCount() { return 3; }
        public virtual IAssetStreamEncrypt GetEncrypt() { return new DefaultAssetStreamEncrypt(); }
        public virtual bool GetAutoUnloadBundle() { return true; }

        public virtual IAssetLife GetAssetLife() { return new LRULife(1024 * 1024 * 1024); }


    }
}
