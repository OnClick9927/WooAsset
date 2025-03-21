using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour
{

    // 显示
    public virtual void Show() {
        gameObject.SetActive(true);
    }
    // 隐藏
    public virtual void Hide() {
        gameObject.SetActive(false);
    }

}
