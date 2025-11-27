using UnityEngine;

public class ClockClickHandler : MonoBehaviour
{
    public Camera Camera3;   // 原场景的主摄像机（如 Camera2）
    public Camera clockCamera; // 柜子俯视摄像机（CabinetCamera）

    void OnMouseDown()
    {

        // 点击柜子时，禁用原摄像机，启用俯视摄像机
        Camera3.gameObject.SetActive(false);
        clockCamera.gameObject.SetActive(true);
        ViewManager.Instance.EnterClockView();
    }
}