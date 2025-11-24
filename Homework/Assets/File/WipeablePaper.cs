using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class WipeablePaper : MonoBehaviour
{
    [Header("纸张资源")]
    public Sprite dustyPaperSprite; // 带灰尘的纸（场景中可点击的纸张Sprite）
    public Sprite cleanPaperSprite;  // 干净的纸（UI中显示的纹理）

    [Header("UI 引用")]
    public GameObject paperUI;       // 弹窗面板（需销毁）
    public RawImage paperDisplay;    // 显示纸张的RawImage

    [Header("擦拭设置")]
    public int baseWipeRadius = 500;  // 基础擦拭半径
    public float cleanThreshold = 0.8f; // 清洁度阈值

    [Header("销毁设置")]
    public float delayAfterClean = 2.0f; // 擦拭完成后停留时间（秒）

    private Texture2D workingTexture;
    private bool isCleaned = false;
    private bool isRevealed = false;
    private bool isUIOpen = false;
    private float actualWipeRadius = 50f;

    void Start()
    {
        if (!CheckReferences()) return;
        paperUI.SetActive(false);
        if (!isRevealed) PrepareWorkingTexture();
        BindDragEvent();
    }

    // UI激活时计算擦拭半径
    void OnEnable()
    {
        if (paperDisplay != null && paperUI.activeSelf)
        {
            CalculateActualWipeRadius();
        }
    }

    // 计算适配UI的实际擦拭半径
    private void CalculateActualWipeRadius()
    {
        if (paperDisplay == null || paperDisplay.rectTransform == null) return;
        Vector2 screenSize = RectTransformUtility.WorldToScreenPoint(Camera.main, paperDisplay.rectTransform.position);
        screenSize = paperDisplay.rectTransform.sizeDelta * paperDisplay.rectTransform.lossyScale;
        if (screenSize.x > 0 && paperDisplay.rectTransform.rect.width > 0)
        {
            float scaleFactor = screenSize.x / paperDisplay.rectTransform.rect.width;
            actualWipeRadius = baseWipeRadius * scaleFactor;
        }
        else
        {
            actualWipeRadius = baseWipeRadius;
        }
    }

    // 检查必要引用
    private bool CheckReferences()
    {
        if (dustyPaperSprite == null || cleanPaperSprite == null || paperUI == null || paperDisplay == null)
        {
            Debug.LogError("【WipeablePaper】请为所有公共变量赋值！");
            enabled = false;
            return false;
        }
        if (dustyPaperSprite.texture.width != cleanPaperSprite.texture.width || dustyPaperSprite.texture.height != cleanPaperSprite.texture.height)
        {
            Debug.LogError("【WipeablePaper】带灰尘的纸和干净的纸纹理尺寸必须一致！");
            enabled = false;
            return false;
        }
        return true;
    }

    // 准备擦拭用纹理
    private void PrepareWorkingTexture()
    {
        if (!dustyPaperSprite.texture.isReadable)
        {
            Debug.LogError($"【WipeablePaper】请勾选 {dustyPaperSprite.name} 的 'Read/Write Enabled'！");
            return;
        }
        if (workingTexture != null) Destroy(workingTexture);
        workingTexture = new Texture2D(dustyPaperSprite.texture.width, dustyPaperSprite.texture.height, TextureFormat.RGBA32, false);
        workingTexture.SetPixels(dustyPaperSprite.texture.GetPixels());
        workingTexture.Apply();
        paperDisplay.texture = workingTexture;
        isCleaned = false;
    }

    // 绑定拖拽擦拭事件
    private void BindDragEvent()
    {
        EventTrigger eventTrigger = paperDisplay.GetComponent<EventTrigger>();
        if (eventTrigger == null) eventTrigger = paperDisplay.gameObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Clear();

        EventTrigger.Entry dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        dragEntry.callback.AddListener(new UnityAction<BaseEventData>(OnPaperDrag));
        eventTrigger.triggers.Add(dragEntry);
    }

    // 点击场景中的paper打开UI
    private void OnMouseDown()
    {
        if (!isUIOpen && EventSystem.current.currentSelectedGameObject == null)
        {
            paperUI.SetActive(true);
            isUIOpen = true;
            CalculateActualWipeRadius();
            if (isRevealed) paperDisplay.texture = cleanPaperSprite.texture;
        }
    }

    // 拖拽擦拭逻辑
    private void OnPaperDrag(BaseEventData eventData)
    {
        if (isCleaned || isRevealed || workingTexture == null) return;

        PointerEventData pointerData = eventData as PointerEventData;
        RectTransform rt = paperDisplay.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointerData.position, pointerData.pressEventCamera, out Vector2 localPos)) return;

        Vector2 normalizedPos = new Vector2(
            (localPos.x - rt.rect.xMin) / rt.rect.width,
            (localPos.y - rt.rect.yMin) / rt.rect.height
        );

        int pixelX = Mathf.Clamp((int)(normalizedPos.x * workingTexture.width), 0, workingTexture.width - 1);
        int pixelY = Mathf.Clamp((int)(normalizedPos.y * workingTexture.height), 0, workingTexture.height - 1);

        WipeAtPixel(pixelX, pixelY, Mathf.RoundToInt(actualWipeRadius));

        // 擦拭完成后触发销毁逻辑
        if (!isCleaned && CheckIfCleaned())
        {
            isCleaned = true;
            isRevealed = true;
            paperDisplay.texture = cleanPaperSprite.texture;
            Debug.Log($"【WipeablePaper】纸张擦拭完成！将在 {delayAfterClean} 秒后销毁UI和纸张物体。");

            // 启动协程：延迟后销毁
            StartCoroutine(DestroyAfterDelay(delayAfterClean));
        }
    }

    // 协程：延迟后销毁paperUI和场景中的paper物体
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 停留指定秒数

        // 销毁UI和场景中的纸张物体（this.gameObject 就是挂载脚本的paper物体）
        if (paperUI != null) Destroy(paperUI);
        Destroy(gameObject); // 销毁场景中可点击的纸张

        Debug.Log("【WipeablePaper】UI和纸张物体已销毁！");
    }

    // 像素擦拭逻辑
    private void WipeAtPixel(int centerX, int centerY, int radius)
    {
        Color[] cleanPixels = cleanPaperSprite.texture.GetPixels();
        int radiusSquared = radius * radius;
        for (int xOffset = -radius; xOffset <= radius; xOffset++)
        {
            for (int yOffset = -radius; yOffset <= radius; yOffset++)
            {
                if (xOffset * xOffset + yOffset * yOffset > radiusSquared) continue;
                int currentX = centerX + xOffset;
                int currentY = centerY + yOffset;
                if (currentX >= 0 && currentX < workingTexture.width && currentY >= 0 && currentY < workingTexture.height)
                {
                    int pixelIndex = currentY * workingTexture.width + currentX;
                    workingTexture.SetPixel(currentX, currentY, cleanPixels[pixelIndex]);
                }
            }
        }
        workingTexture.Apply();
    }

    // 检查是否擦干净
    private bool CheckIfCleaned()
    {
        Color[] workingPixels = workingTexture.GetPixels();
        Color[] cleanPixels = cleanPaperSprite.texture.GetPixels();
        int matched = 0;
        int step = Mathf.Max(1, workingPixels.Length / 1000);
        for (int i = 0; i < workingPixels.Length; i += step)
        {
            if (ColorDistance(workingPixels[i], cleanPixels[i]) < 0.01f) matched++;
        }
        return (float)matched / (workingPixels.Length / step) >= cleanThreshold;
    }

    // 计算颜色相似度
    private float ColorDistance(Color a, Color b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.r - b.r, 2) + Mathf.Pow(a.g - b.g, 2) + Mathf.Pow(a.b - b.b, 2) + Mathf.Pow(a.a - b.a, 2));
    }

    // 重置功能（测试用，物体销毁后无效）
    public void ResetPaper()
    {
        isCleaned = false;
        isRevealed = false;
        PrepareWorkingTexture();
    }
}