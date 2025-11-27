using UnityEngine;
using UnityEngine.UI;

public class CameraAndUISwitcher : MonoBehaviour
{
    [Header("场景摄像机")]
    public Camera camera1;
    public Camera camera2;
    public Camera camera3;
    public Camera camera4;

    [Header("UI按钮")]
    public Button btnLeft;
    public Button btnRight;

    [Header("UI画布")]
    public Canvas uiCanvas;

    [Header("按钮设置")]
    [Tooltip("按钮与场景边缘的距离（越大越靠外）")]
    public float offset = 5f;
    [Tooltip("按钮大小（避免太小看不见）")]
    public Vector2 buttonSize = new Vector2(100, 100);

    private int currentIndex = 0;
    private Camera[] cameras;
    private Camera currentCamera;
    private RectTransform leftRect;
    private RectTransform rightRect;

    void Start()
    {
        // 初始化组件引用
        cameras = new Camera[] { camera1, camera2, camera3, camera4 };
        leftRect = btnLeft.GetComponent<RectTransform>();
        rightRect = btnRight.GetComponent<RectTransform>();

        // 强制设置按钮大小（避免看不见）
        leftRect.sizeDelta = buttonSize;
        rightRect.sizeDelta = buttonSize;

        // 初始激活第一个摄像机
        SetActiveCamera(currentIndex);

        // 绑定按钮事件
        btnLeft.onClick.AddListener(OnLeftClick);
        btnRight.onClick.AddListener(OnRightClick);
    }

    void Update()
    {
        if (currentCamera != null)
        {
            UpdateButtonPositions();
        }
    }

    // 切换激活的摄像机
    void SetActiveCamera(int index)
    {
        // 禁用所有摄像机，激活目标摄像机
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
                cameras[i].gameObject.SetActive(i == index);
        }
        currentCamera = cameras[index];

        // 确保Canvas关联当前摄像机
        if (uiCanvas != null)
            uiCanvas.worldCamera = currentCamera;
    }

    // 更新按钮位置（确保在当前摄像机视野内）
    void UpdateButtonPositions()
    {
        // 计算摄像机视野范围（正交模式）
        float orthoSize = currentCamera.orthographicSize;
        float aspectRatio = currentCamera.aspect;
        float halfWidth = orthoSize * aspectRatio; // 视野半宽
        float halfHeight = orthoSize; // 视野半高

        // 摄像机位置（世界坐标）
        Vector3 camPos = currentCamera.transform.position;

        // 计算按钮世界坐标（确保在视野边缘内）
        Vector3 leftPos = new Vector3(
            camPos.x - halfWidth + (leftRect.sizeDelta.x / 200f), // 左边缘内（适配UI像素）
            camPos.y,
            camPos.z + 1f // 摄像机前方，不被遮挡
        );
        Vector3 rightPos = new Vector3(
            camPos.x + halfWidth - (rightRect.sizeDelta.x / 200f), // 右边缘内
            camPos.y,
            camPos.z + 1f
        );

        // 转换世界坐标到UI坐标
        leftRect.localPosition = WorldToUI(leftPos);
        rightRect.localPosition = WorldToUI(rightPos);
    }

    // 世界坐标转UI坐标（兼容任意摄像机位置）
    Vector2 WorldToUI(Vector3 worldPos)
    {
        // 世界坐标 → 屏幕坐标
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(currentCamera, worldPos);
        // 屏幕坐标 → UI局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas.GetComponent<RectTransform>(),
            screenPos,
            currentCamera,
            out Vector2 uiPos
        );
        return uiPos;
    }

    // 左按钮切换
    void OnLeftClick()
    {
        currentIndex = (currentIndex - 1 + cameras.Length) % cameras.Length;
        SetActiveCamera(currentIndex);
    }

    // 右按钮切换
    void OnRightClick()
    {
        currentIndex = (currentIndex + 1) % cameras.Length;
        SetActiveCamera(currentIndex);
    }
}