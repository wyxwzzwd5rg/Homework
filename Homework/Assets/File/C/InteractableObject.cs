using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectTag;
    public GameObject springSprite; // 新增：拖入场景中的弹簧Sprite（平级对象）
    private bool isCuckooActive = false; // 新增：记录布谷鸟是否已显示

    // 新增：供时钟脚本调用，激活布谷鸟（此时弹簧仍隐藏）
    public void ShowCuckoo()
    {
        gameObject.SetActive(true);
        isCuckooActive = true;
        Debug.Log("显示布谷鸟！");
    }

    // void Update()
    // {
    //     // 按T键强制触发交互（测试用）
    //     if (Input.GetKeyDown(KeyCode.T) && isCuckooActive)
    //     {
    //         Debug.LogError("按T键触发布谷鸟交互");
    //         if (BackpackManager.Instance != null)
    //         {
    //             BackpackManager.Instance.OnInteractWithObject(objectTag, this);
    //         }
    //     }
    // }

    // 原OnMouseDown方法（保留，用于鼠标点击触发）
    void OnMouseDown()
    {
        Debug.LogError("布谷鸟被点击了!OnMouseDown触发");
        if (!isCuckooActive)
        {
            Debug.LogError("布谷鸟未激活，不执行交互");
            return;
        }
        TriggerInteract(); // 调用新增的方法
    }

    // 新增：供Button绑定的方法
    public void TriggerInteract()
    {
        if (BackpackManager.Instance != null)
        {
            Debug.LogError("调用背包交互方法");
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
}