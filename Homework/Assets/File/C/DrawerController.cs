using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DrawerController : MonoBehaviour
{
    [Header("图片设置")]
    public Sprite closedSprite;  // 关闭状态图片
    public Sprite openSprite;    // 打开状态图片

    [Header("道具设置")]
    public GameObject propInDrawer; // 抽屉内的道具（拖入Unity编辑器）

    [Header("动画设置")]
    public float moveDistance = 80f;  // 打开时移动的距离（Y轴负方向为向下）
    public float animDuration = 0.3f; // 动画持续时间
    public float scaleFactor = 0.2f;  // 图片缩放因子（0.1~0.5为宜）

    private bool isOpen = false;
    private Image drawerImage;
    private Vector3 closedPosition;
    private Vector2 closedSize;
    private Vector2 openSize;

    void Start()
    {
        // 获取组件和初始状态
        drawerImage = GetComponent<Image>();
        closedPosition = transform.localPosition;

        // 初始化图片尺寸（缩放后）
        if (closedSprite != null)
        {
            closedSize = new Vector2(
                closedSprite.rect.width * scaleFactor,
                closedSprite.rect.height * scaleFactor
            );
        }
        if (openSprite != null)
        {
            openSize = new Vector2(
                openSprite.rect.width * scaleFactor,
                openSprite.rect.height * scaleFactor
            );
        }

        // 设置初始状态
        drawerImage.sprite = closedSprite;
        drawerImage.rectTransform.sizeDelta = closedSize;
    }

    // 点击事件触发
    public void ToggleDrawer()
    {
        isOpen = !isOpen;

        // 切换图片和尺寸
        drawerImage.sprite = isOpen ? openSprite : closedSprite;
        drawerImage.rectTransform.sizeDelta = isOpen ? openSize : closedSize;

        if (propInDrawer != null)
        {
            if (isOpen)
            {
                Debug.Log("抽屉打开，尝试显示道具: " + propInDrawer.name); // <--- 添加这行
                // 【修改点】：当抽屉打开时，先检查物品是否被收集，再决定是否显示
                ItemClickHandler itemHandler = propInDrawer.GetComponent<ItemClickHandler>();
                if (itemHandler != null)
                {
                    itemHandler.CheckAndSetActive();
                }
                else
                {
                    // 如果没有ItemClickHandler脚本，直接显示
                    propInDrawer.SetActive(true);
                }
            }
            else
            {
                // 抽屉关闭时，直接隐藏道具
                propInDrawer.SetActive(false);
            }

        }

        // 计算目标位置（移动方向：向下）
        Vector3 targetPos = closedPosition;
        if (isOpen)
        {
            targetPos += new Vector3(0, -moveDistance, 0);
        }

        // 播放位移动画
        StopCoroutine("PlayMoveAnimation");
        StartCoroutine("PlayMoveAnimation", targetPos);
    }

    // 平滑移动动画
    IEnumerator PlayMoveAnimation(Vector3 targetPos)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;

        while (elapsed < animDuration)
        {
            // 缓动函数：先慢后快再慢（更自然）
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 确保精确到达目标位置
        transform.localPosition = targetPos;
    }
}