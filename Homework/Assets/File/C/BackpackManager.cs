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
    public string solventName = "rongjieji";            // 溶解剂
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
    [Header("空槽背景")]
    public Sprite emptySlotSprite;


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
    // private void UpdateBackpackUI()
    // {
    //     if (itemSlots == null || itemSlots.Count == 0)
    //     {
    //         Debug.LogError("物品槽列表为空，请检查关联！");
    //         return;
    //     }
    //     // 先清空所有物品槽
    //     foreach (var slot in itemSlots)
    //     {

    //         slot.sprite = null; // 清空图片
    //         slot.enabled = true;
    //         slot.sprite = emptySlotSprite;
    //     }

    //     // 显示收集的物品
    //     for (int i = 0; i < collectedItems.Count; i++)
    //     {
    //         if (i < itemSlots.Count) // 避免超出物品槽数量
    //         {
    //             itemSlots[i].sprite = collectedItems[i]; // 给槽赋值物品图片

    //         }
    //         else
    //         {
    //             Debug.Log("背包已满！");
    //             break;
    //         }
    //     }
    // }

    private void UpdateBackpackUI()
    {
        // 调试1：检查itemSlots列表是否为空
        if (itemSlots == null || itemSlots.Count == 0)
        {
            Debug.LogError("[背包调试] itemSlots列表为空！请在Inspector中拖入ItemSlot的Image组件");
            return;
        }
        Debug.Log($"[背包调试] itemSlots数量：{itemSlots.Count}");

        // 调试2：遍历每个ItemSlot，打印激活状态、尺寸、位置
        for (int i = 0; i < itemSlots.Count; i++)
        {
            Image slot = itemSlots[i];
            if (slot == null)
            {
                Debug.LogError($"[背包调试] 第{i}个ItemSlot为空！");
                continue;
            }

            // 强制激活ItemSlot及其父物体
            slot.gameObject.SetActive(true);
            slot.transform.parent.gameObject.SetActive(true); // 激活BackpackPanel
            slot.enabled = true;

            // 打印关键信息
            Debug.Log($"[背包调试] 第{i}个ItemSlot：激活状态={slot.gameObject.activeSelf}，Image启用={slot.enabled}，尺寸={slot.rectTransform.sizeDelta}，位置={slot.rectTransform.anchoredPosition}，Sprite={slot.sprite?.name ?? "无"}");
        }

        // 原有逻辑：显示收集的物品
        for (int i = 0; i < collectedItems.Count; i++)
        {
            if (i < itemSlots.Count)
            {
                itemSlots[i].sprite = collectedItems[i];
                Debug.Log($"[背包调试] 第{i}个槽位赋值物品：{collectedItems[i].name}");
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
        Debug.LogError($"[背包选中] 选中物品：{selectedItem.name}，槽位索引：{slotIndex}");
        Debug.LogError($"[背包选中] 提示：现在可以点击场景中的交互对象（如藤蔓）来使用这个物品");
    }

    // BackpackManager.cs 中新增：
    // 通用交互方法：物品与场景对象交互
    public void OnInteractWithObject(string objectTag, InteractableObject interactObj)
    {
        Debug.LogError($"[交互调试] 进入交互逻辑：选中物品={selectedItem?.name ?? "null"}，交互对象Tag={objectTag}");

        if (selectedItem == null)
        {
            Debug.LogError("[交互调试] 交互失败：未选中任何物品！请先在背包中点击物品槽位选中物品。");
            return;
        }

        // 构建匹配键（物品名称 + 场景对象标签）
        string matchKey = $"{selectedItem.name}_{objectTag}";
        Debug.LogError($"[交互调试] 匹配键：{matchKey}，期望的镜片键：{mirrorPieceName}_Vine");

        // 根据“物品名称 + 场景对象标签”匹配交互逻辑
        switch (matchKey)
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
                Debug.LogError($"[镜片交互] 匹配成功！使用镜片与藤蔓交互");
                ConsumeSelectedItem();
                
                // 1. 记录藤蔓已清除（用于解锁抽屉）
                GameData.AddCollectedItem("vine_cleared");
                
                // 2. 触发交互成功事件（隐藏藤蔓等）
                interactObj.InvokeSuccessEvent();
                
                // 3. 解锁所有抽屉（查找场景中所有DrawerController并解锁）
                UnlockAllDrawers();
                
                Debug.LogError("[镜片交互] 镜片切开藤蔓，抽屉已解锁！");
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
            // 默认：未匹配到任何交互
            default:
                Debug.LogError($"[交互调试] 未找到匹配的交互逻辑！物品名={selectedItem.name}，对象Tag={objectTag}，匹配键={matchKey}");
                Debug.LogError($"[交互调试] 提示：请检查1)物品Sprite名字是否为'{mirrorPieceName}' 2)藤蔓的objectTag是否为'Vine'");
                break;
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

    // 解锁所有抽屉（镜片清除藤蔓后调用）
    private void UnlockAllDrawers()
    {
        DrawerController[] allDrawers = FindObjectsOfType<DrawerController>();
        foreach (var drawer in allDrawers)
        {
            if (drawer != null)
            {
                drawer.CheckUnlockStatus();
                Debug.Log($"[抽屉解锁] 已通知抽屉解锁：{drawer.gameObject.name}");
            }
        }
    }
}