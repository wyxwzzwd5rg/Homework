using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HuarongdaoManager : MonoBehaviour
{
    [Header("拼图配置")]
    public List<Button> puzzlePieces; // 8个拼图按钮（按3×3网格顺序排列，索引0-7）
    public int gridSize = 3; // 网格尺寸（3×3）

    [Header("暗格配置")]
    public GameObject secretCompartment; // 暗格容器
    public string bladeObjectName = "Blade"; // 刀片物体名称

    private int emptyIndex = 7; // 空位对应的拼图列表索引（初始在最后一个拼图位置）
    private bool isPuzzleSolved = false;
    private List<Sprite> originalSprites = new List<Sprite>(); // 存储每个按钮的初始图片（从Inspector设置的图片）
    private string[] correctPieceNames; // 正确的拼图名称顺序（Piece_1~Piece_8）

    void Start()
    {
        // 初始化正确顺序数组
        correctPieceNames = new string[puzzlePieces.Count];
        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            correctPieceNames[i] = $"Piece_{i + 1}";
            // 记录每个按钮的初始图片（从Inspector设置的图片）
            originalSprites.Add(puzzlePieces[i].image.sprite);
            // 初始化按钮名称（用于通关判断）
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

        // 初始化暗格状态（隐藏刀片）
        InitSecretCompartment();
    }

    /// <summary>
    /// 打乱拼图（使用初始图片，不依赖外部文件）
    /// </summary>
    public void ShufflePuzzle()
    {
        isPuzzleSolved = false;
        List<int> validShuffledIndices = GenerateValidShuffledIndices();

        // 同步乱序到9个位置
        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            int originalIdx = validShuffledIndices[i];
            // 若当前位置是空位（originalIdx=8），则显示空位图片
            if (originalIdx == 8)
            {
                puzzlePieces[i].gameObject.name = "Piece_Empty";
                puzzlePieces[i].image.sprite = GetEmptySprite();
            }
            else
            {
                // 否则显示对应数字的图片（originalIdx=0~7对应Piece_1~Piece_8）
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

        // 判断是否与空位相邻（仅相邻可移动）
        if (!IsAdjacentToEmpty(clickedPieceIdx))
        {
            Debug.Log($"拼图{clickedPieceIdx + 1}不与空位相邻，无法移动");
            return;
        }

        // 交换拼图与空位（更新图片和名称）
        SwapPieceWithEmpty(clickedPieceIdx);

        // 检查是否通关
        if (IsPuzzleComplete())
        {
            isPuzzleSolved = true;
            OpenSecretCompartment();
            Debug.Log("华容道通关！暗格已打开");
        }
    }

    /// <summary>
    /// 生成有解的乱序索引（对应originalSprites的索引）
    /// </summary>
    private List<int> GenerateValidShuffledIndices()
    {
        List<int> indices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 }; // 9个位置的索引
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

            // 找到空位的索引（Piece_Empty的索引是8）
            int emptyPos = indices.IndexOf(8);
            // 计算逆序数（排除空位）
            List<int> numsWithoutEmpty = indices.FindAll(idx => idx != 8);
            int inversions = CalculateInversions(numsWithoutEmpty);
            // 3×3奇数网格：逆序数为偶数则有解
            if (inversions % 2 == 0)
            {
                emptyIndex = emptyPos; // 更新空位索引
                return indices;
            }
            attempts++;
        } while (attempts < maxAttempts);

        // 兜底：返回有序索引
        return new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
    }

    /// <summary>
    /// 计算索引数组的逆序数（判断乱序是否有解）
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
        // 3×3网格，计算拼图和空位的行列
        int pieceRow = pieceIdx / 3;
        int pieceCol = pieceIdx % 3;
        int emptyRow = emptyIndex / 3;
        int emptyCol = emptyIndex % 3;

        // 相邻判定：同一行/列，且行列差为1
        return (pieceRow == emptyRow && Mathf.Abs(pieceCol - emptyCol) == 1) ||
               (pieceCol == emptyCol && Mathf.Abs(pieceRow - emptyRow) == 1);
    }

    /// <summary>
    /// 交换拼图与空位（更新图片和名称）
    /// </summary>
    private void SwapPieceWithEmpty(int pieceIdx)
    {
        // 保存拼图的名称和图片
        string tempName = puzzlePieces[pieceIdx].gameObject.name;
        Sprite tempSprite = puzzlePieces[pieceIdx].image.sprite;

        // 拼图 ← 空位（更新为空位的图片和名称）
        puzzlePieces[pieceIdx].gameObject.name = $"Piece_Empty";
        puzzlePieces[pieceIdx].image.sprite = GetEmptySprite();

        // 空位 ← 拼图（更新为拼图的图片和名称）
        puzzlePieces[emptyIndex].gameObject.name = tempName;
        puzzlePieces[emptyIndex].image.sprite = tempSprite;

        // 更新空位索引
        emptyIndex = pieceIdx;
    }

    /// <summary>
    /// 检查拼图是否完成（按正确顺序排列）
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
    /// 初始化暗格（隐藏刀片）
    /// </summary>
    private void InitSecretCompartment()
    {
        if (secretCompartment == null) return;

        Transform blade = secretCompartment.transform.Find(bladeObjectName);
        if (blade != null)
        {
            blade.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError($"暗格中未找到名为{bladeObjectName}的物体");
        }
    }

    /// <summary>
    /// 打开暗格（显示刀片）
    /// </summary>
    private void OpenSecretCompartment()
    {
        if (secretCompartment == null) return;

        Transform blade = secretCompartment.transform.Find(bladeObjectName);
        if (blade != null)
        {
            blade.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 获取空位图片（透明图，无需外部资源）
    /// </summary>
    private Sprite GetEmptySprite()
    {
        // 动态生成一个1x1的透明图作为空位
        Texture2D emptyTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        emptyTexture.SetPixel(0, 0, new Color(0, 0, 0, 0)); // 透明色
        emptyTexture.Apply();
        return Sprite.Create(emptyTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
    }

    /// <summary>
    /// 重新开始游戏（外部按钮可调用）
    /// </summary>
    public void RestartGame()
    {
        ShufflePuzzle();
        InitSecretCompartment();
    }
}