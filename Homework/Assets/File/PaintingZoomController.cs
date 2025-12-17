using UnityEngine;

/// <summary>
/// 控制油画放大面板的显示，供背包交互成功后调用。
/// </summary>
public class PaintingZoomController : MonoBehaviour
{
    public GameObject zoomPanel;

    public void ShowZoom()
    {
        if (zoomPanel != null)
        {
            zoomPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("zoomPanel 未绑定，无法显示放大UI");
        }
    }
}

