using UnityEngine;
using UnityEngine.UI;

public class CameraAndUISwitcher : MonoBehaviour
{
    // 1-4号默认相机（原有配置）
    public Camera Camera1;
    public Camera Camera2;
    public Camera Camera3;
    public Camera Camera4;

    // UI Canvas配置（原有）
    public Canvas UiCanvas;          // 场景主UI画布
    public Canvas BackpackCanvas;    // 背包画布

    // 背包关联的所有相机（需包含Video1Camera）
    public Camera[] allCamerasForBackpack;

    // 按钮引用（原有）
    public Button BtnLeft;
    public Button BtnRight;

    // 当前激活的相机编号
    private int currentCamIndex = 1;

    void Start()
    {
        // 初始化按钮监听（原有逻辑）
        if (BtnLeft != null)
        {
            BtnLeft.onClick.AddListener(OnBtnLeftClick);
        }
        if (BtnRight != null)
        {
            BtnRight.onClick.AddListener(OnBtnRightClick);
        }

        // 初始化激活1号相机
        SwitchCamera(currentCamIndex);

        // 确保背包Canvas默认激活
        if (BackpackCanvas != null && !BackpackCanvas.gameObject.activeSelf)
        {
            BackpackCanvas.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // 实时同步背包Canvas到当前激活的相机
        Camera activeCam = GetActiveCamera();
        if (activeCam != null && BackpackCanvas != null)
        {
            BackpackCanvas.worldCamera = activeCam;
            // 确保背包Canvas始终激活（防止被误关闭）
            if (!BackpackCanvas.gameObject.activeSelf)
            {
                BackpackCanvas.gameObject.SetActive(true);
            }
        }
    }

    // 左按钮点击逻辑（切换上一个相机）
    void OnBtnLeftClick()
    {
        currentCamIndex--;
        if (currentCamIndex < 1)
        {
            currentCamIndex = 4; // 循环到最后一个默认相机
        }
        SwitchCamera(currentCamIndex);
    }

    // 右按钮点击逻辑（切换下一个相机）
    void OnBtnRightClick()
    {
        currentCamIndex++;
        if (currentCamIndex > 4)
        {
            currentCamIndex = 1; // 循环到第一个默认相机
        }
        SwitchCamera(currentCamIndex);
    }

    // 核心：切换相机并同步UI（补充Video1Camera适配）
    void SwitchCamera(int camNum)
    {
        // 1. 激活/关闭1-4号默认相机（原有逻辑）
        if (Camera1 != null) Camera1.gameObject.SetActive(camNum == 1);
        if (Camera2 != null) Camera2.gameObject.SetActive(camNum == 2);
        if (Camera3 != null) Camera3.gameObject.SetActive(camNum == 3);
        if (Camera4 != null) Camera4.gameObject.SetActive(camNum == 4);

        // 2. 同步1-4号相机的主UI Canvas（原有逻辑）
        Camera currentActiveCam = null;
        switch (camNum)
        {
            case 1: currentActiveCam = Camera1; break;
            case 2: currentActiveCam = Camera2; break;
            case 3: currentActiveCam = Camera3; break;
            case 4: currentActiveCam = Camera4; break;
        }
        if (UiCanvas != null && currentActiveCam != null)
        {
            UiCanvas.worldCamera = currentActiveCam;
        }

        // 3. 新增：处理非1-4号相机（如Video1Camera）的UI同步
        Camera activeCam = GetActiveCamera();
        if (activeCam != null && !IsInDefaultCameras(activeCam))
        {
            // 同步主UI到当前激活的非默认相机
            if (UiCanvas != null)
            {
                UiCanvas.worldCamera = activeCam;
            }
            // 同步背包UI到当前激活的非默认相机
            if (BackpackCanvas != null)
            {
                BackpackCanvas.worldCamera = activeCam;
            }
        }
    }

    // 辅助：获取当前场景中激活的相机
    private Camera GetActiveCamera()
    {
        // 先检查1-4号默认相机
        if (Camera1 != null && Camera1.gameObject.activeSelf) return Camera1;
        if (Camera2 != null && Camera2.gameObject.activeSelf) return Camera2;
        if (Camera3 != null && Camera3.gameObject.activeSelf) return Camera3;
        if (Camera4 != null && Camera4.gameObject.activeSelf) return Camera4;

        // 再检查背包关联的其他相机（如Video1Camera）
        if (allCamerasForBackpack != null)
        {
            foreach (Camera cam in allCamerasForBackpack)
            {
                if (cam != null && cam.gameObject.activeSelf)
                {
                    return cam;
                }
            }
        }

        return null;
    }

    // 辅助：判断相机是否为1-4号默认相机
    private bool IsInDefaultCameras(Camera cam)
    {
        return cam == Camera1 || cam == Camera2 || cam == Camera3 || cam == Camera4;
    }

    // 可选：外部调用切换相机的方法（比如时钟关卡切换到Video1Camera时调用）
    public void SwitchToCustomCamera(Camera targetCam)
    {
        // 关闭所有默认相机
        if (Camera1 != null) Camera1.gameObject.SetActive(false);
        if (Camera2 != null) Camera2.gameObject.SetActive(false);
        if (Camera3 != null) Camera3.gameObject.SetActive(false);
        if (Camera4 != null) Camera4.gameObject.SetActive(false);

        // 激活目标相机（如Video1Camera）
        if (targetCam != null)
        {
            targetCam.gameObject.SetActive(true);
            // 同步UI到目标相机
            if (UiCanvas != null) UiCanvas.worldCamera = targetCam;
            if (BackpackCanvas != null) BackpackCanvas.worldCamera = targetCam;
        }
    }

    // 可选：恢复到默认相机（视频播放结束后调用）
    public void RestoreToDefaultCamera(int defaultCamNum = 1)
    {
        SwitchCamera(defaultCamNum);
    }
}