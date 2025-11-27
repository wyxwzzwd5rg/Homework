using UnityEngine;

public class ClockViewController : MonoBehaviour
{
    // --- 请在这里填写你的引用 ---
    public Camera clockCloseUpCamera;       // 时钟特写视角的相机 (ClockCamera)
    public Canvas clockCanvas;               // 时钟特写视角的UI (ClockCanvas)
    public Camera previousCameraForClock;    // 时钟返回后要激活的相机 (RoomBCamera)
    // -------------------------

    // 当玩家点击时钟时，调用此方法
    public void OnClockClicked()
    {
        // 1. 禁用“上一级视角”的相机
        if (previousCameraForClock != null)
        {
            previousCameraForClock.enabled = false;
        }

        // 2. 激活时钟特写视角的相机和UI
        if (clockCloseUpCamera != null)
        {
            clockCloseUpCamera.enabled = true;
        }
        if (clockCanvas != null)
        {
            clockCanvas.enabled = true;
        }

        Debug.Log("已切换到时钟特写视角");
    }

    // 当点击时钟UI上的返回按钮 (anniu2) 时，调用此方法
    public void OnClockBackClicked()
    {
        // 1. 禁用时钟特写视角的相机和UI
        if (clockCloseUpCamera != null)
        {
            clockCloseUpCamera.enabled = false;
        }
        if (clockCanvas != null)
        {
            clockCanvas.enabled = false;
        }

        // 2. 激活“上一级视角”的相机
        if (previousCameraForClock != null)
        {
            previousCameraForClock.enabled = true;
        }

        Debug.Log("已从时钟特写视角返回");
    }
}