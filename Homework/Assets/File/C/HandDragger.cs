using UnityEngine;
using UnityEngine.EventSystems;

public class HandDragger : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public bool isHourHand; // 标记是否为时针（否则为分针）
    private Transform handTransform;
    // private Transform clockTransform; // 时钟表盘的transform（用于获取中心位置）
    public Transform customPivot;
    void Awake()
    {
        handTransform = transform;
        // 获取时钟表盘的transform（假设父对象是时钟表盘）
        // clockTransform = handTransform.parent;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 新增：校验自定义中心点是否配置，未配置则用默认父对象（兼容旧逻辑）
        if (customPivot == null)
        {
            customPivot = handTransform.parent;
            Debug.LogWarning("未配置自定义中心点，自动使用父对象作为中心点");
        }

        // 1. 计算鼠标相对于【自定义中心点】的角度（核心修改点）
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = customPivot.position.z; // 确保与中心点在同一平面（避免3D偏移）
                                                  // 从【自定义中心点】指向鼠标，而非原父对象
        Vector3 direction = mouseWorldPos - customPivot.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // 0度指向12点的逻辑不变

        // 后续角度循环、对齐刻度、应用旋转的逻辑保持不变...
        angle = Mathf.Repeat(angle, 360f);
        if (isHourHand)
            angle = Mathf.Round(angle / 30f) * 30f;
        else
            angle = Mathf.Round(angle / 6f) * 6f;

        handTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 拖动结束后验证时间（如果需要）
        if (ClockManager.Instance != null)
            ClockManager.Instance.CheckTime();
    }
}