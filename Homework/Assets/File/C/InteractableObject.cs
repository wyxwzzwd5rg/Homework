using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    public string objectTag;
    public GameObject springSprite; // 新增：拖入场景中的弹簧Sprite（平级对象）
    [Tooltip("如果为true，始终允许交互；否则需被激活（用于布谷鸟等逻辑）")]
    public bool alwaysActive = false;
    
    // 用于调试：检查组件是否正确设置
    void Start()
    {
        // 检查是否有 Collider（OnMouseDown 需要 Collider 才能工作）
        Collider2D col2D = GetComponent<Collider2D>();
        Collider col3D = GetComponent<Collider>();
        
        if (col2D == null && col3D == null)
        {
            Debug.LogError($"[交互对象检查] {gameObject.name} 缺少 Collider 或 Collider2D 组件！OnMouseDown 无法工作。请添加 BoxCollider2D 或 Collider。");
        }
        else
        {
            if (col2D != null)
            {
                Debug.Log($"[交互对象检查] {gameObject.name} 有 Collider2D，IsTrigger={col2D.isTrigger}，Enabled={col2D.enabled}");
            }
            if (col3D != null)
            {
                Debug.Log($"[交互对象检查] {gameObject.name} 有 Collider，IsTrigger={col3D.isTrigger}，Enabled={col3D.enabled}");
            }
        }
        
        Debug.Log($"[交互对象检查] {gameObject.name} - objectTag={objectTag}，alwaysActive={alwaysActive}");
    }
    [Tooltip("交互成功时触发的事件（可在Inspector绑定隐藏藤蔓、打开抽屉等逻辑）")]
    public UnityEvent onInteractSuccess;
    [Tooltip("如果为true，交互成功后自动隐藏自己（用于藤蔓等物体）")]
    public bool autoHideOnSuccess = false;
    [Tooltip("场景A：如果勾选，交互成功时会解锁所有抽屉（用于镜片+藤蔓交互）")]
    public bool unlockDrawersOnSuccess = false;
    [Tooltip("场景A：交互成功后要显示的物体（用于荆棘消失后显示柜子等）")]
    public GameObject showObjectOnSuccess;
    [Tooltip("场景A：交互成功后要显示的道具物体（带ItemClickHandler，用于小刀+人物等交互）")]
    public GameObject showItemOnSuccess;
    [Tooltip("场景A：如果勾选，交互成功时不会消耗物品（用于放大镜查看油画等）")]
    public bool dontConsumeItem = false;
    [Tooltip("场景A：交互成功后要显示的UI面板（用于放大镜+油画等）")]
    public GameObject showUIPanelOnSuccess;
    [Tooltip("场景A：交互成功后要隐藏的物体（用于溶解剂+油画等）")]
    public GameObject hideObjectOnSuccess;

    private bool isCuckooActive = false; // 新增：记录布谷鸟是否已显示

    // 新增：供时钟脚本调用，激活布谷鸟（此时弹簧仍隐藏）
    public void ShowCuckoo()
    {
        gameObject.SetActive(true);
        isCuckooActive = true;
        Debug.Log("显示布谷鸟！");
    }

    // 原OnMouseDown方法（保留，用于鼠标点击触发）
    void OnMouseDown()
    {
        Debug.Log($"[点击检测] {gameObject.name} 被点击了！");
        
        // 检查是否点击到了UI元素（但允许背包等小窗口UI，只阻止全屏UI）
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            // 获取点击到的UI对象
            UnityEngine.EventSystems.PointerEventData pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            pointerData.position = Input.mousePosition;
            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
            
            // 检查是否点击到了全屏UI（比如密码面板、放大UI等）
            bool isBlockingUI = false;
            foreach (var result in results)
            {
                // 如果点击到的是Canvas根节点或全屏面板，则阻止交互
                if (result.gameObject.name.Contains("Panel") || 
                    result.gameObject.name.Contains("Canvas") ||
                    result.gameObject.GetComponent<Canvas>() != null)
                {
                    // 检查是否是背包UI（背包UI应该允许交互继续）
                    if (!result.gameObject.name.Contains("Backpack") && 
                        !result.gameObject.name.Contains("ItemSlot"))
                    {
                        isBlockingUI = true;
                        Debug.Log($"[点击检测] 点击到了全屏UI：{result.gameObject.name}，阻止交互");
                        break;
                    }
                }
            }
            
            if (isBlockingUI)
            {
                return;
            }
            else
            {
                Debug.Log($"[点击检测] 点击到了UI但允许交互继续（可能是背包等小窗口）");
            }
        }

        Debug.Log($"[点击检测] alwaysActive={alwaysActive}，isCuckooActive={isCuckooActive}");
        if (!alwaysActive && !isCuckooActive)
        {
            Debug.LogError($"[点击检测] 交互对象未激活，不执行交互。物体名={gameObject.name}");
            return;
        }
        
        Debug.Log($"[点击检测] 开始触发交互，物体名={gameObject.name}，objectTag={objectTag}");
        TriggerInteract(); // 调用新增的方法
    }

    // 新增：供Button绑定的方法
    public void TriggerInteract()
    {
        Debug.Log($"[交互触发] 开始交互，对象Tag={objectTag}，物体名={gameObject.name}");
        
        if (BackpackManager.Instance == null)
        {
            Debug.LogError("[交互触发] 找不到BackpackManager实例!请确保场景中有BackpackManager物体。");
            return;
        }
        
        Debug.Log($"[交互触发] 调用背包交互方法，对象Tag={objectTag}");
        BackpackManager.Instance.OnInteractWithObject(objectTag, this);
    }

    // 供BackpackManager调用，单独激活弹簧
    public void ShowSpring()
    {
        if (springSprite != null)
        {
            springSprite.SetActive(true);
            Debug.Log("弹簧已显示！");
        }
    }

    // 供通用交互调用：让Inspector中绑定的行为执行
    public void InvokeSuccessEvent()
    {
        Debug.Log($"[交互成功] ========== 开始执行交互成功事件 ==========");
        Debug.Log($"[交互成功] 物体名={gameObject.name}");
        Debug.Log($"[交互成功] autoHideOnSuccess={autoHideOnSuccess}");
        Debug.Log($"[交互成功] unlockDrawersOnSuccess={unlockDrawersOnSuccess}");
        Debug.Log($"[交互成功] showObjectOnSuccess={(showObjectOnSuccess != null ? showObjectOnSuccess.name : "null")}");
        
        // 先触发Inspector中绑定的事件
        onInteractSuccess?.Invoke();
        Debug.Log($"[交互成功] 已触发Inspector绑定的事件");
        
        // 如果设置了自动隐藏，则隐藏自己（用于藤蔓等物体）
        if (autoHideOnSuccess)
        {
            Debug.Log($"[交互成功] 准备隐藏物体：{gameObject.name}");
            gameObject.SetActive(false);
            Debug.Log($"[交互成功] ✓ {gameObject.name} 已自动隐藏");
        }
        else
        {
            Debug.Log($"[交互成功] autoHideOnSuccess=false，不隐藏物体");
        }

        // 场景A：如果设置了显示物体，则显示物体（用于荆棘消失后显示柜子等）
        if (showObjectOnSuccess != null)
        {
            showObjectOnSuccess.SetActive(true);
            Debug.Log($"[场景A] ✓ 物体已显示：{showObjectOnSuccess.name}");
        }
        else
        {
            Debug.Log($"[场景A] showObjectOnSuccess=null，不显示物体");
        }

        // 场景A：如果设置了解锁抽屉，则解锁所有抽屉（用于镜片+藤蔓交互）
        if (unlockDrawersOnSuccess)
        {
            Debug.Log($"[交互成功] 开始解锁抽屉");
            UnlockAllDrawers();
        }

        // 场景A：如果设置了显示道具，则显示道具（用于小刀+人物、溶解剂+油画等交互）
        if (showItemOnSuccess != null)
        {
            Debug.Log($"[交互成功] 准备显示道具：{showItemOnSuccess.name}");
            ShowItemObject();
        }
        else
        {
            Debug.Log($"[交互成功] showItemOnSuccess=null，不显示道具");
        }

        // 场景A：如果设置了显示UI面板，则显示UI（用于放大镜+油画等）
        if (showUIPanelOnSuccess != null)
        {
            showUIPanelOnSuccess.SetActive(true);
            Debug.Log($"[场景A] UI面板已显示：{showUIPanelOnSuccess.name}");
        }

        // 场景A：如果设置了隐藏物体，则隐藏物体（用于溶解剂+油画等）
        if (hideObjectOnSuccess != null)
        {
            hideObjectOnSuccess.SetActive(false);
            Debug.Log($"[场景A] 物体已隐藏：{hideObjectOnSuccess.name}");
        }
        
        Debug.Log($"[交互成功] 交互成功事件执行完成");
    }

    // 解锁所有抽屉（场景A：镜片+藤蔓交互时调用）
    private void UnlockAllDrawers()
    {
        // 记录藤蔓已清除（用于解锁抽屉，让柜子可以点击）
        GameData.AddCollectedItem("vine_cleared");
        Debug.Log($"[场景A] 已记录藤蔓清除：vine_cleared");

        // 解锁所有抽屉（让柜子可以点击打开）
        DrawerController[] allDrawers = FindObjectsOfType<DrawerController>();
        foreach (var drawer in allDrawers)
        {
            if (drawer != null)
            {
                drawer.CheckUnlockStatus();
                Debug.Log($"[场景A] 抽屉已解锁：{drawer.gameObject.name}");
            }
        }

        Debug.Log($"[场景A] 镜片切开藤蔓，柜子已显露，可以点击打开！");
    }

    // 显示道具物体（场景A：小刀+人物、溶解剂+油画等交互时调用）
    private void ShowItemObject()
    {
        if (showItemOnSuccess == null) return;

        // 确保物体是激活的
        showItemOnSuccess.SetActive(true);
        Debug.Log($"[场景A] 道具物体已激活：{showItemOnSuccess.name}");

        ItemClickHandler itemHandler = showItemOnSuccess.GetComponent<ItemClickHandler>();
        if (itemHandler != null)
        {
            // 显示道具（如果未收集）
            itemHandler.CheckAndSetActive();
            Debug.Log($"[场景A] 道具已显示：{showItemOnSuccess.name}");
            
            // 确保道具可见且可点击（类似SafeLockController的逻辑）
            EnsureItemVisibleAndClickable(showItemOnSuccess, itemHandler);
        }
        else
        {
            Debug.LogError($"[场景A] 道具物体缺少ItemClickHandler组件！");
        }
    }
    
    // 确保道具可见且可点击
    private void EnsureItemVisibleAndClickable(GameObject itemObj, ItemClickHandler itemHandler)
    {
        if (itemObj == null) return;

        // 1. 确保物体是激活的
        itemObj.SetActive(true);

        // 2. 确保SpriteRenderer可见
        SpriteRenderer itemRenderer = itemObj.GetComponent<SpriteRenderer>();
        if (itemRenderer != null)
        {
            itemRenderer.enabled = true;
            itemRenderer.color = new Color(itemRenderer.color.r, itemRenderer.color.g, itemRenderer.color.b, 1f); // 确保不透明
            // 设置合适的Sorting Order（确保在场景中可见）
            if (itemRenderer.sortingOrder == 0)
            {
                itemRenderer.sortingOrder = 1;
            }
            Debug.Log($"[场景A] 道具SpriteRenderer已设置，Sorting Order={itemRenderer.sortingOrder}");
        }

        // 3. 确保Collider2D可用
        Collider2D itemCollider = itemObj.GetComponent<Collider2D>();
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
            itemCollider.isTrigger = false; // 用于点击检测
        }
        else
        {
            // 如果没有Collider，添加一个
            BoxCollider2D newCollider = itemObj.AddComponent<BoxCollider2D>();
            newCollider.isTrigger = false;
            Debug.Log($"[场景A] 已为道具添加BoxCollider2D");
        }

        // 4. 确保位置正确
        itemObj.transform.position = new Vector3(itemObj.transform.position.x, itemObj.transform.position.y, 0);
        
        Debug.Log($"[场景A] ✓ 道具已确保可见且可点击：{itemObj.name}");
    }
}