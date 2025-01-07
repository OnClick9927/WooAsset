using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WooAsset;

public class LosePanel : View
{
    // 重新开始的按钮点击事件
    public void OnRestartClick()
    {
        GameObject.Find("Canvas/GamePanel").GetComponent<GamePanel>().RestartGame();
        // 隐藏当前界面
        Hide();
    }

    // 退出按钮的点击事件
    public async void OnExitClick()
    {
       GamePanel.ExitGame();

    }
}
