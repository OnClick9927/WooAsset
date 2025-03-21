using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WooAsset;

public class GamePanel : MonoBehaviour
{

    #region UI控件
    public Text text_score;      // 分数
    public Text text_best_score; // 最高分
    public Button btn_last;      // 上一步
    public Button btn_restart;   // 重新开始
    public Button btn_exit;      // 退出

    public WinPanel winPanel;
    public LosePanel losePanel;

    #endregion


    #region 属性 变量



    public Transform gridParent; // 格子的父物体

    public Dictionary<int, int> grid_config = new Dictionary<int, int>() { { 4, 100 }, { 5, 80 }, { 6, 65 } };

    private int row;  // 行
    private int col;  // 列
    public MyGrid[][] grids = null; // 所有的格子 
    public List<MyGrid> canCreateNumberGrid = new List<MyGrid>();  // 可以创建数字的格子

    public GameObject gridPrefab;
    public GameObject numberPrefab;

    private Vector3 pointerDownPos, pointerUpPos;

    private bool isNeedCreateNumber = false;

    public int currentScore;
    public StepModel lastStepModel;

    public AudioClip bgClip;

    public NoviceGuidePanel guidePanel;

    #endregion


    #region 游戏逻辑

    // 初始化格子 
    public void InitGrid()
    {
        // 获取格子数量 
        int gridNum = PlayerPrefs.GetInt(Const.GameModel, 4);
        GridLayoutGroup gridLayoutGroup = gridParent.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.constraintCount = gridNum;
        gridLayoutGroup.cellSize = new Vector2(grid_config[gridNum], grid_config[gridNum]);

        // 初始化格子 
        grids = new MyGrid[gridNum][];

        // 创建格子 
        row = gridNum;
        col = gridNum;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                // 创建 i j  格子 
                if (grids[i] == null)
                {
                    grids[i] = new MyGrid[gridNum];
                }
                grids[i][j] = CreateGrid();
            }
        }

    }
    // 创建格子
    public MyGrid CreateGrid()
    {
        // 实例化格子预制体 
        GameObject gameObject = GameObject.Instantiate(gridPrefab, gridParent);

        return gameObject.GetComponent<MyGrid>();
    }
    // 创建数字
    public void CreateNumber()
    {

        // 找到这个数字所在的格子
        canCreateNumberGrid.Clear();
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (!grids[i][j].IsHaveNumber())
                {  // 判断这个格子有没有数字
                    // 如果没有数字
                    canCreateNumberGrid.Add(grids[i][j]);
                }
            }
        }
        if (canCreateNumberGrid.Count == 0)
        {
            return;
        }
        // 随机一个格子 
        int index = Random.Range(0, canCreateNumberGrid.Count);

        CreateNumber(canCreateNumberGrid[index], 2);
    }

    public void CreateNumber(MyGrid myGrid, int number)
    {
        GameObject gameObj = ObjectPool.Instance.LoadObject("Assets/Project/Prefab/Number.prefab");
        gameObj.transform.SetParent(myGrid.transform);
        gameObj.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        gameObj.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        // GameObject gameObj = GameObject.Instantiate(numberPrefab, myGrid.transform);
        gameObj.GetComponent<Number>().Init(myGrid); // 进行初始化
        gameObj.GetComponent<Number>().SetNumber(number);
    }

    public MoveType CaculateMoveType()
    {
        if (Mathf.Abs(pointerUpPos.x - pointerDownPos.x) > Mathf.Abs(pointerDownPos.y - pointerUpPos.y))
        {
            // 左右移动
            if (pointerUpPos.x - pointerDownPos.x > 0)
            {
                // 向右移动
                // Debug.Log("向右移动");
                return MoveType.RIGHT;
            }
            else
            {
                // 向左移动
                // Debug.Log("向左移动");
                return MoveType.LEFT;
            }
        }
        else
        {
            // 上下移动
            if (pointerUpPos.y - pointerDownPos.y > 0)
            {
                // 向上移动
                // Debug.Log("向上移动");
                return MoveType.TOP;
            }
            else
            {
                // 向下移动
                // Debug.Log("向下移动");
                return MoveType.DOWN;
            }

        }
    }

    public void MoveNumber(MoveType moveType)
    {
        switch (moveType)
        {
            case MoveType.TOP:

                for (int j = 0; j < col; j++)
                {
                    for (int i = 1; i < row; i++)
                    {
                        if (grids[i][j].IsHaveNumber())
                        {

                            Number number = grids[i][j].GetNumber();

                            // Debug.Log(" 坐标: "+ i+" , " +j);
                            for (int m = i - 1; m >= 0; m--)
                            {
                                Number targetNumber = null;
                                if (grids[m][j].IsHaveNumber())
                                {
                                    targetNumber = grids[m][j].GetNumber();
                                }
                                HandleNumber(number, targetNumber, grids[m][j]);

                                if (targetNumber != null) // 如果有数字跳出循环
                                {
                                    break;
                                }

                            }
                        }
                    }
                }

                break;
            case MoveType.DOWN:


                for (int j = 0; j < col; j++)
                {
                    for (int i = row - 2; i >= 0; i--)
                    {
                        if (grids[i][j].IsHaveNumber())
                        {

                            Number number = grids[i][j].GetNumber();

                            // Debug.Log(" 坐标: "+ i+" , " +j);
                            for (int m = i + 1; m < row; m++)
                            {

                                Number targetNumber = null;
                                if (grids[m][j].IsHaveNumber())
                                {
                                    targetNumber = grids[m][j].GetNumber();
                                }
                                HandleNumber(number, targetNumber, grids[m][j]);

                                if (targetNumber != null) // 如果有数字跳出循环
                                {
                                    break;
                                }

                            }
                        }
                    }
                }

                break;
            case MoveType.LEFT:

                for (int i = 0; i < row; i++)
                {
                    for (int j = 1; j < col; j++)
                    {
                        if (grids[i][j].IsHaveNumber())
                        {

                            Number number = grids[i][j].GetNumber();

                            // Debug.Log(" 坐标: "+ i+" , " +j);
                            for (int m = j - 1; m >= 0; m--)
                            {

                                Number targetNumber = null;
                                if (grids[i][m].IsHaveNumber())
                                {
                                    targetNumber = grids[i][m].GetNumber();
                                }
                                HandleNumber(number, targetNumber, grids[i][m]);

                                if (targetNumber != null) // 如果有数字跳出循环
                                {
                                    break;
                                }

                            }
                        }
                    }
                }

                break;
            case MoveType.RIGHT:
                for (int i = 0; i < row; i++)
                {
                    for (int j = col - 2; j >= 0; j--)
                    {
                        if (grids[i][j].IsHaveNumber())
                        {

                            Number number = grids[i][j].GetNumber();

                            // Debug.Log(" 坐标: "+ i+" , " +j);
                            for (int m = j + 1; m < col; m++)
                            {

                                Number targetNumber = null;
                                if (grids[i][m].IsHaveNumber())
                                {
                                    targetNumber = grids[i][m].GetNumber();
                                }
                                HandleNumber(number, targetNumber, grids[i][m]);

                                if (targetNumber != null) // 如果有数字跳出循环
                                {
                                    break;
                                }

                            }
                        }
                    }
                }

                break;
        }
    }

    public void HandleNumber(Number current, Number target, MyGrid targetGrid)
    {
        if (target != null)
        {
            // 判断能不能合并
            if (current.IsMerge(target))
            {
                target.Merge();

                // 销毁当前的数字
                current.GetGrid().SetNumber(null);
                // GameObject.Destroy(current.gameObject);
                current.DestroyOnMoveEnd(target.GetGrid());

                isNeedCreateNumber = true;
            }
        }
        else
        {
            // 没有数字
            current.MoveToGrid(targetGrid);
            isNeedCreateNumber = true;
        }
    }

    public void ResetNumberStatus()
    {
        // 遍历所有的数字
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (grids[i][j].IsHaveNumber())
                {
                    grids[i][j].GetNumber().status = NumberStatus.Normal;
                }
            }
        }
    }

    // 判断游戏是否失败
    public bool IsGameLose()
    {

        // 判断格子是否满了 
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (!grids[i][j].IsHaveNumber())
                {
                    return false;
                }
            }
        }
        // 判断有没有数字能够合并
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {

                MyGrid up = IsHaveGrid(i - 1, j) ? grids[i - 1][j] : null;
                MyGrid down = IsHaveGrid(i + 1, j) ? grids[i + 1][j] : null;
                MyGrid left = IsHaveGrid(i, j - 1) ? grids[i][j - 1] : null;
                MyGrid right = IsHaveGrid(i, j + 1) ? grids[i][j + 1] : null;
                // grids[i][j] 
                if (up != null)
                {
                    if (grids[i][j].GetNumber().IsMerge(up.GetNumber()))
                    {
                        return false;
                    }
                }

                if (down != null)
                {
                    if (grids[i][j].GetNumber().IsMerge(down.GetNumber()))
                    {
                        return false;
                    }
                }

                if (left != null)
                {
                    if (grids[i][j].GetNumber().IsMerge(left.GetNumber()))
                    {
                        return false;
                    }
                }

                if (right != null)
                {
                    if (grids[i][j].GetNumber().IsMerge(right.GetNumber()))
                    {
                        return false;
                    }
                }


            }
        }


        return true;// 游戏失败
    }

    public bool IsHaveGrid(int i, int j)
    {

        if (i >= 0 && i < row && j >= 0 && j < col)
        {
            return true;
        }

        return false;
    }

    #endregion


    #region 脚本周期

    private void Awake()
    {

        // 初始化界面信息
        InitPanelMessage();

        // 初始化格子
        InitGrid();
        // 创建第一个数字
        CreateNumber();

        if (MenuPanel.isGuide)
        {
            guidePanel.Show();
            guidePanel.ExcuteStep(0);
        }


    }
    #endregion



    #region 事件监听
    public void OnPointerDown()
    {
        // Debug.Log("按下:" + Input.mousePosition);
        pointerDownPos = Input.mousePosition;
    }

    private void GG(MoveType moveType)
    {
        lastStepModel.UpdateData(this.currentScore, PlayerPrefs.GetInt(Const.BestScore, 0), grids);
        btn_last.interactable = true;
        MoveNumber(moveType);

        // 产生数字
        if (isNeedCreateNumber)
        {

            Invoke("CreateNumber", 0.3f);
        }

        // 把所有数字的状态 恢复成正常状态
        ResetNumberStatus();
        isNeedCreateNumber = false;
        // 判断游戏是否结束
        if (IsGameLose())
        { // 说明游戏结束
            GameLose();
        }

        switch (moveType)
        {
            case MoveType.TOP:
                NoviceGuidePanel._instance.NextStep(GameGuideConst.MoveUp);
                break;
            case MoveType.DOWN:
                NoviceGuidePanel._instance.NextStep(GameGuideConst.MoveDown);
                break;
            case MoveType.LEFT:
                NoviceGuidePanel._instance.NextStep(GameGuideConst.MoveLeft);
                break;
            case MoveType.RIGHT:
                NoviceGuidePanel._instance.NextStep(GameGuideConst.MoveRight);
                break;
            default:
                break;
        }
    }
    public void OnPointerUp()
    {
        // Debug.Log("抬起:" + Input.mousePosition);
        pointerUpPos = Input.mousePosition;

        if (Vector3.Distance(pointerDownPos, pointerUpPos) < 100)
        {
            Debug.Log("无效的操作!");
            return;
        }
        GG(CaculateMoveType());
        // 保存数据
        //lastStepModel.UpdateData(this.currentScore, PlayerPrefs.GetInt(Const.BestScore, 0), grids);
        //btn_last.interactable = true;

        //MoveType moveType = CaculateMoveType();
        //Debug.Log("移动类型:" + moveType);


    }
    private void Update()
    {
        if (losePanel.gameObject.activeSelf) return;

        MoveType moveType = MoveType.None;
        if (Input.GetKeyDown(KeyCode.W))
            moveType = MoveType.TOP;
        if (Input.GetKeyDown(KeyCode.S))
            moveType = MoveType.DOWN;
        if (Input.GetKeyDown(KeyCode.A))
            moveType = MoveType.LEFT;
        if (Input.GetKeyDown(KeyCode.D))
            moveType = MoveType.RIGHT;
        //Debug.Log("移动类型:" + moveType);
        GG(moveType);

        //// 产生数字
        //MoveNumber(moveType);
        //if (!isNeedCreateNumber) return;
        //Invoke("CreateNumber", 0.3f);

        //// 把所有数字的状态 恢复成正常状态
        //ResetNumberStatus();
        //isNeedCreateNumber = false;
        //// 判断游戏是否结束
        //if (IsGameLose())
        //{ // 说明游戏结束
        //    GameLose();
        //}

        //switch (moveType)
        //{
        //    case MoveType.TOP:
        //        NoviceGuidePanel._instance.NextStep(GameGuideConst.MoveUp);
        //        break;
        //    case MoveType.DOWN:
        //        NoviceGuidePanel._instance.NextStep(GameGuideConst.MoveDown);
        //        break;
        //    case MoveType.LEFT:
        //        NoviceGuidePanel._instance.NextStep(GameGuideConst.MoveLeft);
        //        break;
        //    case MoveType.RIGHT:
        //        NoviceGuidePanel._instance.NextStep(GameGuideConst.MoveRight);
        //        break;
        //    default:
        //        break;
        //}
    }
    // 上一步
    public void OnLastClick()
    {

        BackToLastStep();
        btn_last.interactable = false;

        NoviceGuidePanel._instance.NextStep(GameGuideConst.ClickLast);
    }

    public void OnRestartClick()
    {
        RestartGame();
        NoviceGuidePanel._instance.NextStep(GameGuideConst.ClickRestart);
    }

    public void OnExitClick()
    {
        ExitGame();

        NoviceGuidePanel._instance.NextStep(GameGuideConst.ClickBack);
    }

    public void OnFinshGuide()
    {
        PlayerPrefs.SetInt(Const.Novice, 1);
    }

    #endregion

    #region 界面更新

    public void InitPanelMessage()
    {
        this.text_best_score.text = PlayerPrefs.GetInt(Const.BestScore, 0).ToString();
        lastStepModel = new StepModel();
        btn_last.interactable = false;

        // 播放音乐
        AudioManager._instance.PlayMusic(bgClip);

    }

    public void AddScore(int score)
    {
        currentScore += score;
        UpdateScore(currentScore);

        // 判断当前的分数是不是最高的分数
        if (currentScore > PlayerPrefs.GetInt(Const.BestScore, 0))
        {
            PlayerPrefs.SetInt(Const.BestScore, currentScore);
            UpdateBestScore(currentScore);
        }

    }

    public void UpdateScore(int score)
    {
        this.text_score.text = score.ToString();
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScore(currentScore);
    }

    public void UpdateBestScore(int bestScore)
    {
        this.text_best_score.text = bestScore.ToString();


    }

    #endregion


    #region 游戏流程

    public void BackToLastStep()
    {
        // 分数
        currentScore = lastStepModel.score;
        UpdateScore(lastStepModel.score);

        PlayerPrefs.SetInt(Const.BestScore, lastStepModel.bestScore);
        UpdateBestScore(lastStepModel.bestScore);

        // 数字
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (lastStepModel.numbers[i][j] == 0)
                {
                    if (grids[i][j].IsHaveNumber())
                    {

                        grids[i][j].GetNumber().Destroy();
                    }
                }
                else if (lastStepModel.numbers[i][j] != 0)
                {
                    if (grids[i][j].IsHaveNumber())
                    {
                        // 修改数字
                        grids[i][j].GetNumber().SetNumber(lastStepModel.numbers[i][j]);
                    }
                    else
                    {
                        // 创建数字
                        CreateNumber(grids[i][j], lastStepModel.numbers[i][j]);
                    }
                }
            }
        }
    }

    public static async void ExitGame()
    {
        // 把对象池清空
        SubPool subPool = ObjectPool.Instance.GetSubPool("Assets/Project/Prefab/Number.prefab");
        subPool?.RecoverAllObject();
        var asset = await Assets.LoadSceneAsset("Assets/Project/Scenes/01-menu.unity");
        asset.LoadScene(UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    // 重新开始
    public void RestartGame()
    {
        // 数据清空 

        btn_last.interactable = false;
        // 清空分数
        ResetScore();
        // 清空数字 
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {

                if (grids[i][j].IsHaveNumber())
                {
                    grids[i][j].GetNumber().Destroy();
                }
            }
        }
        // 创建一个数字 
        CreateNumber();

    }

    public void GameWin()
    {
        Debug.Log("游戏胜利了");
        winPanel.Show();
    }
    public void GameLose()
    {
        Debug.Log("游戏失败了");
        losePanel.Show();
    }

    #endregion



}
