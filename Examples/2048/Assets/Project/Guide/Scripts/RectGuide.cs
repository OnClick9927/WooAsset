using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectGuide : GuideBase
{
 
    protected float width;       // 镂空区域的宽
    protected float height;      // 镂空区域的高

    private float scaleWidth;    // 动态变化的宽
    private float scaleHeight;   // 动态变化的高

    // 引导
    public override void Guide( Canvas canvas , RectTransform target, TranslateType translateType = TranslateType.Direct, float time = 1)
    {
        base.Guide( canvas,target,translateType,time);

        // 计算宽 和 高 
        width = (targetCorners[3].x - targetCorners[0].x)/2;
        height = (targetCorners[1].y - targetCorners[0].y)/2;

        material.SetFloat("_SliderX", width);
        material.SetFloat("_SliderY", height);

    }

    public override void Guide(Canvas canvas, RectTransform target, float scale, float time, TranslateType translateType = TranslateType.Direct, float moveTime = 1)
    {
        this.Guide(canvas, target,translateType,moveTime);

        scaleWidth = width * scale;
        scaleHeight = height * scale;

        isScaling = true;
        scaleTimer = 0;
        this.scaleTime = time;
    }


    protected override void Update()
    {
        base.Update();
        if (isScaling)
        {
            material.SetFloat("_SliderX", Mathf.Lerp(scaleWidth, width, scaleTimer));
            material.SetFloat("_SliderY", Mathf.Lerp(scaleHeight, height, scaleTimer));
        }
    }

}
