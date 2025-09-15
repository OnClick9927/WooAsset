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
            var op = await Assets.LoadRemoteVersions();
            if (op.Versions != null)
            {
                var version = op.Versions.NewestVersion();
                var down = await Assets.DownloadVersionData(version);
                var versionData = down.GetVersion();
                var compare = await Assets.CompareVersion(versionData, versionData.GetAllPkgs(), VersionCompareType.FileHash);
                List<DownLoader> downloader = new List<DownLoader>();
                for (int i = 0; i < compare.add.Count; i++)
                    downloader.Add(Assets.DownLoadBundleFile(versionData.version, compare.add[i].bundleName));
                for (int i = 0; i < compare.change.Count; i++)
                    downloader.Add(Assets.DownLoadBundleFile(versionData.version, compare.change[i].bundleName));
                GroupOperation<DownLoader> op2 = new GroupOperation<DownLoader>();
                op2.Done(downloader);
                await op2;
            }


            await Assets.InitAsync("", true);


            var sceneAsset = await Assets.LoadSceneAssetAsync("Assets/Example/pkg2/Scene/New Scene2.unity");

            await sceneAsset.LoadSceneAsync(LoadSceneMode.Additive);
            await Task.Delay(1000);
            await Assets.UnloadSceneAsync("Assets/Example/pkg2/Scene/New Scene2.unity", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            await Task.Delay(1000);


            sceneAsset = await Assets.LoadSceneAssetAsync("Assets/Example/pkg2/Scene/New Scene2.unity");

            await sceneAsset.LoadSceneAsync(LoadSceneMode.Additive);
            await Task.Delay(1000);
            await Assets.UnloadSceneAsync("Assets/Example/pkg2/Scene/New Scene2.unity", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);


            for (int i = 0; i < 10; i++)
            {
                string path = "Assets/Example/pkg2/px/avatar.jpg";
                string path1 = "Assets/Example/pkg2/px/b_da5501a6ee682f0b0cbba61a7ad46106.jpg";
                string path2 = "Assets/Example/pkg2/px/OIP.jpg";

                path = i % 3 == 0 ? path : i % 3 == 1 ? path1 : path2;
                var asset = await Assets.LoadAssetAsync(path);
                this.image.sprite = asset.GetAsset<Sprite>();
                await Task.Delay(500);
            }
            string _path = "Assets/Example/pkg2/px/test.jpg";
            for (int i = 0; i < 10; i++)
            {
                var index = Mathf.Repeat(i,4);

                var asset = await Assets.LoadSubAsset(_path);
                this.image.sprite = asset.GetSubAsset<Sprite>($"test_{index}");
                await Task.Delay(500);
            }
        }

    }
}
