using UnityEngine;
using UnityEngine.EventSystems;

public class HandDragger : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public bool isHourHand; // 标记是否为时针（否则为分针）
    private Transform handTransform;
    private Transform clockTransform; // 时钟表盘的transform（用于获取中心位置）

    void Awake()
    {
        handTransform = transform;
        // 获取时钟表盘的transform（假设父对象是时钟表盘）
        clockTransform = handTransform.parent;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 1. 计算鼠标相对于时钟表盘中心的角度
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = clockTransform.position.z; // 确保在同一平面
        Vector3 direction = mouseWorldPos - clockTransform.position; // 从表盘中心指向鼠标
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // 调整0度指向12点

        // 2. 处理角度循环（确保角度在0°~360°范围内，避免-180°~180°的显示问题）
        angle = Mathf.Repeat(angle, 360f);

        // 3. 对齐到刻度（时针30°/格，分针6°/格）
        if (isHourHand)
            angle = Mathf.Round(angle / 30f) * 30f; // 时针每小时30度
        else
            angle = Mathf.Round(angle / 6f) * 6f; // 分针每分钟6度

        // 4. 应用旋转（使用localEulerAngles，确保绕表盘中心旋转）
        handTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 拖动结束后验证时间（如果需要）
        if (ClockManager.Instance != null)
            ClockManager.Instance.CheckTime();
    }
}