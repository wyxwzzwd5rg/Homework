using UnityEngine;

public class CabinetViewController : MonoBehaviour
{
    // --- 请在这里填写你的引用 ---
    public Camera cabinetCloseUpCamera;      // 柜子特写视角的相机 (CabinetCamera)
    public Canvas drawersCanvas;             // 柜子特写视角的UI (DrawersCanvas)
    public Camera previousCameraForCabinet;  // 柜子返回后要激活的相机 (RoomACamera)
    // -------------------------

    // 当玩家点击柜子时，调用此方法
    // 你需要将柜子的点击事件（如Button或EventTrigger）绑定到这个方法上
    public void OnCabinetClicked()
    {
        // 1. 禁用“上一级视角”的相机
        if (previousCameraForCabinet != null)
        {
            previousCameraForCabinet.enabled = false;
        }

        // 2. 激活柜子特写视角的相机和UI
        if (cabinetCloseUpCamera != null)
        {
            cabinetCloseUpCamera.enabled = true;
        }
        if (drawersCanvas != null)
        {
            drawersCanvas.enabled = true;
        }

        Debug.Log("已切换到柜子特写视角");
    }

    // 当点击柜子UI上的返回按钮 (anniu1) 时，调用此方法
    public void OnCabinetBackClicked()
    {
        // 1. 禁用柜子特写视角的相机和UI
        if (cabinetCloseUpCamera != null)
        {
            cabinetCloseUpCamera.enabled = false;
        }
        if (drawersCanvas != null)
        {
            drawersCanvas.enabled = false;
        }

        // 2. 激活“上一级视角”的相机
        if (previousCameraForCabinet != null)
        {
            previousCameraForCabinet.enabled = true;
        }

        Debug.Log("已从柜子特写视角返回");
    }
}