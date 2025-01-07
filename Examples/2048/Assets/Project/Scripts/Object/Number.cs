using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WooAsset;

public class Number : MonoBehaviour,IRecycleObject
{
    private Image bg;
    private Text number_text;

    private MyGrid inGrid; // 这个数字所在的格子 

    public NumberStatus status;


    private float spawnScaleTime = 1;
    private bool isPlayingSpawnAnim = false;

    private float mergeSceleTime = 1;
    private float mergeSceleTimeBack = 1;
    private bool isPlayingMergeAnim = false;

    private float movePosTime = 1;
    private bool isMoving = false;
    private bool isDestroyOnMoveEnd = false;
    private Vector3 startMove, endMovePos;

    public Color[] bg_colors;
    public List<int> number_index;

    public AudioClip mergeClip;

    private void Awake()
    {
        bg = transform.GetComponent<Image>();
        number_text = transform.Find("Text").GetComponent<Text>();
    }


    // 初始化
    public void Init( MyGrid myGrid ) {
        //Debug.Log("初始化数字");
        myGrid.SetNumber(this);
        // 设置所在的格子 
        this.SetGrid(myGrid);
        // 给它一个初始化的数字
        this.SetNumber(2);
        status = NumberStatus.Normal;

        transform.localScale = Vector3.zero;

        PlaySpawnAnim(); // 播放动画
    }

    // 设置格子
    public void SetGrid( MyGrid myGrid )
    {
        this.inGrid = myGrid;
    }

    // 获取格子 
    public MyGrid GetGrid( )
    {
        return this.inGrid;
    }

    // 设置数字
    public void SetNumber(int number)
    {
        this.number_text.text = number.ToString();
        
        this.bg.color = this.bg_colors[number_index.IndexOf(number)];
        
    }
    // 获取数字
    public int GetNumber() {
        return int.Parse(number_text.text);
    }

    // 把这个数字移动到某一个格子的下面
    public void MoveToGrid( MyGrid myGrid )
    {
        transform.SetParent(myGrid.transform);
        // transform.localPosition = Vector3.zero;
        startMove = transform.localPosition;
        // endMovePos = myGrid.transform.position;

        movePosTime = 0;
        isMoving = true;

        this.GetGrid().SetNumber(null);

        // 设置格子
        myGrid.SetNumber(this);
        this.SetGrid(myGrid);
    }

    // 在移动结束时候销毁
    public void DestroyOnMoveEnd(MyGrid myGrid) {
        transform.SetParent(myGrid.transform);
        startMove = transform.localPosition;

        movePosTime = 0;
        isMoving = true;
        isDestroyOnMoveEnd = true;
    }

    // 合并
    public void Merge() {

        GamePanel gamePanel = GameObject.Find("Canvas/GamePanel").GetComponent<GamePanel>();
        gamePanel.AddScore(this.GetNumber());

        int number = this.GetNumber() * 2;
        this.SetNumber(number);
        if ( number == 2048 )
        {
            // 游戏胜利了
            gamePanel.GameWin();
        }

        status = NumberStatus.NotMerge;
        // 播放合并动画
        PlayMergeAnim();
        var asset = Assets.LoadAsset("Assets/Project/Audio/merge.wav");
        // 播放音效
        AudioManager._instance.PlaySound(asset.GetAsset<AudioClip>());
    }

    // 判断能不能合并
    public bool IsMerge( Number number )
    {
        if ( this.GetNumber() == number.GetNumber() && number.status == NumberStatus.Normal )
        {
            return true;
        }
        return false;
    }

    // 播放创建动画
    public void PlaySpawnAnim() {
        spawnScaleTime = 0;
        isPlayingSpawnAnim = true;
    }
    public void PlayMergeAnim() {
        mergeSceleTime = 0;
        mergeSceleTimeBack = 0;
        isPlayingMergeAnim = true;
    }
    

    private void Update()
    {
        // 创建动画

        if (isPlayingSpawnAnim) {
            if (spawnScaleTime <= 1)
            {
                spawnScaleTime += Time.deltaTime * 4;
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, spawnScaleTime);
            }
            else {
                isPlayingSpawnAnim = false;
            }
        }


        // 合并动画

        if (isPlayingMergeAnim) {
            if (mergeSceleTime <= 1 && mergeSceleTimeBack == 0) // 变大的过程
            {
                mergeSceleTime += Time.deltaTime * 4;
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, mergeSceleTime);
            }
            if (mergeSceleTime >= 1 && mergeSceleTimeBack <= 1)
            {
                mergeSceleTimeBack += Time.deltaTime * 4;
                transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, mergeSceleTimeBack);
            }

            if ( mergeSceleTime >=1 && mergeSceleTimeBack >= 1 )
            {
                isPlayingMergeAnim = false;
            }
        }


        // 移动动画
        if (isMoving)
        {
            movePosTime += Time.deltaTime * 5;
            transform.localPosition = Vector3.Lerp(startMove, Vector3.zero, movePosTime);
            // Debug.Log(" movePosTime: " + movePosTime + " pos: " + Vector3.Lerp(startMove, Vector3.zero, movePosTime));
            if (movePosTime >= 1)
            {
                isMoving = false;
                if (isDestroyOnMoveEnd) {
                    //GameObject.Destroy(gameObject);
                    ObjectPool.Instance.RecoverObject(gameObject);
                }
            }
        }


    }

    public void Destroy()
    {
        this.GetGrid().SetNumber(null);
        ObjectPool.Instance.RecoverObject(gameObject);
    }

    public void OnLoad()
    {
        // TODO

    }

    public void OnRecover()
    {
        //Debug.Log("回收当前数字");
        // TODO
        // this.GetGrid().SetNumber(null);
        this.SetGrid(null);
        isDestroyOnMoveEnd = false;
        this.status = NumberStatus.Normal;
    }
}
