/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.2.241
 *UnityVersion:   2019.4.36f1c1
 *Date:           2022-03-14
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using UnityEngine;

namespace WooAsset
{
    public class LocalSetting : AssetsSetting
    {
        //public override string GetUrlByBundleName(string buildTarget, string bundleName)
        //{
        //    return base.GetUrlByBundleName(buildTarget, bundleName) + StreamBundlesData.fileExt;
        //}
        //public override string GetUrlByBundleName(string buildTarget, string version, string bundleName)
        //{
        //    return GetUrlByBundleName(buildTarget, bundleName);
        //}
        public override bool GetFuzzySearch()
        {
            return false;
        }
        public override bool NeedCopyStreamBundles()
        {
            return true;
        }
        public override bool GetAutoUnloadBundle()
        {
            return true;
        }
        public override bool GetSaveBytesWhenPlaying()
        {
            return false;
        }
        public override bool GetBundleAlwaysFromWebRequest()
        {
            return true;
        }
        public override bool GetCachesDownloadedBundles()
        {
            return false;
        }
        protected override string GetBaseUrl()
        {
            return "http://127.0.0.1:8080";
            return Application.streamingAssetsPath;
            //return "https://pic.trinityleaves.cn/images/xxx";
            //Application.dataPath, "../DLCDownLoad"
            //return AssetsInternal.ToRegularPath(AssetsInternal.CombinePath());
        }
   
        public override IAssetLife GetAssetLife()
        {
            return null;
        }
    }
}
