using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NoviceGuidePanel : View
{

    private GuideController guideController;

    private Step[] steps;

    private int currentStep;

    private Canvas canvas;

    public static NoviceGuidePanel _instance;

    private int tempStep;

    private bool isExcuting = false;

    public UnityEvent onFinshGuide;

    private void Awake()
    {
        _instance = this;
        guideController = transform.GetComponent<GuideController>();
        // 初始化所有的步骤
        InitSteps();

        canvas = transform.GetComponentInParent<Canvas>();
    }

    private void InitSteps() {
        steps = new Step[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            steps[i] = transform.GetChild(i).GetComponent<Step>();
        }
    }

    // 执行某一个步骤
    public void ExcuteStep(int step)
    {
        if (isExcuting) { return; }

        Show();
        // 隐藏所有的步骤
        HideAllSteps();

        isExcuting = true;
        tempStep = step;
        //currentStep = step;
        if (step >= 0 && step < steps.Length)
        {
            //steps[step].Excute(guideController, canvas);
            Invoke("Excute", steps[step].delayTime);
        }
    }

    private void Excute(){



        currentStep = tempStep;

        steps[this.currentStep].Excute(guideController,canvas);

        isExcuting = false;
    }

    public void NextStep(string eventName) {
        if ( eventName == steps[this.currentStep].EventName )
        {

            if ( this.currentStep + 1 >= steps.Length )
            {
                // 把所有的步数都走完了
                onFinshGuide?.Invoke();
                return;
            }

            Hide();
            //this.currentStep++;
            ExcuteStep(this.currentStep+1);
        }
    }

    // 隐藏所有的步骤
    private void HideAllSteps() {
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].Hide();
        }
    }

    public override void Hide()
    {
        //base.Hide();
        guideController.Guide(canvas, null, GuideType.Rect);
    }


}
