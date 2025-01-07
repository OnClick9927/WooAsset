using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{

    public SelectModelPanel selectModelPanel;
    public SetPanel setPanel;

    public NoviceGuideTipPanel tipPanel;

    public AudioClip bgClip;

    public static bool isGuide = false;

    private void Start()
    {
        AudioManager._instance.PlayMusic(bgClip);
        NoviceGuidePanel._instance.Hide();
        MenuPanel.isGuide = false;
        // 判断是不是新手
        //if (PlayerPrefs.GetInt(Const.Novice, 0) == 0)
        //{
        //    tipPanel.Show();
        //}
        //OnStartGameClick();
    }

    // 点击开始游戏
    public void OnStartGameClick()
    {
        // 显示选择模式的界面 
        selectModelPanel.Show();
        if (isGuide)
        {

            NoviceGuidePanel._instance.NextStep(MenuGuideConst.ClickBtnStart);
        }

    }

    // 点击设置
    public void OnSetClick()
    {
        // 显示设置的界面
        setPanel.Show();
    }

    // 点击退出游戏
    public void OnExitClick()
    {
        // 退出游戏
        Application.Quit();
    }


}
