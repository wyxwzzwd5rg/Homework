using UnityEngine;
using UnityEngine.UI;

public class ZoomPanelManager : MonoBehaviour
{
    // 引用关闭按钮
    public Button closeButton;

    void Start()
    {
        // 绑定关闭按钮的点击事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    // 当关闭按钮被点击时
    public void OnCloseButtonClicked()
    {
        // 隐藏UI面板
        gameObject.SetActive(false);
        // 可选：恢复游戏
        // Time.timeScale = 1;
    }

    // 当点击面板空白区域时（可选）
    public void OnPanelClicked()
    {
        // 这里可以留空，或者也调用关闭函数
        // OnCloseButtonClicked();
    }
}