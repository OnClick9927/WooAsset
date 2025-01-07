using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step : View
{

    public string EventName;
    public RectTransform target;
    public GuideType guideType = GuideType.Rect;
    public float scale = 1;
    public float scaleTime = 1;

    public TranslateType translateType = TranslateType.Direct;
    public float transTime = 1;

    public RectTransform targetPos;

    public float delayTime=0;

    private GuideController guideController;

    

    public void Excute(GuideController guideController,Canvas canvas) {

        this.guideController = guideController;

        // 显示当前步
        Show();

        // 进行引导
        guideController.Guide(canvas, target, guideType,scale,scaleTime,translateType, transTime);

        

    }

    private void Update()
    {
        if (targetPos != null)
        {
            // 赋值位置
            targetPos.localPosition = guideController.Center;
        }
    }

}
