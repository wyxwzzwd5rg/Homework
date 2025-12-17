using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraAspectRatioLock : MonoBehaviour
{
    public float targetAspect = 1920f / 1215f;
    private Camera cam;
    private int lastScreenWidth; // 记录上一帧的屏幕宽度
    private int lastScreenHeight; // 记录上一帧的屏幕高度

    void Start()
    {
        cam = GetComponent<Camera>();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        AdjustCamera();
    }

    void Update()
    {
        // 检测屏幕尺寸是否变化
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            AdjustCamera(); // 尺寸变化时重新适配
        }
    }

    void AdjustCamera()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}