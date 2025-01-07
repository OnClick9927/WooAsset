using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WooAsset;
public class MyAssetsSetting : AssetsSetting
{
    protected override string GetBaseUrl()
    {
        return Application.streamingAssetsPath;
    }
    public override string GetUrlByBundleName(string buildTarget, string bundleName)
    {
        return $"{base.GetUrlByBundleName(buildTarget, bundleName)}{StreamBundlesData.fileExt}";
    }
    public override string GetUrlByBundleName(string buildTarget, string version, string bundleName)
    {
        return this.GetUrlByBundleName(buildTarget, bundleName);
    }
    public override bool GetAutoUnloadBundle()
    {
        return false;
    }
    public override bool GetSaveBundlesWhenPlaying()
    {
        return false;
    }
}
public class A : MonoBehaviour
{
    private async void Awake()
    {

        Assets.SetAssetsSetting(new MyAssetsSetting());
        await Assets.InitAsync();
        var assets = Assets.GetAllAssetPaths().ToArray();
        await Assets.PrepareAssets(assets);
        var asset = await Assets.LoadSceneAsset("Assets/Project/Scenes/01-menu.unity");
        var s = Assets.LoadAsset("Assets/Project/shadervariants.shadervariants").GetAsset<ShaderVariantCollection>();
        s.WarmUp();
        asset.LoadScene(UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
