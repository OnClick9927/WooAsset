/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.2.241
 *UnityVersion:   2019.4.36f1c1
 *Date:           2022-03-14
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
namespace WooAsset
{
    public class LocalSetting : AssetsSetting
    {
        //public override string GetUrlByBundleName(string buildTarget, string bundleName)
        //{
        //    return base.GetUrlByBundleName(buildTarget, bundleName) + ".bytes";
        //}
        //public override string GetUrlByBundleName(string buildTarget, string version, string bundleName)
        //{
        //    return GetUrlByBundleName(buildTarget, bundleName);
        //}
        public override bool NeedCopyStreamBundles()
        {
            return false;
        }
        public override bool GetAutoUnloadBundle()
        {
            return true;
        }
        public override bool GetBundleAlwaysFromWebRequest()
        {
            return false;
        }
        protected override string GetBaseUrl()
        {
            //return Application.streamingAssetsPath;
            //return "https://pic.trinityleaves.cn/images/xxx";
            return "http://127.0.0.1:8080";
            //Application.dataPath, "../DLCDownLoad"
            //return AssetsInternal.ToRegularPath(AssetsInternal.CombinePath());
        }

        public override IAssetLife GetAssetLife()
        {
            return null;
        }
    }
}
