using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GuideType {
    Rect,
    Circle
}


[RequireComponent(typeof(CircleGuide))]
[RequireComponent(typeof(RectGuide))]
public class GuideController : MonoBehaviour , ICanvasRaycastFilter
{

    private CircleGuide circleGuide;
    private RectGuide rectGuide;

    public Material rectMat;
    public Material circleMat;

    private Image mask;

    private RectTransform target;

    private GuideType guideType;

    #region 属性
    public Vector3 Center {
        get {

            switch (this.guideType)
            {
                case GuideType.Rect:
                    return rectGuide.Center;
                case GuideType.Circle:
                    return circleGuide.Center;
            }

            return rectGuide.Center;
        }
    }
    #endregion

    private void Awake()
    {
        mask = transform.GetComponent<Image>();

        if ( mask == null ) { throw new System.Exception("mask 初始化失败!"); }

        if (rectMat == null || circleMat == null) { throw new System.Exception("材质未赋值!"); }

        circleGuide = transform.GetComponent<CircleGuide>();
        rectGuide = transform.GetComponent<RectGuide>();

    }

    private void Guide(RectTransform target, GuideType guideType) {
        this.target = target;
        this.guideType = guideType;

        switch (guideType)
        {
            case GuideType.Rect:
                mask.material = rectMat;
                break;
            case GuideType.Circle:
                mask.material = circleMat;
                break;
        }
    }

    public void Guide(Canvas canvas, RectTransform target, GuideType guideType,TranslateType translateType = TranslateType.Direct,float time = 1) {

        Guide(target, guideType);

        switch (guideType)
        {
            case GuideType.Rect:
                rectGuide.Guide(canvas, target,translateType,time);
                break;
            case GuideType.Circle:
                circleGuide.Guide(canvas, target,translateType, time);
                break;
        }
    }

    public void Guide(Canvas canvas, RectTransform target, GuideType guideType,float scale,float time, TranslateType translateType = TranslateType.Direct, float moveTime = 1) {

        Guide(target, guideType);

        switch (guideType)
        {
            case GuideType.Rect:
                rectGuide.Guide(canvas, target,scale,time,translateType,moveTime);
                break;
            case GuideType.Circle:
                circleGuide.Guide(canvas, target,scale,time, translateType, moveTime);
                break;
        }
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (target == null) { return false; } // 事件不会渗透 

        return !RectTransformUtility.RectangleContainsScreenPoint(target,sp);
    }
}
