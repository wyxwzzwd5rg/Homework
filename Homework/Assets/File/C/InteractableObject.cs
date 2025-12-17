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
        onInteractSuccess?.Invoke();
    }
}