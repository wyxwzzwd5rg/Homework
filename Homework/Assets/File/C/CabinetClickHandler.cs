using UnityEngine;

public class CabinetClickHandler : MonoBehaviour
{
    public Camera Camera2;   // 原场景的主摄像机（如 Camera2）
    public Camera cabinetCamera; // 柜子俯视摄像机（CabinetCamera）

    void OnMouseDown()
    {
        // 检查必要的引用是否存在
        if (Camera2 == null)
        {
            Debug.LogError("[柜子点击] Camera2未设置！");
            return;
        }
        if (cabinetCamera == null)
        {
            Debug.LogError("[柜子点击] cabinetCamera未设置！");
            return;
        }
        if (ViewManager.Instance == null)
        {
            Debug.LogWarning("[柜子点击] ViewManager.Instance为null，跳过EnterCabinetView调用");
            // 继续执行摄像机切换，但不调用ViewManager
        }
        else
        {
            ViewManager.Instance.EnterCabinetView();
        }

        // 点击柜子时，禁用原摄像机，启用俯视摄像机
        Camera2.gameObject.SetActive(false);
        cabinetCamera.gameObject.SetActive(true);
    }
}