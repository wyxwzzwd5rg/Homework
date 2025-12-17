using UnityEngine;
using UnityEngine.UI; // 引入Unity UI的命名空间
[RequireComponent(typeof(CanvasScaler))]
public class CanvasAspectRatioLock : MonoBehaviour
{
    public float targetAspect = 1920f / 1215f; // 目标比例（比如4:3填4/3，1:1填1）

    private CanvasScaler canvasScaler;

    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        if (canvasScaler == null)
        {
            canvasScaler = GetComponentInParent<CanvasScaler>();
        }
        LockAspectRatio();
    }

    void LockAspectRatio()
    {
        // 空值校验，避免报错
        if (canvasScaler == null)
        {
            Debug.LogError("CanvasScaler组件未找到！请给挂载该脚本的对象添加Canvas Scaler组件");
            return;
        }

        // 原有逻辑（保留）
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;
        canvasScaler.matchWidthOrHeight = (scaleHeight < 1.0f) ? 1.0f : 0.0f;
    }

    // 屏幕尺寸变化时重新适配（比如窗口拉伸）
    void OnRectTransformDimensionsChange()
    {
        LockAspectRatio();
    }
}