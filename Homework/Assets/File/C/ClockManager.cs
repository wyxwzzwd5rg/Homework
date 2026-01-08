using UnityEngine;
using UnityEngine.Video; // 引入原生VideoPlayer命名空间（新增）

public class ClockManager : MonoBehaviour
{
    public static ClockManager Instance;
    public Transform hourHand; // 拖入场景中的时针
    public Transform minuteHand; // 拖入场景中的分针
    public Transform customPivot;
    // public GameObject cuckooBird; // 拖入布谷鸟对象
    // public GameObject secretCompartment; // 拖入暗格对象
    // 替换：删除原VideoPlayer变量，新增MediaPlayer变量

    public GameObject videoCanvas; // 拖入VideoCanvas
    public VideoPlayer nativeVideoPlayer;  // 拖入VideoPlayerRawImage上的MediaPlayer组件
    public float correctHourAngle = 210f; // 9点对应的角度（从12点顺时针转270度）
    public float correctMinuteAngle = 330f; // 15分对应的角度（从12点顺时针转90度）
    private bool isSolved = false;
    public Camera clockV1Camera; // 拖入ClockV1Camera
    public GameObject originalClockCanvas; // 拖入原时钟画布（如ClockCanvas）

    public GameObject clockPivot; // 拖入控制时针分针中心点的空物体（ClockPivot）
    public GameObject originalBackground; // 拖入原背景图（IMG_7709，不在ClockCanvas下）
    public GameObject newBackground; // 拖入新的背景图（需要显示的新背景）
    public GameObject tanhuang; // 拖入ClockCanvas下的弹簧（MinuteHand tanhuang）
    public Sprite springSprite;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    // ClockManager.cs - HideSpringAndCollect 方法补充容错
    public void HideSpringAndCollect()
    {
        // 1. 隐藏时钟视角的弹簧（核心逻辑）
        if (tanhuang != null && !tanhuang.activeSelf)
        {
            Debug.LogWarning("[时钟弹簧] 弹簧已隐藏，无需重复收集");
            return;
        }
        if (tanhuang != null)
        {
            tanhuang.SetActive(false);
            Debug.Log("[时钟弹簧] 弹簧已隐藏");
        }
        else
        {
            Debug.LogError("[时钟弹簧] 弹簧对象（tanhuang）未在Inspector赋值！");
            return;
        }

        // 2. 收集弹簧到背包（补充：校验sprite和背包实例）
        if (springSprite == null)
        {
            Debug.LogError("[时钟弹簧] springSprite未赋值！请拖入弹簧的Sprite资源");
            return;
        }
        if (BackpackManager.Instance == null)
        {
            Debug.LogError("[时钟弹簧] 找不到BackpackManager实例！");
            return;
        }

        BackpackManager.Instance.CollectSpring(springSprite);
        Debug.Log($"[时钟弹簧] 弹簧({springSprite.name})已收集到背包");
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
            PlayClockVideo();
            // OpenSecretCompartment(); // 打开暗格
            // ShowCuckooBird(); // 弹出布谷鸟
        }
    }
    // 重写：适配MediaPlayer的播放方法
    private void PlayClockVideo()
    {
        // 校验所有必要对象是否配置（避免空引用错误）
        if (videoCanvas != null && nativeVideoPlayer != null
            && clockV1Camera != null && originalClockCanvas != null)
        {
            // 1. 切换相机：激活Video1Camera，关闭原时钟相机
            clockV1Camera.gameObject.SetActive(true);
            Camera originalCamera = originalClockCanvas.GetComponentInParent<Camera>(); // 获取原时钟相机（如ClockCamera）
            if (originalCamera != null)
            {
                originalCamera.gameObject.SetActive(false);
            }

            // 2. 切换画布：显示Video1Canvas，隐藏原时钟画布
            videoCanvas.SetActive(true);
            originalClockCanvas.SetActive(false);

            // 3. 播放视频（原生VideoPlayer方法）
            nativeVideoPlayer.Play();
            // 监听视频播放结束事件（新增）
            nativeVideoPlayer.loopPointReached += OnVideoFinished;
            Debug.Log("已激活Video1Camera并播放原生视频");
        }
        else
        {
            Debug.LogError("未配置视频/相机/画布对象，请检查ClockManager的Inspector赋值");
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // 1. 基础画布/相机切换：隐藏视频画布，恢复原ClockCanvas和相机
        videoCanvas.SetActive(false);
        originalClockCanvas.SetActive(true);
        clockV1Camera.gameObject.SetActive(false);
        Camera originalCamera = originalClockCanvas.GetComponentInParent<Camera>();
        if (originalCamera != null)
        {
            originalCamera.gameObject.SetActive(true);
        }

        // 2. 隐藏指定旧元素：时针、分针、ClockPivot、原背景图IMG_7709
        if (hourHand != null) hourHand.gameObject.SetActive(false); // 隐藏时针
        if (minuteHand != null) minuteHand.gameObject.SetActive(false); // 隐藏分针
        if (clockPivot != null) clockPivot.SetActive(false); // 隐藏中心点空物体
        if (originalBackground != null) originalBackground.SetActive(false); // 隐藏原背景图

        // 3. 显示新元素：新背景图 + ClockCanvas下的弹簧
        if (newBackground != null) newBackground.SetActive(true); // 显示新背景
        if (tanhuang != null) tanhuang.SetActive(true); // 显示弹簧

        // 4. 移除事件监听（避免重复触发）
        nativeVideoPlayer.loopPointReached -= OnVideoFinished;
        Debug.Log("视频播放结束，已恢复ClockCanvas并切换元素显隐");
    }
}
// 打开暗格（示例：向上移动暗格）
// void OpenSecretCompartment()
// {
//     secretCompartment.SetActive(false);
//     // LeanTween.moveY(secretCompartment, secretCompartment.transform.position.y + 50f, 1f); // 使用LeanTween实现平滑动画（需导入插件）
//     // 若无LeanTween，可直接设置位置：secretCompartment.transform.position += new Vector3(0, 50f, 0);
// }

// // 弹出布谷鸟（示例：向上移动布谷鸟）
// void ShowCuckooBird()
// {
//     Debug.Log("显示布谷鸟！");
//     // 找到布谷鸟的InteractableObject脚本，调用ShowCuckoo()
//     InteractableObject cuckoo = cuckooBird.GetComponent<InteractableObject>();
//     if (cuckoo != null)
//     {
//         cuckoo.ShowCuckoo(); // 只激活布谷鸟，弹簧仍隐藏
//     }
// }
