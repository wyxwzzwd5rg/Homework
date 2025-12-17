using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 背包管理器：单例模式（整个游戏只有一个实例，跨场景不销毁）
public class BackpackManager : MonoBehaviour
{
    [Header("当前选中状态")]
    public int selectedSlotIndex = -1; // 当前选中的背包槽索引（-1表示未选中）
    public Sprite selectedItem; // 当前选中的物品Sprite

    [Header("场景A动态道具占位与命名")]
    public string mirrorPieceName = "jingpian";       // 镜片
    public string knifeName = "xiaodao";               // 小刀
    public string magnifierName = "fangdajing";        // 放大镜
    public string solventName = "rongjiej";            // 溶解剂
    public string umbrellaName = "yusan";              // 雨伞
    public Sprite mirrorPieceSprite;   // 保险柜1奖励
    public Sprite magnifierSprite;     // 人物交互奖励
    public Sprite solventSprite;       // 保险柜2奖励
    public Sprite umbrellaSprite;      // 溶解后掉落
    public Color placeholderColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    // 单例实例（全局唯一，其他脚本可通过 BackpackManager.Instance 访问）
    public static BackpackManager Instance;

    // 物品槽列表（拖入UI中的所有物品槽）
    public List<Image> itemSlots = new List<Image>();

    // 存储收集的物品（Sprite格式，对应物品图片）
    private static List<Sprite> collectedItems = new List<Sprite>();

