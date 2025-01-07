using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleGuide : GuideBase
{

    private float r;        // 镂空区域圆形的半径

    private float scaleR;   // 变化之后的半径大小 




    public override void Guide(Canvas canvas, RectTransform target, TranslateType translateType = TranslateType.Direct, float moveTime = 1)
    {
        base.Guide(canvas, target,translateType,moveTime);
        float width = (targetCorners[3].x - targetCorners[0].x) / 2;
        float height = (targetCorners[1].y - targetCorners[0].y) / 2;
        // 计算半径 
        r  = Mathf.Sqrt( width * width + height * height);
        // 
        this.material.SetFloat("_Slider", r);
    }

    public override void Guide(Canvas canvas, RectTransform target, float scale, float time, TranslateType translateType = TranslateType.Direct, float moveTime = 1)
    {
        //base.Guide(canvas, target, scale, time);
        this.Guide(canvas, target,translateType,moveTime);

        scaleR = r * scale;
        this.material.SetFloat("_Slider", scaleR);

        this.scaleTime = time;
        isScaling = true;
        scaleTimer = 0;
    }

    protected override void Update()
    {
        base.Update();

        if ( isScaling )
        {
            this.material.SetFloat("_Slider", Mathf.Lerp(scaleR, r, scaleTimer));
        }

    }


}
