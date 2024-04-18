/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.2.241
 *UnityVersion:   2019.4.36f1c1
 *Date:           2022-03-14
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WooAsset
{
    public class LocalSetting : AssetsSetting
    {
        public override bool NeedCopyStreamBundles()
        {
            return true;
        }
        public override bool GetBundleAwalysFromWebRequest()
        {
            return false;
        }
        protected override string GetBaseUrl()
        {
            //return "https://pic.trinityleaves.cn/images/xxx";
            return "http://127.0.0.1:8080";
            //Application.dataPath, "../DLCDownLoad"
            //return AssetsInternal.ToRegularPath(AssetsInternal.CombinePath());
        }

        public override IAssetLife GetAssetLife()
        {
            return null;
            return new LRULife(1024 * 50);
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
            var op = await Assets.VersionCheck();
            if (op.Versions != null)
            {
                var version = op.Versions.NewestVersion();
                var compare = await op.Compare(version, version.GetAllPkgs());


                for (int i = 0; i < compare.add.Count; i++)
                    await Assets.DownLoadBundle(compare.add[i].name);
                for (int i = 0; i < compare.change.Count; i++)
                    await Assets.DownLoadBundle(compare.change[i].name);
            }
            await Assets.InitAsync();
            await Assets.UnzipRawFile();
            var oppp = await Assets.InstantiateAsync("Assets/Example/New Folder/Cube.prefab", null);
            var sceneAsset = await Assets.LoadSceneAssetAsync("Assets/Example/Scene/New Scene2.unity");
            await sceneAsset.LoadSceneAsync(LoadSceneMode.Additive);
            var asset = await Assets.LoadAsset("Assets/Example/New Folder/aaa");
            RawObject raw = asset.GetAsset<RawObject>();
            if (!asset.unloaded)
            {
                Debug.Log(raw.bytes.Length);
            }

            var _asset = Assets.LoadAsset("Assets/Example/New Folder/a.jpg");
            image2.sprite = _asset.GetSubAsset<Sprite>("a_1");

            int index = 0;
            for (int i = 0; i < 20; i++)
            {
                index = (int)Mathf.Repeat(index++, 3);
                var asset_1 = Assets.LoadAsset($"Assets/Example/px/{++index}.png");
                image.sprite = asset_1.GetAsset<Sprite>();
                await Task.Delay(100);
            }
            oppp.Destroy();
            var __asset = await Assets.LoadFileAssetAsync(Assets.GetRawFileToDlcPath("Assets/Example/New Folder/kkk.rfc"));
            var fo = __asset.GetAsset<RawObject>();
            Debug.Log(fo.bytes.Length);
        }

    }
}
