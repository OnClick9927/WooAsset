/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.2.241
 *UnityVersion:   2019.4.36f1c1
 *Date:           2022-03-14
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace WooAsset
{
    public class LocalSetting : AssetsSetting
    {
        protected override string GetBaseUrl()
        {
            return AssetsInternal.ToRegularPath(AssetsInternal.CombinePath(Application.dataPath, "../DLCDownLoad"));
        }
    }
    public class AssetExample : UnityEngine.MonoBehaviour
    {
        public Image image;
        public Image image2;
        private async void Start()
        {
            Assets.SetAssetsSetting(new LocalSetting());
            var op = await Assets.VersionCheck();
            for (int i = 0; i < op.downLoadOnes.Count; i++)
            {
                await Assets.DownLoadBundle(op.downLoadOnes[i].bundleName);
            }
            //await Assets.CopyDLCFromSteam();
            await Assets.InitAsync();
            await Assets.InstantiateAsync("Assets/Woo/__Example/New Folder/Cube.prefab", null);
            var sceneAsset = await Assets.LoadSceneAssetAsync("Assets/Woo/__Example/New Scene2.unity");
            await sceneAsset.LoadSceneAsync(LoadSceneMode.Additive);
        }
        int index = 0;
        private async void Update()
        {
            if (!Assets.Initialized()) return;
            index = (int)Mathf.Repeat(++index, 3);

            string path = $"Assets/Woo/__Example/px/{index + 1}.png";
            var asset = await Assets.LoadAssetAsync(path);
            var prefab = asset.GetAsset<Sprite>();
            image.sprite = prefab;
            index = (int)Mathf.Repeat(++index, 3);
            path = $"Assets/Woo/__Example/px/{index + 1}.png";
            asset = await Assets.LoadAssetAsync(path);
            prefab = asset.GetAsset<Sprite>();
            image2.sprite = prefab;
        }
    }
}
