using UnityEngine;

public class ClockManager : MonoBehaviour
{
    public static ClockManager Instance;
    public Transform hourHand; // 拖入场景中的时针
    public Transform minuteHand; // 拖入场景中的分针
    public Transform customPivot;
    public GameObject cuckooBird; // 拖入布谷鸟对象
    public GameObject secretCompartment; // 拖入暗格对象
    public float correctHourAngle = -150f; // 9点对应的角度（从12点顺时针转270度）
    public float correctMinuteAngle = -30f; // 15分对应的角度（从12点顺时针转90度）
    private bool isSolved = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 验证当前时间是否为9:15
    public void CheckTime()
    {
        if (isSolved) return;

        // 获取当前指针角度（取0~360度范围内的值）
        float currentHourAngle = Mathf.Repeat(hourHand.localEulerAngles.z, 360f);
        float currentMinuteAngle = Mathf.Repeat(minuteHand.localEulerAngles.z, 360f);
        Debug.Log("当前时针角度：" + currentHourAngle + "，当前分针角度：" + currentMinuteAngle);
        // 允许微小误差（1度内）
        bool isHourCorrect = Mathf.Abs(currentHourAngle - correctHourAngle) < 1f;
        bool isMinuteCorrect = Mathf.Abs(currentMinuteAngle - correctMinuteAngle) < 1f;

        if (isHourCorrect && isMinuteCorrect)
        {
            isSolved = true;
            OpenSecretCompartment(); // 打开暗格
            ShowCuckooBird(); // 弹出布谷鸟
        }
    }

    // 打开暗格（示例：向上移动暗格）
    void OpenSecretCompartment()
    {
        secretCompartment.SetActive(false);
        // LeanTween.moveY(secretCompartment, secretCompartment.transform.position.y + 50f, 1f); // 使用LeanTween实现平滑动画（需导入插件）
        // 若无LeanTween，可直接设置位置：secretCompartment.transform.position += new Vector3(0, 50f, 0);
    }

    // 弹出布谷鸟（示例：向上移动布谷鸟）
    void ShowCuckooBird()
    {
        Debug.Log("显示布谷鸟！");
        // 找到布谷鸟的InteractableObject脚本，调用ShowCuckoo()
        InteractableObject cuckoo = cuckooBird.GetComponent<InteractableObject>();
        if (cuckoo != null)
        {
            cuckoo.ShowCuckoo(); // 只激活布谷鸟，弹簧仍隐藏
        }
    }
}