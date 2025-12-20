using UnityEngine;
using RenderHeads.Media.AVProVideo;
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
    public MediaPlayer mediaPlayer; // 拖入VideoPlayerRawImage上的MediaPlayer组件
    public float correctHourAngle = 210f; // 9点对应的角度（从12点顺时针转270度）
    public float correctMinuteAngle = 330f; // 15分对应的角度（从12点顺时针转90度）
    private bool isSolved = false;
    public Camera clockV1Camera; // 拖入ClockV1Camera
    public GameObject originalClockCanvas; // 拖入原时钟画布（如ClockCanvas）
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
            PlayClockVideo();
            // OpenSecretCompartment(); // 打开暗格
            // ShowCuckooBird(); // 弹出布谷鸟
        }
    }
    // 重写：适配MediaPlayer的播放方法
    private void PlayClockVideo()
    {
        if (videoCanvas != null && mediaPlayer != null
            && clockV1Camera != null && originalClockCanvas != null)
        {
            // 1. 激活ClockV1Camera，关闭原时钟相机（如ClockCamera）
            clockV1Camera.gameObject.SetActive(true);
            Camera originalCamera = originalClockCanvas.GetComponentInParent<Camera>(); // 原时钟对应的相机
            if (originalCamera != null)
            {
                originalCamera.gameObject.SetActive(false);
            }

            // 2. 显示ClockV1Canvas，隐藏原时钟画布
            videoCanvas.SetActive(true);
            originalClockCanvas.SetActive(false);

            // 3. 播放视频
            mediaPlayer.Play();
            mediaPlayer.Events.AddListener(OnVideoEvent);
            Debug.Log("已激活ClockV1Camera并播放视频");
        }
        else
        {
            Debug.LogError("未配置视频/相机/画布对象，请检查ClockManager的Inspector赋值");
        }
    }

    // 新增：AVPro视频事件回调（处理播放结束）
    private void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
    {
        if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
        {
            // 1. 隐藏ClockV1Canvas，恢复原时钟画布
            videoCanvas.SetActive(false);
            originalClockCanvas.SetActive(true);

            // 2. 关闭ClockV1Camera，恢复原时钟相机
            clockV1Camera.gameObject.SetActive(false);
            Camera originalCamera = originalClockCanvas.GetComponentInParent<Camera>();
            if (originalCamera != null)
            {
                originalCamera.gameObject.SetActive(true);
            }

            mp.Events.RemoveListener(OnVideoEvent);
            Debug.Log("视频播放结束，恢复原时钟界面");
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
}