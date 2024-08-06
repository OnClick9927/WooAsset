using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPanel : View
{

    public Slider slider_sound;
    public Slider slider_music;

    // 关闭按钮
    public void OnBtnCloseClick() {
        // 当前这个界面隐藏
        Hide();
    }
    // 音效
    public void OnSoundValueChange(float f)
    {
        // 修改音效的大小  
        AudioManager._instance.OnSoundVolumeChange(f);
        // 保存当前的修改
        PlayerPrefs.SetFloat(Const.Sound, f);

    }

    // 音乐
    public void OnMusicValueChange(float f) {
        // 修改音乐的大小 
        AudioManager._instance.OnMusicVolumeChange(f);
        // 保存当前的修改
        PlayerPrefs.SetFloat(Const.Music, f);
    }


    public override void Show()
    {
        base.Show();
        // 对界面进行初始化 
        slider_sound.value = PlayerPrefs.GetFloat(Const.Sound,0);
        slider_music.value = PlayerPrefs.GetFloat(Const.Music, 0);
    }


}
