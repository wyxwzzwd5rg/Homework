using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 背包管理器：单例模式（整个游戏只有一个实例，跨场景不销毁）
public class BackpackManager : MonoBehaviour
{
    [Header("背包UI配置")]
    public Canvas backpackCanvas;

    [Header("当前选中状态")]
    public int selectedSlotIndex = -1; // 当前选中的背包槽索引（-1表示未选中）
    [System.NonSerialized] // 防止Unity序列化，避免Inspector报错
    public Sprite selectedItem; // 当前选中的物品Sprite（运行时动态赋值，不需要在Inspector中设置）

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
        backpackCanvas = GetComponent<Canvas>();
        // 强制设置为全局最高层级
        backpackCanvas.sortingOrder = 999;
        backpackCanvas.overrideSorting = true;
        // 额外检查：确保物品槽在场景切换后仍有效
        UpdateBackpackUI();
    }

    void Start()
    {
        // 自动绑定背包槽位的点击事件（修复Target为空的问题）
        AutoBindSlotButtons();
    }

    // 自动绑定背包槽位的点击事件
    private void AutoBindSlotButtons()
    {
        if (itemSlots == null || itemSlots.Count == 0)
        {
            Debug.LogWarning("[背包绑定] itemSlots列表为空，无法自动绑定");
            return;
        }

        int boundCount = 0;
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i] == null) continue;

            // 获取Button组件（可能在Image的父物体上）
            Button btn = itemSlots[i].GetComponent<Button>();
            if (btn == null)
            {
                // 如果Image本身没有Button，检查父物体
                btn = itemSlots[i].transform.parent?.GetComponent<Button>();
            }

            if (btn != null)
            {
                // 清空原有事件
                btn.onClick.RemoveAllListeners();
                // 绑定到OnSlotClicked，传入槽位索引
                int slotIndex = i; // 捕获变量
                btn.onClick.AddListener(() => OnSlotClicked(slotIndex));
                boundCount++;
                Debug.Log($"[背包绑定] ✓ 已绑定槽位{i}的Button到OnSlotClicked({i})");
            }
        }

        Debug.Log($"[背包绑定] 完成：共绑定{boundCount}个槽位");
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

        // 不自动选中，需要用户手动在背包中点击槽位才能选中
        Debug.Log($"[收集物品] 已收集物品：{itemSprite.name}，请在背包中点击槽位选中该物品");
    }
    // BackpackManager.cs中新增方法（CollectItem下方）
    /// <summary>
    /// 收集弹簧物品（供ClockManager调用）
    /// </summary>
    /// <param name="springSprite">弹簧的Sprite</param>
    // BackpackManager.cs - CollectSpring 方法补充
    public void CollectSpring(Sprite springSprite)
    {
        // 修复：先检查是否重复收集弹簧
        if (collectedItems.Contains(springSprite))
        {
            Debug.Log($"[收集弹簧] 已收集过该弹簧：{springSprite.name}，无需重复收集");
            return;
        }

        CollectItem(springSprite);
        // 修复：移除重复的UpdateBackpackUI调用（CollectItem内部已调用），改为强制刷新层级
        RefreshBackpackLayer();
        Debug.Log($"[收集弹簧] 弹簧已添加到背包：{springSprite.name}");
    }
    public void RefreshBackpackLayer()
    {
        if (backpackCanvas != null)
        {
            backpackCanvas.sortingOrder = 999;
            Debug.Log("背包UI层级已强制设为999");
        }
    }

    // 更新背包UI：将收集的物品显示到物品槽中
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

            // 修复：先重置所有槽位为空白背景，再赋值物品
            slot.sprite = emptySlotSprite;

            // 打印关键信息
            // Debug.Log($"[背包调试] 第{i}个ItemSlot：激活状态={slot.gameObject.activeSelf}，Image启用={slot.enabled}，尺寸={slot.rectTransform.sizeDelta}，位置={slot.rectTransform.anchoredPosition}，Sprite={slot.sprite?.name ?? "无"}");
        }

        // 原有逻辑：显示收集的物品
        for (int i = 0; i < collectedItems.Count; i++)
        {
            if (i < itemSlots.Count)
            {
                itemSlots[i].sprite = collectedItems[i];
                Debug.Log($"[背包调试] 第{i}个槽位赋值物品：{collectedItems[i].name}");
            }
            else
            {
                Debug.LogWarning("[背包调试] 背包槽位不足，无法显示全部物品");
                break;
            }
        }

        // 新增：选中状态校验（避免选中已被消耗的槽位）
        if (selectedSlotIndex >= collectedItems.Count)
        {
            selectedSlotIndex = -1;
            selectedItem = null;
            Debug.Log("[背包调试] 选中槽位超出物品数量，已重置选中状态");
        }
    }

    // 检查是否拥有某个物品（后续解密可用，比如需要钥匙才能开门）
    public bool HasItem(Sprite itemSprite)
    {
        return collectedItems.Contains(itemSprite);
    }

    // 选中槽位、记录选中物品（原有逻辑）
    public void OnSlotClicked(int slotIndex)
    {
        Debug.Log($"[背包选中] ========== OnSlotClicked 被调用 ==========");
        Debug.Log($"[背包选中] 槽位索引：{slotIndex}");
        Debug.Log($"[背包选中] 背包物品数量：{collectedItems.Count}");
        Debug.Log($"[背包选中] 当前selectedItem：{(selectedItem != null ? selectedItem.name : "null")}");

        if (slotIndex < 0 || slotIndex >= collectedItems.Count)
        {
            Debug.LogError($"[背包选中] × 槽位索引超出范围: {slotIndex}，背包物品数量: {collectedItems.Count}");
            Debug.LogError($"[背包选中] 提示：槽位索引应该从0开始，最大为 {collectedItems.Count - 1}");
            // 重置选中状态，避免错误
            selectedItem = null;
            return;
        }
        // 1. 验证槽位索引是否有效
        if (slotIndex < 0)
        {
            Debug.LogError($"[背包选中] ✗ 槽位索引不能为负数：{slotIndex}");
            selectedItem = null;
            selectedSlotIndex = -1;
            return;
        }

        if (collectedItems.Count == 0)
        {
            Debug.LogError($"[背包选中] ✗ 背包为空，没有物品可选中");
            selectedItem = null;
            selectedSlotIndex = -1;
            return;
        }

        if (slotIndex >= collectedItems.Count)
        {
            Debug.LogError($"[背包选中] ✗ 槽位索引超出范围：{slotIndex}，背包物品数量：{collectedItems.Count}");
            Debug.LogError($"[背包选中] 提示：槽位索引应该从0开始，最大为{collectedItems.Count - 1}");
            selectedItem = null;
            selectedSlotIndex = -1;
            return;
        }

        // 2. 获取槽位对应的物品
        Sprite currentItem = collectedItems[slotIndex];
        if (currentItem == null)
        {
            Debug.LogError($"[背包选中] ✗ 槽位{slotIndex}的物品为null");
            selectedItem = null;
            selectedSlotIndex = -1;
            return;
        }

        // 3. 赋值selectedItem（与原有逻辑一致）
        selectedItem = currentItem;
        selectedSlotIndex = slotIndex;
        Debug.Log($"[背包选中] ✓ 选中物品：{selectedItem.name}，槽位索引：{slotIndex}");
        Debug.Log($"[背包选中] ✓ selectedItem已设置，现在可以点击场景中的交互对象");
    }

    // 测试方法：用于检查背包状态
    [ContextMenu("测试：打印背包状态")]
    public void TestPrintBackpackState()
    {
        Debug.Log($"[测试] 背包物品数量：{collectedItems.Count}");
        Debug.Log($"[测试] 当前选中物品：{(selectedItem != null ? selectedItem.name : "null")}");
        Debug.Log($"[测试] 当前选中槽位索引：{selectedSlotIndex}");
        for (int i = 0; i < collectedItems.Count; i++)
        {
            Debug.Log($"[测试] 槽位{i}：{(collectedItems[i] != null ? collectedItems[i].name : "null")}");
        }
    }

    // BackpackManager.cs 中新增：
    // 通用交互方法：物品与场景对象交互
    public void OnInteractWithObject(string objectTag, InteractableObject interactObj)
    {
        Debug.Log($"[交互调试] ========== OnInteractWithObject 被调用 ==========");
        Debug.Log($"[交互调试] objectTag={objectTag}");
        Debug.Log($"[交互调试] selectedSlotIndex={selectedSlotIndex}");

        // 先检查是否有选中物品（在访问name之前）
        if (selectedItem == null)
        {
            Debug.LogWarning($"[交互调试] ✗ 交互失败：未选中任何物品！");
            Debug.LogWarning($"[交互调试] 提示：请先在背包中点击物品槽位选中物品，然后再点击场景中的交互对象");
            Debug.LogWarning($"[交互调试] 当前背包物品数量：{collectedItems.Count}");
            if (collectedItems.Count > 0)
            {
                Debug.LogWarning($"[交互调试] 背包中的物品：");
                for (int i = 0; i < collectedItems.Count; i++)
                {
                    string itemName = collectedItems[i] != null ? collectedItems[i].name : "null";
                    Debug.LogWarning($"[交互调试]   槽位{i}：{itemName}");
                }
            }
            return;
        }

        // 现在可以安全访问 selectedItem.name
        string selectedItemName = selectedItem != null ? selectedItem.name : "null";
        Debug.Log($"[交互调试] ========== 开始交互 ==========");
        Debug.Log($"[交互调试] 选中物品：{selectedItemName}（Sprite名字）");
        Debug.Log($"[交互调试] 交互对象Tag：{objectTag}");
        Debug.Log($"[交互调试] 交互对象名：{interactObj.gameObject.name}");

        // 构建匹配键（物品名称 + 场景对象标签）
        string matchKey = $"{selectedItemName}_{objectTag}";
        Debug.Log($"[交互调试] 匹配键：{matchKey}");
        Debug.Log($"[交互调试] 期望匹配：");
        Debug.Log($"[交互调试]   - jingpian_Vine（镜片+荆棘）");
        Debug.Log($"[交互调试]   - xiaodao_Person（小刀+人物）");
        Debug.Log($"[交互调试]   - fangdajing_Artwork（放大镜+艺术品）");
        Debug.Log($"[交互调试]   - rongjieji_PaintingBase（溶解剂+油画）");

        // 根据"物品名称 + 场景对象标签"匹配交互逻辑
        switch (matchKey)
        {
            // 案例1：螺丝刀与布谷鸟交互（原有逻辑，保持不变）
            // BackpackManager.cs - OnInteractWithObject 方法的镊子+弹簧分支补充
            case "niezi_Spring":
            case "niezi_spring":
            case "Niezi_Spring":
            case "Niezi_spring":
                Debug.Log($"[镊子+弹簧] ✓ 匹配成功！开始交互");
                // 补充：校验ClockManager实例
                if (ClockManager.Instance == null)
                {
                    Debug.LogError("[镊子+弹簧] 找不到ClockManager实例！");
                    return;
                }
                // 1. 触发弹簧隐藏+收集
                ClockManager.Instance.HideSpringAndCollect();
                // 2. 消耗镊子（核心：销毁背包中的镊子）
                ConsumeSelectedItem();
                Debug.Log($"[镊子+弹簧] ✓ 交互完成：弹簧已收集，镊子已消耗");
                break;
            // 场景A：镜片与荆棘交互（明确匹配，确保交互成功）
            case "jingpian_Vine":
            case "jingpian_vine":
            case "Jingpian_Vine":
            case "Jingpian_vine":
                Debug.Log($"[镜片+荆棘] ✓ 匹配成功！开始交互");
                // 触发交互成功事件（荆棘会消失，抽屉会解锁）
                interactObj.InvokeSuccessEvent();
                // 消耗镜片
                ConsumeSelectedItem();
                Debug.Log($"[镜片+荆棘] ✓ 交互完成：荆棘已消失，抽屉已解锁");
                break;
            // 场景A：小刀与人物交互（明确匹配，确保交互成功）
            case "xiaodao_Person":
            case "xiaodao_person":
            case "Xiaodao_Person":
            case "Xiaodao_person":
                Debug.Log($"[小刀+人物] ✓ 匹配成功！开始交互");
                // 触发交互成功事件（人物会显示放大镜道具）
                interactObj.InvokeSuccessEvent();
                // 消耗小刀
                ConsumeSelectedItem();
                Debug.Log($"[小刀+人物] ✓ 交互完成：放大镜已显示");
                break;
            // 场景A：放大镜与艺术品交互（明确匹配，确保交互成功）
            case "fangdajing_Artwork":
            case "fangdajing_artwork":
            case "Fangdajing_Artwork":
            case "Fangdajing_artwork":
                Debug.Log($"[放大镜+艺术品] ✓ 匹配成功！开始交互");
                // 触发交互成功事件（显示放大UI面板，不消耗放大镜）
                interactObj.InvokeSuccessEvent();
                // 不消耗放大镜（因为dontConsumeItem=true）
                Debug.Log($"[放大镜+艺术品] ✓ 交互完成：放大UI已显示，放大镜未消耗");
                break;
            // 场景A：溶解剂与油画交互（明确匹配，确保交互成功）
            case "rongjieji_PaintingBase":
            case "rongjieji_paintingbase":
            case "Rongjieji_PaintingBase":
            case "Rongjieji_paintingbase":
                Debug.Log($"[溶解剂+油画] ✓ 匹配成功！开始交互");
                // 触发交互成功事件（油画会消失，伞会显示）
                interactObj.InvokeSuccessEvent();
                // 消耗溶解剂
                ConsumeSelectedItem();
                Debug.Log($"[溶解剂+油画] ✓ 交互完成：油画已消失，伞已显示");
                break;
            // 默认：未匹配到任何交互，拒绝交互（防止错误匹配）
            default:
                string itemName = selectedItem != null ? selectedItem.name : "null";
                Debug.LogWarning($"[交互调试] ⚠ 未找到匹配的交互逻辑！");
                Debug.LogWarning($"[交互调试] 物品名={itemName}，对象Tag={objectTag}，匹配键={matchKey}");
                Debug.LogWarning($"[交互调试] 期望的匹配键：");
                Debug.LogWarning($"[交互调试]   - jingpian_Vine（镜片+荆棘）");
                Debug.LogWarning($"[交互调试]   - xiaodao_Person（小刀+人物）");
                Debug.LogWarning($"[交互调试]   - fangdajing_Artwork（放大镜+艺术品）");
                Debug.LogWarning($"[交互调试]   - rongjieji_PaintingBase（溶解剂+油画）");
                Debug.LogWarning($"[交互调试] 请检查：1)物品Sprite名字是否正确 2)交互对象的objectTag是否正确");

                // 明确拒绝不匹配的交互（防止油画和放大镜交互等错误情况）
                Debug.LogWarning($"[交互调试] ✗ 交互被拒绝：{itemName} 不能与 {objectTag} 交互");

                // 特殊检查：如果油画（PaintingBase）被放大镜点击，明确拒绝
                if (objectTag == "PaintingBase" && itemName == "fangdajing")
                {
                    Debug.LogError($"[交互错误] ✗ 油画（PaintingBase）只能与溶解剂交互，不能与放大镜交互！");
                    Debug.LogError($"[交互错误] 请使用溶解剂（rongjieji）与油画交互。");
                }
                // 特殊检查：如果艺术品（Artwork）被溶解剂点击，明确拒绝
                else if (objectTag == "Artwork" && itemName == "rongjieji")
                {
                    Debug.LogError($"[交互错误] ✗ 艺术品（Artwork）只能与放大镜交互，不能与溶解剂交互！");
                    Debug.LogError($"[交互错误] 请使用放大镜（fangdajing）与艺术品交互。");
                }

                // 不触发任何交互，直接返回
                return;
        }
    }

    // ---------- 工具方法 ----------
    private void ConsumeSelectedItem()
    {
        // 修复：增加安全校验，避免索引越界
        if (selectedSlotIndex < 0 || selectedSlotIndex >= collectedItems.Count)
        {
            Debug.LogError($"[消耗物品] 选中槽位索引无效：{selectedSlotIndex}，物品总数：{collectedItems.Count}");
            selectedSlotIndex = -1;
            selectedItem = null;
            return;
        }

        // 记录消耗的物品名称，便于调试
        string consumedItemName = collectedItems[selectedSlotIndex]?.name ?? "未知物品";
        collectedItems.RemoveAt(selectedSlotIndex);

        // 修复：先重置选中状态，再更新UI（避免UI更新时选中状态不一致）
        selectedSlotIndex = -1;
        selectedItem = null;

        UpdateBackpackUI();
        Debug.Log($"[消耗物品] 已消耗：{consumedItemName}，剩余物品数量：{collectedItems.Count}");
    }

}