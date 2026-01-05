using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class WipeablePaper : MonoBehaviour
{
    [Header("ֽ����Դ")]
    public Sprite dustyPaperSprite; // ���ҳ���ֽ�������пɵ����ֽ��Sprite��
    public Sprite cleanPaperSprite;  // �ɾ���ֽ��UI����ʾ��������

    [Header("UI ����")]
    public GameObject paperUI;       // ������壨�����٣�
    public RawImage paperDisplay;    // ��ʾֽ�ŵ�RawImage

    [Header("��������")]
    public int baseWipeRadius = 1500;  // 已增大涂抹范围，涂抹更快  // �������ð뾶
    public float cleanThreshold = 0.8f; // ������ֵ

    [Header("��������")]
    public float delayAfterClean = 2.0f; // ������ɺ�ͣ��ʱ�䣨�룩

    private Texture2D workingTexture;
    private bool isCleaned = false;
    private bool isRevealed = false;
    private bool isUIOpen = false;
    private float actualWipeRadius = 250f;  // 初始值（当前的一半）

    void Start()
    {
        if (!CheckReferences()) return;
        paperUI.SetActive(false);
        if (!isRevealed) PrepareWorkingTexture();
        BindDragEvent();
    }

    // UI����ʱ������ð뾶
    void OnEnable()
    {
        if (paperDisplay != null && paperUI.activeSelf)
        {
            CalculateActualWipeRadius();
        }
    }

    // ��������UI��ʵ�ʲ��ð뾶
    private void CalculateActualWipeRadius()
    {
        // 简化逻辑：直接基于纹理尺寸的百分比计算，确保涂抹范围足够大
        if (workingTexture != null)
        {
            // 使用纹理宽度的12.5%作为涂抹半径（当前的一半）
            actualWipeRadius = Mathf.Max(workingTexture.width * 0.125f, workingTexture.height * 0.125f, 250f);
            Debug.Log($"[纸张涂抹] 涂抹半径已设置为：{actualWipeRadius}（纹理尺寸：{workingTexture.width}x{workingTexture.height}）");
        }
        else if (dustyPaperSprite != null && dustyPaperSprite.texture != null)
        {
            // 如果workingTexture还没准备好，使用原始sprite的尺寸
            actualWipeRadius = Mathf.Max(dustyPaperSprite.texture.width * 0.125f, dustyPaperSprite.texture.height * 0.125f, 250f);
            Debug.Log($"[纸张涂抹] 涂抹半径已设置为：{actualWipeRadius}（Sprite尺寸：{dustyPaperSprite.texture.width}x{dustyPaperSprite.texture.height}）");
        }
        else
        {
            // 最后的保底值
            actualWipeRadius = 400f;
            Debug.Log($"[纸张涂抹] 使用默认涂抹半径：{actualWipeRadius}");
        }
    }

    // ����Ҫ����
    private bool CheckReferences()
    {
        if (dustyPaperSprite == null || cleanPaperSprite == null || paperUI == null || paperDisplay == null)
        {
            Debug.LogError("��WipeablePaper����Ϊ���й���������ֵ��");
            enabled = false;
            return false;
        }
        if (dustyPaperSprite.texture.width != cleanPaperSprite.texture.width || dustyPaperSprite.texture.height != cleanPaperSprite.texture.height)
        {
            Debug.LogError("��WipeablePaper�����ҳ���ֽ�͸ɾ���ֽ�����ߴ����һ�£�");
            enabled = false;
            return false;
        }
        return true;
    }

    // ׼������������
    private void PrepareWorkingTexture()
    {
        if (!dustyPaperSprite.texture.isReadable)
        {
            Debug.LogError($"��WipeablePaper���빴ѡ {dustyPaperSprite.name} �� 'Read/Write Enabled'��");
            return;
        }
        if (workingTexture != null) Destroy(workingTexture);
        workingTexture = new Texture2D(dustyPaperSprite.texture.width, dustyPaperSprite.texture.height, TextureFormat.RGBA32, false);
        workingTexture.SetPixels(dustyPaperSprite.texture.GetPixels());
        workingTexture.Apply();
        paperDisplay.texture = workingTexture;
        isCleaned = false;
    }

    // ����ק�����¼�
    private void BindDragEvent()
    {
        EventTrigger eventTrigger = paperDisplay.GetComponent<EventTrigger>();
        if (eventTrigger == null) eventTrigger = paperDisplay.gameObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Clear();

        EventTrigger.Entry dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        dragEntry.callback.AddListener(new UnityAction<BaseEventData>(OnPaperDrag));
        eventTrigger.triggers.Add(dragEntry);
    }

    // ��������е�paper��UI
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

    // ��ק�����߼�
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

        // ������ɺ󴥷������߼�
        if (!isCleaned && CheckIfCleaned())
        {
            isCleaned = true;
            isRevealed = true;
            paperDisplay.texture = cleanPaperSprite.texture;
            Debug.Log($"��WipeablePaper��ֽ�Ų�����ɣ����� {delayAfterClean} �������UI��ֽ�����塣");

            // ����Э�̣��ӳٺ�����
            StartCoroutine(DestroyAfterDelay(delayAfterClean));
        }
    }

    // Э�̣��ӳٺ�����paperUI�ͳ����е�paper����
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // ͣ��ָ������

        // ����UI�ͳ����е�ֽ�����壨this.gameObject ���ǹ��ؽű���paper���壩
        if (paperUI != null) Destroy(paperUI);
        Destroy(gameObject); // ���ٳ����пɵ����ֽ��

        Debug.Log("��WipeablePaper��UI��ֽ�����������٣�");
    }

    // ���ز����߼�
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

    // ����Ƿ���ɾ�
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

    // ������ɫ���ƶ�
    private float ColorDistance(Color a, Color b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.r - b.r, 2) + Mathf.Pow(a.g - b.g, 2) + Mathf.Pow(a.b - b.b, 2) + Mathf.Pow(a.a - b.a, 2));
    }

    // ���ù��ܣ������ã��������ٺ���Ч��
    public void ResetPaper()
    {
        isCleaned = false;
        isRevealed = false;
        PrepareWorkingTexture();
    }
}