

namespace WooAsset
{
    public abstract class AssetsSetting
    {
        public enum FileCheckType
        {
            MD5,
            FileLength
        }
        protected virtual string GetBaseUrl() { return string.Empty; }
        public virtual string GetUrlByBundleName(string buildTarget, string bundleName)
        {
            return AssetsInternal.ToRegularPath(AssetsInternal.CombinePath(GetBaseUrl(), $"{buildTarget}/{bundleName}"));
        }
        public virtual string GetVersionUrl(string buildTarget)
        {
            return AssetsInternal.ToRegularPath(AssetsInternal.CombinePath(GetBaseUrl(),$"{buildTarget}/version"));
        }
        public virtual FileCheckType GetFileCheckType() { return FileCheckType.MD5; }
        public virtual int GetWebRequestTimeout() { return 30; }

        public virtual IAssetStraemEncrypt GetEncrypt() { return new DefaultAssetStraemEncrypt(); }
        public virtual bool GetAutoUnloadBundle() { return true; }

    }
}
