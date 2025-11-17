using UnityEngine;

public class CabinetClickHandler : MonoBehaviour
{
    public Camera Camera2;   // 原场景的主摄像机（如 Camera2）
    public Camera cabinetCamera; // 柜子俯视摄像机（CabinetCamera）

    void OnMouseDown()
    {
        // 点击柜子时，禁用原摄像机，启用俯视摄像机
        Camera2.gameObject.SetActive(false);
        cabinetCamera.gameObject.SetActive(true);
    }
}