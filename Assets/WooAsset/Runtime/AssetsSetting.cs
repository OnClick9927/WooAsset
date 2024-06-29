using System;
using static WooAsset.FileData;

namespace WooAsset
{

    public abstract class AssetsSetting
    {
        public virtual bool NeedCopyStreamBundles() { return true; }
        public virtual long GetLoadingMaxTimeSlice() { return long.MaxValue; }
        protected virtual string GetBaseUrl() { return string.Empty; }
        public virtual string GetUrlByBundleName(string buildTarget, string bundleName) => $"{GetBaseUrl()}/{buildTarget}/{bundleName}";
        public virtual string GetUrlByBundleName(string buildTarget, string version, string bundleName) => $"{GetBaseUrl()}/{buildTarget}/{version}/{bundleName}";


        public virtual bool GetSaveBundlesWhenPlaying() => true;
        public virtual bool GetBundleAlwaysFromWebRequest() { return true; }
        public virtual int GetWebRequestTimeout() { return 30; }
        public virtual int GetWebRequestRetryCount() { return 3; }
        public virtual bool GetAutoUnloadBundle() { return true; }

        public virtual IAssetLife GetAssetLife() { return new LRULife(1024 * 1024 * 1024); }

        public virtual IAssetEncrypt GetEncrypt(int code)
        {
            if (code == NoneAssetStreamEncrypt.code) return none;
            if (code == DefaultAssetStreamEncrypt.code) return def;
            return null;
        }
        NoneAssetStreamEncrypt none = new NoneAssetStreamEncrypt();
        DefaultAssetStreamEncrypt def = new DefaultAssetStreamEncrypt();

    }
}