    void Awake()
    {
        // 单例模式：确保场景中只有一个 BackpackManager，跨场景不销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景时不销毁该物体
        }
        else
        {
            Destroy(gameObject); // 若已有实例，销毁重复的
        }
        // 额外检查：确保物品槽在场景切换后仍有效
        UpdateBackpackUI();
    }

    // 收集物品的方法（其他脚本调用此方法添加物品）
    public void CollectItem(Sprite itemSprite)
    {
        // 避免重复收集同一物品（可选，根据需求关闭）
        if (collectedItems.Contains(itemSprite))
        {
            Debug.Log("已收集过该物品！");
            return;
        }

        // 添加物品到列表
        collectedItems.Add(itemSprite);
        // 更新背包UI显示
        UpdateBackpackUI();
    }

    // 更新背包UI：将收集的物品显示到物品槽中
    private void UpdateBackpackUI()
    {
        if (itemSlots == null || itemSlots.Count == 0)
        {
            Debug.LogError("物品槽列表为空，请检查关联！");
            return;
        }
        // 先清空所有物品槽
        foreach (var slot in itemSlots)
        {
            slot.sprite = null; // 清空图片
            slot.enabled = false; // 隐藏槽（无物品时不显示）
        }

        // 显示收集的物品
        for (int i = 0; i < collectedItems.Count; i++)
        {
            if (i < itemSlots.Count) // 避免超出物品槽数量
            {
                itemSlots[i].sprite = collectedItems[i]; // 给槽赋值物品图片
                itemSlots[i].enabled = true; // 显示槽
            }
            else
            {
                Debug.Log("背包已满！");
                break;
            }
        }
    }

    // 检查是否拥有某个物品（后续解密可用，比如需要钥匙才能开门）
    public bool HasItem(Sprite itemSprite)
    {
        return collectedItems.Contains(itemSprite);
    }

    // BackpackManager.cs 中新增：
    // 你的原有代码（选中槽位、记录选中物品）
    public void OnSlotClicked(int slotIndex)
    {
        // 1. 验证槽位索引是否有效
        if (slotIndex < 0 || slotIndex >= collectedItems.Count)
        {
            Debug.LogError($"无效槽位索引：{slotIndex}");
            selectedItem = null;
            return;
        }

        // 2. 获取槽位对应的物品
        Sprite currentItem = collectedItems[slotIndex];
        if (currentItem == null)
        {
            Debug.LogError($"槽位{slotIndex}无物品");
            selectedItem = null;
            return;
        }

        // 3. 赋值selectedItem
        selectedItem = currentItem;
        selectedSlotIndex = slotIndex; // 同时记录选中的槽位索引
        Debug.Log($"选中物品：{selectedItem.name}，槽位索引：{slotIndex}");
    }

    // BackpackManager.cs 中新增：
    // 通用交互方法：物品与场景对象交互
    public void OnInteractWithObject(string objectTag, InteractableObject interactObj)
    {
        Debug.LogError($"进入交互逻辑：选中物品={selectedItem?.name}，交互对象Tag={objectTag}");

        if (selectedItem == null)
        {
            Debug.LogError("交互失败：未选中任何物品！");
            return;
        }

        // 根据“物品名称 + 场景对象标签”匹配交互逻辑
        switch ($"{selectedItem.name}_{objectTag}")
        {
            // 案例1：螺丝刀与布谷鸟交互
            case "luosidao_CuckooBird":
                // 1. 移除背包中的螺丝刀（保持原有）
                ConsumeSelectedItem();
                // 2. 调用布谷鸟的ShowSpring方法，激活弹簧显示（替换原有的Instantiate）
                interactObj.ShowSpring();
                Debug.Log("使用螺丝刀，布谷鸟弹出弹簧！");
                break;
            // 镜片与藤蔓交互：清除藤蔓，允许打开抽屉
            case var key when key == $"{mirrorPieceName}_Vine":
                ConsumeSelectedItem();
                interactObj.InvokeSuccessEvent();
                Debug.Log("镜片切开藤蔓，抽屉可用");
                break;
            // 小刀与人物交互：获得放大镜
            case var key when key == $"{knifeName}_Person":
                ConsumeSelectedItem();
                CollectRuntimeItem(GetOrCreateSprite(magnifierSprite, magnifierName, new Color(0.9f, 0.9f, 0.6f, 1f)), "item_magnifier");
                interactObj.InvokeSuccessEvent();
                Debug.Log("使用小刀完成交互，获得放大镜");
                break;
            // 放大镜与油画交互：打开放大UI
            case var key when key == $"{magnifierName}_PaintingZoom":
                // 放大镜不消耗，保持选中
                interactObj.InvokeSuccessEvent();
                Debug.Log("使用放大镜查看油画，打开放大UI");
                break;
            // 溶解剂与油画交互：溶解并掉落雨伞
            case var key when key == $"{solventName}_PaintingBase":
                ConsumeSelectedItem();
                CollectRuntimeItem(GetOrCreateSprite(umbrellaSprite, umbrellaName, new Color(0.6f, 0.8f, 1f, 1f)), "item_umbrella");
                interactObj.InvokeSuccessEvent();
                Debug.Log("使用溶解剂，油画溶解，获得雨伞");
                break;
                // 后续新增物品交互时，直接在这里添加case即可
        }
    }

    // ---------- 工具方法 ----------
    private void ConsumeSelectedItem()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < collectedItems.Count)
        {
            collectedItems.RemoveAt(selectedSlotIndex);
            UpdateBackpackUI();
        }
        selectedSlotIndex = -1;
        selectedItem = null;
    }

    private void CollectRuntimeItem(Sprite sprite, string itemId)
    {
        if (sprite == null)
        {
            Debug.LogError($"CollectRuntimeItem失败：sprite为空，itemId={itemId}");
            return;
        }
        CollectItem(sprite);
        GameData.AddCollectedItem(itemId);
    }

    private Sprite GetOrCreateSprite(Sprite input, string desiredName, Color color)
    {
        if (input != null) return input;

        // 生成一个临时占位Sprite，避免素材缺失阻塞流程
        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++) colors[i] = color.a > 0 ? color : placeholderColor;
        tex.SetPixels(colors);
        tex.Apply();
        Sprite generated = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        generated.name = desiredName;
        Debug.LogWarning($"未找到素材，自动生成占位Sprite：{desiredName}");
        return generated;
    }
}