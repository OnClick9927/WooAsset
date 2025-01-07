using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid : MonoBehaviour
{

    public Number number;  // 当这个格子的数字

    // 判断是不是有数字
    public bool IsHaveNumber() {
        return number != null;
    }
    // 获取这个格子的数字
    public Number GetNumber() {
        return number;
    }

    // 设置数字
    public void SetNumber(Number number)
    {
        this.number = number;
    }

}
