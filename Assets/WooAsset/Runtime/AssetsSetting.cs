﻿namespace WooAsset
{

    public abstract class AssetsSetting
    {
        public virtual bool CheckVersionByVersionCollection() => false;
        public virtual bool NeedCopyStreamBundles() => true;
        public virtual string GetStreamingFileUrl(string url) => url;
        public virtual long GetLoadingMaxTimeSlice() => long.MaxValue;
        protected virtual string GetBaseUrl() => string.Empty;
        public virtual string GetUrlByBundleName(string buildTarget, string bundleName) => $"{GetBaseUrl()}/{buildTarget}/{bundleName}";
        public virtual string GetUrlByBundleName(string buildTarget, string version, string bundleName) => $"{GetBaseUrl()}/{buildTarget}/{version}/{bundleName}";


        public virtual bool GetSaveBundlesWhenPlaying() => true;
        public virtual bool GetBundleAlwaysFromWebRequest() => true;
        public virtual int GetWebRequestTimeout() => 30;
        public virtual bool GetFuzzySearch() => false;

        public virtual int GetWebRequestRetryCount() => 3;
        public virtual bool GetAutoUnloadBundle() => true;

        public virtual IAssetLife GetAssetLife() => new LRULife(1024 * 1024 * 1024);

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
