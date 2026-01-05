using UnityEngine;

public class PaintingClickHandler : MonoBehaviour
{
    // 放大油画UI面板
    public GameObject zoomPanel;
    // 基础油画（用于溶解剂交互）
    public GameObject basePainting;
    // 溶解后掉落的伞道具
    public GameObject umbrellaItem;

    // InteractableObject组件引用（用于放大镜交互）
    private InteractableObject interactableObject;

    void Start()
    {
        // 获取InteractableObject组件（如果存在）
        interactableObject = GetComponent<InteractableObject>();
    }

    // 处理鼠标点击事件
    void OnMouseDown()
    {
        // 检查是否选中了放大镜
        if (BackpackManager.Instance != null && 
            BackpackManager.Instance.selectedItem != null &&
            BackpackManager.Instance.selectedItem.name == "fangdajing")
        {
            // 如果选中了放大镜，触发InteractableObject的交互逻辑
            if (interactableObject != null)
            {
                Debug.Log("[艺术品] 检测到放大镜已选中，触发交互逻辑");
                interactableObject.TriggerInteract();
            }
            else
            {
                Debug.LogWarning("[艺术品] 选中了放大镜，但未找到InteractableObject组件！请添加该组件。");
                // 即使没有InteractableObject，也直接打开面板
                OpenZoomPanel();
            }
        }
        else
        {
            // 没有选中放大镜，不执行任何操作（必须用放大镜才能打开）
            Debug.Log("[艺术品] 未选中放大镜，无法打开面板。请先在背包中选中放大镜。");
        }
    }

    // 打开放大UI面板（公共方法，供InteractableObject调用）
    public void OpenZoomPanel()
    {
        if (zoomPanel != null)
        {
            zoomPanel.SetActive(true);
            Debug.Log("[艺术品] 放大UI面板已打开");
        }
        else
        {
            Debug.LogWarning("[艺术品] zoomPanel未设置，无法打开面板");
        }
    }

    // 处理交互成功事件（由BackpackManager调用，已废弃，改用InteractableObject）
    public void OnInteractionSuccess(string interactionType)
    {
        switch (interactionType)
        {
            case "magnifier":
                // 放大镜交互，显示放大UI
                OpenZoomPanel();
                break;
            case "solvent":
                // 溶解剂交互，油画消失并掉落伞
                if (basePainting != null)
                {
                    basePainting.SetActive(false);
                }
                if (umbrellaItem != null)
                {
                    umbrellaItem.SetActive(true);
                }
                break;
        }
    }
}