using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// 中心点移动的类型
public enum TranslateType {
    Direct,
    Slow
}

[RequireComponent(typeof(Image))]
public class GuideBase : MonoBehaviour
{
    protected Material material; // 材质

    protected Vector3 center;    // 镂空区域的中心

    protected RectTransform target; // 要显示的目标

    protected Vector3[] targetCorners = new Vector3[4]; // 要引导的目标的边界

    #region Scale变化相关

    protected float scaleTimer;
    protected float scaleTime;
    protected bool isScaling;
    #endregion

    #region 中心点移动相关

    private Vector3 startCenter;

    private float centerTimer;
    private float centerTime;
    private bool isMoving;

    #endregion

    public Vector3 Center {
        get {
            if (material == null) { return Vector3.zero; }
            return material.GetVector("_Center");
        }
    }

    protected virtual void Start()
    {
        //material = transform.GetComponent<Image>().material;
        //if (material == null)
        //{
        //    throw new System.Exception(" 未获取到材质! ");
        //}
    }

    protected virtual void Update()
    {
        if (isScaling)
        {
            scaleTimer += Time.deltaTime * 1 / scaleTime;
            if (scaleTimer >= 1)
            {
                scaleTimer = 0;
                isScaling = false;
            }
        }

        if (isMoving)
        {
            centerTimer += Time.deltaTime * 1 / centerTime;

            // 设置中心点
            material.SetVector("_Center", Vector3.Lerp(startCenter, center, centerTimer));

            if (centerTimer >=1 )
            {
                centerTimer = 0;
                isMoving = false;
            }
        }

    }

    // 引导
    public virtual void Guide(Canvas canvas, RectTransform target,TranslateType translateType = TranslateType.Direct,float time = 1)
    {
        // 初始化材质
        material = transform.GetComponent<Image>().material;

        this.target = target;


        if (target != null)
        {
            // 获取中心点 
            target.GetWorldCorners(targetCorners);

            // 把世界坐标 转成屏幕坐标
            for (int i = 0; i < targetCorners.Length; i++)
            {
                targetCorners[i] = WorldToScreenPoint(canvas, targetCorners[i]);
            }
            // 计算中心点
            center.x = targetCorners[0].x + (targetCorners[3].x - targetCorners[0].x) / 2;
            center.y = targetCorners[0].y + (targetCorners[1].y - targetCorners[0].y) / 2;

            //Debug.Log(" 移动类型: " + translateType);

            switch (translateType)
            {
                case TranslateType.Direct:
                    // 设置中心点
                    material.SetVector("_Center", center);
                    break;
                case TranslateType.Slow:

                    startCenter = material.GetVector("_Center");

                    isMoving = true;
                    centerTimer = 0;
                    centerTime = time;
                    break;
            }
        }
        else {
            center = Vector3.zero;
            targetCorners[0] = new Vector3(-2000,-2000,0);
            targetCorners[1] = new Vector3(-2000, 2000, 0);
            targetCorners[2] = new Vector3(2000, 2000, 0);
            targetCorners[3] = new Vector3(2000, -2000, 0);
        }
         
    }

    public virtual void Guide(Canvas canvas, RectTransform target,float scale, float time, TranslateType translateType = TranslateType.Direct, float moveTime = 1) {

    }

    public Vector2 WorldToScreenPoint(Canvas canvas, Vector3 world)
    {
        // 把世界坐标转成 屏幕坐标
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, world);
        Vector2 localPoint;
        // 把屏幕坐标 转成 局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPoint, canvas.worldCamera, out localPoint);
        return localPoint;
    }

}
