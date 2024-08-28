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
    public class AssetExample : UnityEngine.MonoBehaviour
    {
        public Image image;
        public Image image2;

        private async void Start()
        {
            Assets.SetAssetsSetting(new LocalSetting());
            //await Assets.CopyToSandBox();
            //var op = await Assets.LoadRemoteVersions();
            //if (op.Versions != null)
            //{
            //    var version = op.Versions.NewestVersion();
            //    var down = await Assets.DownloadVersionData(version);
            //    var versionData = down.GetVersion();
            //    var compare = await Assets.CompareVersion(versionData, versionData.GetAllPkgs());

            //    for (int i = 0; i < compare.add.Count; i++)
            //        await Assets.DownLoadBundle(versionData.version, compare.add[i].bundleName);
            //    for (int i = 0; i < compare.change.Count; i++)
            //        await Assets.DownLoadBundle(versionData.version, compare.change[i].bundleName);
            //}


            await Assets.InitAsync();

            var sceneAsset = await Assets.LoadSceneAssetAsync("Assets/Example/Scene/New Scene2.unity");
            var oppp = await Assets.InstantiateAsync("Assets/Example/New Folder/Cube.prefab", null);
            oppp.Destroy();

            await sceneAsset.LoadSceneAsync(LoadSceneMode.Additive);


            var asset = await Assets.LoadRawAssetAsync("Assets/Example/New Folder/aaa");

            RawObject raw = asset.GetAsset();
            Debug.Log(raw.bytes.Length);

            var _asset = await Assets.LoadSubAsset("Assets/Example/New Folder/a");
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
