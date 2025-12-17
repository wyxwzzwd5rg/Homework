using UnityEngine;

public class PaintingClickHandler : MonoBehaviour
{
    // 放大油画UI面板
    public GameObject zoomPanel;
    // 基础油画（用于溶解剂交互）
    public GameObject basePainting;
    // 溶解后掉落的伞道具
    public GameObject umbrellaItem;

    // 处理鼠标点击事件
    void OnMouseDown()
    {
        // 默认显示放大UI面板
        if (zoomPanel != null)
        {
            zoomPanel.SetActive(true);
        }
    }

    // 处理交互成功事件（由BackpackManager调用）
    public void OnInteractionSuccess(string interactionType)
    {
        switch (interactionType)
        {
            case "magnifier":
                // 放大镜交互，显示放大UI
                if (zoomPanel != null)
                {
                    zoomPanel.SetActive(true);
                }
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