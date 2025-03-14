/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.2.241
 *UnityVersion:   2019.4.36f1c1
 *Date:           2022-03-14
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WooAsset
{
    public class AssetExample : UnityEngine.MonoBehaviour
    {
        public Image image;
        public Image image2;
        public AssetReference<UnityEngine.Sprite> assetReference;
        public Slider slider;
        private void Awake()
        {
            Assets.SetAssetsSetting(new LocalSetting());

        }
        private async void Start()
        {
            //await Task.Delay(2000);
            //Debug.LogError("-----------------");

            //{

            //    var opp = Assets.CopyToSandBox();
            //    while (!opp.isDone)
            //    {

            //        await Task.Delay(1);
            //        Debug.LogError(opp.progress);
            //        slider.value = opp.progress;
            //    }
            //    Debug.LogError(opp.progress);

            //    //await opp;

            //}
            //Debug.LogError("-----------------");
            ////return;
            //var op = await Assets.LoadRemoteVersions();
            //if (op.Versions != null)
            //{
            //    var version = op.Versions.NewestVersion();
            //    var down = await Assets.DownloadVersionData(version);
            //    var versionData = down.GetVersion();
            //    var compare = await Assets.CompareVersion(versionData, versionData.GetAllPkgs(), VersionCompareType.FileHash);
            //    List<DownLoader> downloader = new List<DownLoader>();



            //    for (int i = 0; i < compare.add.Count; i++)
            //        downloader.Add(Assets.DownLoadBundleFile(versionData.version, compare.add[i].bundleName));
            //    for (int i = 0; i < compare.change.Count; i++)
            //        downloader.Add(Assets.DownLoadBundleFile(versionData.version, compare.change[i].bundleName));
            //    GroupOperation<DownLoader> op2 = new GroupOperation<DownLoader>();
            //    op2.Done(downloader);
            //    await op2;
            //}
            //Debug.LogError("-----------------");


            await Assets.InitAsync("", true);
            //var asset_svc = await Assets.LoadAsset("Assets/Example/GameObject.prefab");
            //asset_svc.GetAsset<ShaderVariantCollection>().WarmUp();
            var _test = await assetReference.LoadAssetAsync();
            await _test;
            image.sprite = _test.GetAsset<UnityEngine.Sprite>();
            //return;

            Assets.PrepareAssetsByTag("test");
            var sceneAsset = await Assets.LoadSceneAssetAsync("Assets/Example/Scene/PostProcessing.unity");
            //await Assets.LoadSceneAssetAsync("Assets/Example/Scene/New Scene2.unity");
            //var oppp = await Assets.InstantiateAsync("Assets/Example/New Folder/Cube.prefab", null);
            await sceneAsset.LoadSceneAsync(LoadSceneMode.Single);


            //var asset = await Assets.LoadRawAssetAsync("Assets/Example/New Folder/aaa");

            //RawObject raw = asset.GetAsset();
            //Debug.Log(raw.bytes.Length);


            //var _asset = await Assets.LoadSubAsset("Assets/Example/New Folder/a.jpg");
            //image2.sprite = _asset.GetSubAsset<Sprite>("a_1");

            //int index = 0;
            //for (int i = 0; i < 20; i++)
            //{
            //    index = (int)Mathf.Repeat(index++, 3);
            //    var asset_1 = Assets.LoadAsset($"Assets/Example/px/{++index}.png");
            //    image.sprite = asset_1.GetAsset<Sprite>();
            //    await Task.Delay(100);
            //}
            //await Assets.UnloadSceneAsync("Assets/Example/Scene/New Scene2.unity", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            //oppp.Destroy();
        }

    }
}
