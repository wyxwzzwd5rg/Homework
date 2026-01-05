using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 退出解谜返回按钮 - 单独绑定版本
/// 每个按钮可独立配置需要切换的相机
/// </summary>
public class ExitDrawersReturnBtn : MonoBehaviour
{
    [Header("相机配置")]
    [Tooltip("点击返回后需要启用的相机（原场景主相机）")]
    public Camera targetActiveCamera;   // 要启用的相机
    [Tooltip("点击返回后需要禁用的相机（解谜视角相机）")]
    public Camera targetDisableCamera;  // 要禁用的相机

    [Header("按钮引用（可选，自动获取自身按钮组件）")]
    public Button returnButton;

    private void Awake()
    {
        // 自动获取自身的Button组件（如果未手动指定）
        if (returnButton == null)
        {
            returnButton = GetComponent<Button>();
        }

        // 绑定按钮点击事件
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(OnReturnBtnClick);
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] 未找到Button组件，请检查是否挂载了Button组件！");
        }
    }

    /// <summary>
    /// 按钮点击回调 - 切换相机并退出解谜视角
    /// </summary>
    private void OnReturnBtnClick()
    {
        // 安全校验：避免空引用
        if (targetActiveCamera != null)
        {
            targetActiveCamera.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 未设置需要启用的相机！");
        }

        if (targetDisableCamera != null)
        {
            targetDisableCamera.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 未设置需要禁用的相机！");
        }

        // 退出解谜视角（复用原有ViewManager逻辑）
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.ExitDrawersView();
        }
        else
        {
            Debug.LogWarning("ViewManager.Instance 为空，请检查ViewManager是否存在！");
        }
    }

    // 可选：提供外部调用的切换相机方法
    public void SwitchCameraState()
    {
        OnReturnBtnClick();
    }
}