using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HuarongdaoManager : MonoBehaviour
{
    [Header("拼图配置")]
    public List<Button> puzzlePieces; // 8个拼图按钮（按3×3网格顺序排列，索引0-7）
    public int gridSize = 3; // 网格尺寸（3×3）

    [Header("刀子显示配置")]
    public GameObject bladeObj; // 刀子游戏对象（直接拖入场景中的刀子）

    private int emptyIndex = 7; // 空位对应的拼图列表索引（初始在最后一个拼图位置）
    private bool isPuzzleSolved = false;
    private List<Sprite> originalSprites = new List<Sprite>(); // 存储每个按钮的初始图片
    private string[] correctPieceNames; // 正确的拼图名称顺序（Piece_1~Piece_8）

    void Start()
    {
        // 初始化正确顺序数组
        correctPieceNames = new string[puzzlePieces.Count];
        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            correctPieceNames[i] = $"Piece_{i + 1}";
            originalSprites.Add(puzzlePieces[i].image.sprite);
            puzzlePieces[i].gameObject.name = correctPieceNames[i];
        }

        // 绑定拼图点击事件
        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            int idx = i;
            puzzlePieces[i].onClick.AddListener(() => OnPieceClicked(idx));
            puzzlePieces[i].interactable = true;
        }

        // 开局打乱拼图
        ShufflePuzzle();

        // 初始化刀子：仅隐藏
        InitBlade();
    }

    /// <summary>
    /// 打乱拼图（使用初始图片）
    /// </summary>
    public void ShufflePuzzle()
    {
        isPuzzleSolved = false;
        List<int> validShuffledIndices = GenerateValidShuffledIndices();

        // 同步乱序到8个拼图位置
        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            int originalIdx = validShuffledIndices[i];
            if (originalIdx == 8) // 空位
            {
                puzzlePieces[i].gameObject.name = "Piece_Empty";
                puzzlePieces[i].image.sprite = GetEmptySprite();
            }
            else
            {
                puzzlePieces[i].gameObject.name = $"Piece_{originalIdx + 1}";
                puzzlePieces[i].image.sprite = originalSprites[originalIdx];
            }
        }
    }

    /// <summary>
    /// 拼图点击事件
    /// </summary>
    public void OnPieceClicked(int clickedPieceIdx)
    {
        if (isPuzzleSolved) return;

        // 判断是否与空位相邻
        if (!IsAdjacentToEmpty(clickedPieceIdx))
        {
            Debug.Log($"拼图{clickedPieceIdx + 1}不与空位相邻，无法移动");
            return;
        }

        // 交换拼图与空位
        SwapPieceWithEmpty(clickedPieceIdx);

        // 检查是否通关
        if (IsPuzzleComplete())
        {
            isPuzzleSolved = true;
            ShowBladeDirectly(); // 通关后直接显示刀子
            GameData.AddCollectedItem("carpet_puzzle_completed");
            Debug.Log("华容道通关！刀子已显示，可点击收集");
        }
    }

    /// <summary>
    /// 生成有解的乱序索引
    /// </summary>
    private List<int> GenerateValidShuffledIndices()
    {
        List<int> indices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        int maxAttempts = 100;
        int attempts = 0;

        do
        {
            // 随机打乱
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int randomIdx = Random.Range(0, i + 1);
                (indices[i], indices[randomIdx]) = (indices[randomIdx], indices[i]);
            }

            // 找到空位的索引
            int emptyPos = indices.IndexOf(8);
            List<int> numsWithoutEmpty = indices.FindAll(idx => idx != 8);
            int inversions = CalculateInversions(numsWithoutEmpty);
            if (inversions % 2 == 0)
            {
                emptyIndex = emptyPos;
                return indices;
            }
            attempts++;
        } while (attempts < maxAttempts);

        // 兜底：返回有序索引
        return new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
    }

    /// <summary>
    /// 计算逆序数
    /// </summary>
    private int CalculateInversions(List<int> arr)
    {
        int inversions = 0;
        for (int i = 0; i < arr.Count; i++)
        {
            for (int j = i + 1; j < arr.Count; j++)
            {
                if (arr[i] > arr[j])
                {
                    inversions++;
                }
            }
        }
        return inversions;
    }

    /// <summary>
    /// 判断拼图是否与空位相邻
    /// </summary>
    private bool IsAdjacentToEmpty(int pieceIdx)
    {
        int pieceRow = pieceIdx / 3;
        int pieceCol = pieceIdx % 3;
        int emptyRow = emptyIndex / 3;
        int emptyCol = emptyIndex % 3;

        return (pieceRow == emptyRow && Mathf.Abs(pieceCol - emptyCol) == 1) ||
               (pieceCol == emptyCol && Mathf.Abs(pieceRow - emptyRow) == 1);
    }

    /// <summary>
    /// 交换拼图与空位
    /// </summary>
    private void SwapPieceWithEmpty(int pieceIdx)
    {
        string tempName = puzzlePieces[pieceIdx].gameObject.name;
        Sprite tempSprite = puzzlePieces[pieceIdx].image.sprite;

        puzzlePieces[pieceIdx].gameObject.name = $"Piece_Empty";
        puzzlePieces[pieceIdx].image.sprite = GetEmptySprite();

        puzzlePieces[emptyIndex].gameObject.name = tempName;
        puzzlePieces[emptyIndex].image.sprite = tempSprite;

        emptyIndex = pieceIdx;
    }

    /// <summary>
    /// 检查拼图是否完成
    /// </summary>
    private bool IsPuzzleComplete()
    {
        string[] correctNames = { "Piece_1", "Piece_2", "Piece_3", "Piece_4", "Piece_5", "Piece_6", "Piece_7", "Piece_8", "Piece_Empty" };
        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            if (puzzlePieces[i].gameObject.name != correctNames[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 初始化刀子：仅隐藏
    /// </summary>
    private void InitBlade()
    {
        if (bladeObj == null)
        {
            Debug.LogError("未指定刀子对象！请在Inspector中拖入bladeObj");
            return;
        }
        bladeObj.SetActive(false);

        // 确保刀子有收集组件（保留原有逻辑，不修改）
        EnsureCollectHandlerOnBlade();
    }

    /// <summary>
    /// 确保刀子有ItemClickHandler组件（适配现有脚本）
    /// </summary>
    private void EnsureCollectHandlerOnBlade()
    {
        ItemClickHandler handler = bladeObj.GetComponent<ItemClickHandler>();
        if (handler == null)
        {
            handler = bladeObj.AddComponent<ItemClickHandler>();
        }
        handler.itemId = "blade"; // 与收集逻辑保持一致
    }

    /// <summary>
    /// 通关后直接显示刀子（仅激活对象，无其他逻辑）
    /// </summary>
    private void ShowBladeDirectly()
    {
        if (bladeObj == null) return;
        // 未收集过才显示
        if (!GameData.IsItemCollected("blade"))
        {
            bladeObj.SetActive(true);
            Debug.Log("刀子已显示，可点击收集");
        }
    }

    /// <summary>
    /// 获取空位透明图片
    /// </summary>
    private Sprite GetEmptySprite()
    {
        Texture2D emptyTex = new Texture2D(1, 1);
        emptyTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
        emptyTex.Apply();
        return Sprite.Create(emptyTex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
    }

    /// <summary>
    /// 重新开始游戏（外部按钮调用）
    /// </summary>
    public void RestartGame()
    {
        ShufflePuzzle();
        isPuzzleSolved = false;

        // 重置刀子状态：仅隐藏
        if (bladeObj != null && !GameData.IsItemCollected("blade"))
        {
            bladeObj.SetActive(false);
        }
    }
}