using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WooAsset;

public class SelectModelPanel : View
{

    // 点击模式选择按钮
    public async void OnSelectModelClick( int count )
    {
        // 选择模式
        PlayerPrefs.SetInt(Const.GameModel, count);
        // 跳转场景 到 游戏场景
        var asset = await Assets.LoadSceneAssetAsync("Assets/Project/Scenes/02-game.unity");
        asset.LoadScene(LoadSceneMode.Single);

    }


}
