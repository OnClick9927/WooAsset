using System.Collections.Generic;

namespace WooAsset
{

    public abstract class AssetsSetting
    {

        public virtual bool CheckVersionByVersionCollection() => false;
        public virtual bool NeedCopyStreamBundles() => true;
        public virtual string GetStreamingFileUrl(string url)
        {
#if UNITY_STANDALONE_OSX || UNITY_IOS
        return $"file://{url}";
#else
            return url;
#endif
        }
        public virtual long GetLoadingMaxTimeSlice() => long.MaxValue;
        protected virtual string GetBaseUrl() => string.Empty;
        public virtual string GetUrlByBundleName(string buildTarget, string bundleName) => $"{GetBaseUrl()}/{buildTarget}/{bundleName}";
        public virtual string GetUrlByBundleName(string buildTarget, string version, string bundleName) => $"{GetBaseUrl()}/{buildTarget}/{version}/{bundleName}";

        public virtual bool GetCachesDownloadedBundles() => false;
        public virtual bool GetSaveBytesWhenPlaying() => true;
        public virtual bool GetBundleAlwaysFromWebRequest() => true;
        public virtual int GetWebRequestRetryCount() => 3;
        public virtual int GetWebRequestTimeout() => 30;
        public virtual int GetWebRequestCountAtSameTime() => 30;

        public virtual bool GetFuzzySearch() => false;
        public virtual FileNameSearchType GetFileNameSearchType() => FileNameSearchType.FileName;

        public virtual bool GetAutoUnloadBundle() => true;

        public virtual IAssetLife GetAssetLife() => new LRULife(1024 * 1024 * 1024);

        public virtual IAssetEncrypt GetEncrypt(int code)
        {

            IAssetEncrypt en = null;
            map.TryGetValue(code, out en);
            return en;
        }
        private Dictionary<int, IAssetEncrypt> map = new Dictionary<int, IAssetEncrypt>()
        {
            {NoneAssetStreamEncrypt.code,new NoneAssetStreamEncrypt() },
            {DefaultAssetStreamEncrypt.code,new DefaultAssetStreamEncrypt() },
            {OffsetAssetStreamEncrypt.code,new OffsetAssetStreamEncrypt() },

        };




    }
}
