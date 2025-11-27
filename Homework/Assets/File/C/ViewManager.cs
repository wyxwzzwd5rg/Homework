using UnityEngine;

public class ViewManager : MonoBehaviour
{
    // 单例模式，确保全局唯一
    public static ViewManager Instance;

    // 拖入两个特写视角的 Canvas
    public Canvas drawersCanvas;   // 柜子特写的 Canvas (DrawersCanvas)
    public Canvas clockCanvas;     // 时钟特写的 Canvas (ClockCanvas)

    // void Start()
    // {
    //     // 临时测试代码，在游戏开始5秒后执行
    //     Invoke("TestEnterCabinetView", 5f);
    //     Invoke("TestEnterClockView", 10f);
    // }

    // // 测试进入柜子视角
    // void TestEnterCabinetView()
    // {
    //     Debug.LogError("### 临时测试：调用 EnterCabinetView ###");
    //     EnterCabinetView();
    // }

    // // 测试进入时钟视角
    // void TestEnterClockView()
    // {
    //     Debug.LogError("### 临时测试：调用 EnterClockView ###");
    //     EnterClockView();
    // }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 进入柜子特写视角：激活 DrawersCanvas，禁用 ClockCanvas
    public void EnterCabinetView()
    {
        Debug.LogError("EnterCabinetView被调用了！");
        drawersCanvas.gameObject.SetActive(true);
        clockCanvas.gameObject.SetActive(false);
        Debug.Log("进入柜子特写：DrawersCanvas 已激活，ClockCanvas 已禁用");
    }

    // 进入时钟特写视角：激活 ClockCanvas，禁用 DrawersCanvas
    public void EnterClockView()
    {
        Debug.LogError("EnterClockView被调用了！");
        clockCanvas.gameObject.SetActive(true);
        drawersCanvas.gameObject.SetActive(false);
        Debug.Log("进入时钟特写：ClockCanvas 已激活，DrawersCanvas 已禁用");
    }

    // 退出特写视角：禁用所有特写 Canvas
    public void ExitCloseUpView()
    {
        drawersCanvas.gameObject.SetActive(false);
        clockCanvas.gameObject.SetActive(false);
        Debug.Log("退出特写视角：所有 Canvas 已禁用");
    }
}