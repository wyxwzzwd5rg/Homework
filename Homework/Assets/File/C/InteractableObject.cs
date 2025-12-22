using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    public string objectTag;
    public GameObject springSprite; // 新增：拖入场景中的弹簧Sprite（平级对象）
    [Tooltip("如果为true，始终允许交互；否则需被激活（用于布谷鸟等逻辑）")]
    public bool alwaysActive = false;
    [Tooltip("交互成功时触发的事件（可在Inspector绑定隐藏藤蔓、打开抽屉等逻辑）")]
    public UnityEvent onInteractSuccess;
    [Tooltip("如果为true，交互成功后自动隐藏自己（用于藤蔓等物体）")]
    public bool autoHideOnSuccess = false;
    [Tooltip("场景A：如果勾选，交互成功时会解锁所有抽屉（用于镜片+藤蔓交互）")]
    public bool unlockDrawersOnSuccess = false;
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
        if (!alwaysActive && !isCuckooActive)
        {
            Debug.LogError("交互对象未激活，不执行交互");
            return;
        }
        TriggerInteract(); // 调用新增的方法
    }

    // 新增：供Button绑定的方法
    public void TriggerInteract()
    {
        if (BackpackManager.Instance != null)
        {
            Debug.LogError($"调用背包交互方法，对象Tag={objectTag}");
            BackpackManager.Instance.OnInteractWithObject(objectTag, this);
        }
        else
        {
            Debug.LogError("找不到BackpackManager实例!");
        }
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
        // 先触发Inspector中绑定的事件
        onInteractSuccess?.Invoke();
        
        // 如果设置了自动隐藏，则隐藏自己（用于藤蔓等物体）
        if (autoHideOnSuccess)
        {
            gameObject.SetActive(false);
            Debug.Log($"[交互] {gameObject.name} 已自动隐藏");
        }

        // 场景A：如果设置了解锁抽屉，则解锁所有抽屉（用于镜片+藤蔓交互）
        if (unlockDrawersOnSuccess)
        {
            UnlockAllDrawers();
        }

        // 场景A：如果设置了显示道具，则显示道具（用于小刀+人物等交互）
        if (showItemOnSuccess != null)
        {
            ShowItemObject();
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
    }

    // 解锁所有抽屉（场景A：镜片+藤蔓交互时调用）
    private void UnlockAllDrawers()
    {
        // 记录藤蔓已清除（用于解锁抽屉）
        GameData.AddCollectedItem("vine_cleared");

        // 解锁所有抽屉
        DrawerController[] allDrawers = FindObjectsOfType<DrawerController>();
        foreach (var drawer in allDrawers)
        {
            if (drawer != null)
            {
                drawer.CheckUnlockStatus();
                Debug.Log($"[场景A] 抽屉已解锁：{drawer.gameObject.name}");
            }
        }

        Debug.Log($"[场景A] 镜片切开藤蔓，抽屉已解锁！");
    }

    // 显示道具物体（场景A：小刀+人物等交互时调用）
    private void ShowItemObject()
    {
        if (showItemOnSuccess == null) return;

        ItemClickHandler itemHandler = showItemOnSuccess.GetComponent<ItemClickHandler>();
        if (itemHandler != null)
        {
            // 显示道具（如果未收集）
            itemHandler.CheckAndSetActive();
            Debug.Log($"[场景A] 道具已显示：{showItemOnSuccess.name}");
        }
        else
        {
            Debug.LogError($"[场景A] 道具物体缺少ItemClickHandler组件！");
        }
    }
}