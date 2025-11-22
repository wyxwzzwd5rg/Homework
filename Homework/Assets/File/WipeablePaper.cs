using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(EventTrigger))]
public class WipeablePaper : MonoBehaviour
{
    [Header("纸张资源")]
    public Sprite dustyPaperSprite; // 带灰尘的纸（Sprite）
    public Sprite cleanPaperSprite;  // 干净的纸（含数字，Sprite）

    [Header("UI 引用")]
    public GameObject paperUI;       // 弹窗面板（Canvas下的Panel）
    public RawImage paperDisplay;    // 显示纸张的RawImage（paperUI的子物体）

    private Texture2D workingTexture;
    private bool isCleaned = false;
    private EventTrigger eventTrigger;

    void Start()
    {
        if (!CheckReferences()) return;
        paperUI.SetActive(false);
        PrepareWorkingTexture();
        BindDragEvent();
    }

    private bool CheckReferences()
    {
        if (dustyPaperSprite == null || cleanPaperSprite == null || paperUI == null || paperDisplay == null)
        {
            Debug.LogError("【WipeablePaper】请为所有公共变量赋值！");
            enabled = false;
            return false;
        }
        return true;
    }

    private void PrepareWorkingTexture()
    {
        if (!dustyPaperSprite.texture.isReadable)
        {
            Debug.LogError($"【WipeablePaper】请勾选 {dustyPaperSprite.name} 的 Read/Write Enabled！");
            return;
        }
        workingTexture = new Texture2D(dustyPaperSprite.texture.width, dustyPaperSprite.texture.height, TextureFormat.RGBA32, false);
        workingTexture.SetPixels(dustyPaperSprite.texture.GetPixels());
        workingTexture.Apply();
        paperDisplay.texture = workingTexture;
    }

    private void BindDragEvent()
    {
        eventTrigger = paperDisplay.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = paperDisplay.gameObject.AddComponent<EventTrigger>();
        }
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Drag
        };
        entry.callback.AddListener(new UnityAction<BaseEventData>(OnPaperDrag));
        eventTrigger.triggers.Add(entry);
    }

    private void OnMouseDown()
    {
        if (!isCleaned && !paperUI.activeSelf)
        {
            paperUI.SetActive(true);
        }
    }

    public void ClosePaperUI()
    {
        paperUI.SetActive(false);
    }

    public void OnPaperDrag(BaseEventData eventData)
    {
        if (isCleaned || workingTexture == null) return;
        PointerEventData pointerData = eventData as PointerEventData;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(paperDisplay.rectTransform, pointerData.position, pointerData.pressEventCamera, out Vector2 localPos)) return;

        Rect rect = paperDisplay.rectTransform.rect;
        int pixelX = Mathf.Clamp((int)((localPos.x - rect.xMin) * (workingTexture.width / rect.width)), 0, workingTexture.width - 1);
        int pixelY = Mathf.Clamp((int)((localPos.y - rect.yMin) * (workingTexture.height / rect.height)), 0, workingTexture.height - 1);

        WipeAtPixel(pixelX, pixelY, 15);
        if (!isCleaned && CheckIfCleaned())
        {
            isCleaned = true;
            Debug.Log("【WipeablePaper】纸张擦拭完成！");
        }
    }

    private void WipeAtPixel(int centerX, int centerY, int radius)
    {
        Color[] cleanPixels = cleanPaperSprite.texture.GetPixels();
        for (int x = -radius; x < radius; x++)
        {
            for (int y = -radius; y < radius; y++)
            {
                int currentX = centerX + x;
                int currentY = centerY + y;
                if (currentX >= 0 && currentX < workingTexture.width && currentY >= 0 && currentY < workingTexture.height)
                {
                    workingTexture.SetPixel(currentX, currentY, cleanPixels[currentY * workingTexture.width + currentX]);
                }
            }
        }
        workingTexture.Apply();
    }

    private bool CheckIfCleaned()
    {
        Color[] workingPixels = workingTexture.GetPixels();
        Color[] cleanPixels = cleanPaperSprite.texture.GetPixels();
        int matched = 0;
        int step = Mathf.Max(1, workingPixels.Length / 500);
        for (int i = 0; i < workingPixels.Length; i += step)
        {
            if (ColorDistance(workingPixels[i], cleanPixels[i]) < 0.1f) matched++;
        }
        return (float)matched / (workingPixels.Length / step) > 0.8f;
    }

    private float ColorDistance(Color a, Color b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.r - b.r, 2) + Mathf.Pow(a.g - b.g, 2) + Mathf.Pow(a.b - b.b, 2) + Mathf.Pow(a.a - b.a, 2));
    }
}