using UnityEngine;

public class PaintingClickHandler : MonoBehaviour
{
    // 引用放大的UI面板
    public GameObject zoomPanel;

    void OnMouseDown()
    {
        // 当鼠标点击该物体时
        if (zoomPanel != null)
        {
            // 显示UI面板
            zoomPanel.SetActive(true);
            // 可选：暂停游戏
            // Time.timeScale = 0;
        }
    }
}