using UnityEngine;
using UnityEngine.UI;

public class CameraAndUISwitcher : MonoBehaviour
{
    // 保留你原有1-4号摄像机的变量（不修改）
    public Camera Camera1;
    public Camera Camera2;
    public Camera Camera3;
    public Camera Camera4;
    private int currentCamera = 1;

    // 新增：背包Canvas引用（拖入你的背包Canvas）
    public Canvas BackpackCanvas;
    // 新增：存储所有需要关联背包的摄像机（包括1-4号+后续新增）
    public Camera[] allCamerasForBackpack;

    [Header("UI按钮")]
    public Button BtnLeft;
    public Button BtnRight;

    [Header("UI画布")]
    public Canvas UiCanvas;

    [Header("布局参数")]
    public int Offset = 10;
    public Vector2 ButtonSize = new Vector2(1, 1);

    void Start()
    {
        // 新增：游戏启动时强制激活Camera1，禁用其他摄像机
        Camera1.gameObject.SetActive(true);
        Camera2.gameObject.SetActive(false);
        Camera3.gameObject.SetActive(false);
        Camera4.gameObject.SetActive(false);

        // 新增：将背包Canvas和场景UI Canvas绑定到Camera1
        if (UiCanvas != null)
        {
            UiCanvas.worldCamera = Camera1;
        }
        if (BackpackCanvas != null)
        {
            BackpackCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            BackpackCanvas.worldCamera = Camera1;
            BackpackCanvas.planeDistance = 1; // 确保UI渲染距离
        }
        // 原有按钮绑定逻辑（完全保留）
        BtnLeft.onClick.AddListener(SwitchLeft);
        BtnRight.onClick.AddListener(SwitchRight);

        // 新增：初始化时将背包关联到第一个激活的摄像机
        if (BackpackCanvas != null && allCamerasForBackpack.Length > 0)
        {
            BackpackCanvas.worldCamera = GetActiveCamera();
            BackpackCanvas.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // 新增：每一帧检测当前激活的摄像机，同步关联背包Canvas
        Camera activeCam = GetActiveCamera();
        if (activeCam != null && BackpackCanvas != null)
        {
            BackpackCanvas.worldCamera = activeCam;
        }
    }

    // 保留你原有1-4号摄像机切换逻辑（完全不修改）
    void SwitchLeft()
    {
        currentCamera--;
        if (currentCamera < 1) currentCamera = 4;
        SwitchCamera(currentCamera);
    }

    void SwitchRight()
    {
        currentCamera++;
        if (currentCamera > 4) currentCamera = 1;
        SwitchCamera(currentCamera);
    }

    void SwitchCamera(int camNum)
    {
        // 原有1-4号摄像机激活逻辑（完全保留）
        Camera1.gameObject.SetActive(camNum == 1);
        Camera2.gameObject.SetActive(camNum == 2);
        Camera3.gameObject.SetActive(camNum == 3);
        Camera4.gameObject.SetActive(camNum == 4);

        // 原有UI画布关联逻辑（保留）
        Camera currentActiveCam = camNum == 1 ? Camera1 : (camNum == 2 ? Camera2 : (camNum == 3 ? Camera3 : Camera4));
        if (UiCanvas != null)
        {
            UiCanvas.worldCamera = currentActiveCam;
        }
    }

    // 新增：获取当前场景中激活的摄像机（兼容1-4号+新增摄像机）
    private Camera GetActiveCamera()
    {
        // 先检查1-4号摄像机是否有激活的（按钮切换的情况）
        if (Camera1.isActiveAndEnabled) return Camera1;
        if (Camera2.isActiveAndEnabled) return Camera2;
        if (Camera3.isActiveAndEnabled) return Camera3;
        if (Camera4.isActiveAndEnabled) return Camera4;

        // 若1-4号都未激活，检查新增的摄像机数组
        foreach (Camera cam in allCamerasForBackpack)
        {
            if (cam != null && cam.isActiveAndEnabled)
            {
                return cam;
            }
        }

        // 兜底：返回第一个可用的摄像机
        return allCamerasForBackpack.Length > 0 ? allCamerasForBackpack[0] : null;
    }
}