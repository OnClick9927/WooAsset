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
        }

    }
}
