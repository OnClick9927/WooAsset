/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.2.241
 *UnityVersion:   2019.4.36f1c1
 *Date:           2022-03-14
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WooAsset
{
    public class LocalSetting : AssetsSetting
    {
        //public override string GetUrlByBundleName(string buildTarget, string bundleName)
        //{
        //    return base.GetUrlByBundleName(buildTarget, bundleName)+".bytes";
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
        public override bool GetBundleAwalysFromWebRequest()
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
    public class AssetExample : UnityEngine.MonoBehaviour
    {
        public Image image;
        public Image image2;

        private async void Start()
        {
            Assets.SetAssetsSetting(new LocalSetting());
            await Assets.CopyToSandBox();
            var op = await Assets.LoadRemoteVersions();
            if (op.Versions != null)
            {
                var version = op.Versions.NewestVersion();
                var down = await Assets.DonloadVersionData(version);
                var versionData = down.GetVersion();
                var compare = await Assets.CompareVersion(versionData, versionData.GetAllPkgs());

                for (int i = 0; i < compare.add.Count; i++)
                    await Assets.DownLoadBundle(versionData.version, compare.add[i].name);
                for (int i = 0; i < compare.change.Count; i++)
                    await Assets.DownLoadBundle(versionData.version, compare.change[i].name);
            }



            await Assets.InitAsync();

            var sceneAsset = await Assets.LoadSceneAssetAsync("Assets/Example/Scene/New Scene2.unity");
            var oppp = await Assets.InstantiateAsync("Assets/Example/New Folder/Cube.prefab", null);
            oppp.Destroy();

            await sceneAsset.LoadSceneAsync(LoadSceneMode.Additive);


            var asset = await Assets.LoadRawAssetAsync("Assets/Example/New Folder/aaa");

            RawObject raw = asset.GetAsset();
            Debug.Log(raw.bytes.Length);


            var _asset = await Assets.LoadAsset("Assets/Example/New Folder/a.jpg");
            image2.sprite = _asset.GetSubAsset<Sprite>("a_1");

            int index = 0;
            for (int i = 0; i < 20; i++)
            {
                index = (int)Mathf.Repeat(index++, 3);
                var asset_1 = Assets.LoadAsset($"Assets/Example/px/{++index}.png");
                image.sprite = asset_1.GetAsset<Sprite>();
                await Task.Delay(100);
            }
            await Assets.UnloadSceneAsync("Assets/Example/Scene/New Scene2.unity", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        }

    }
}
