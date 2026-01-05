using UnityEngine;

public class ViewManager : MonoBehaviour
{
    // 单例模式，确保全局唯一
    public static ViewManager Instance;

    // 拖入两个特写视角的 Canvas
    public Canvas drawersCanvas;   // 柜子特写的 Canvas (DrawersCanvas)
    public Canvas clockCanvas;     // 时钟特写的 Canvas (ClockCanvas)
    public Canvas puzzleCanvas;
    public Canvas clockV1Canvas;

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
    public void ExitPuzzleView()
    {
        // 安全校验：避免空引用异常
        if (puzzleCanvas != null)
        {
            puzzleCanvas.gameObject.SetActive(false);
            Debug.Log("[ViewManager] 退出华容道视角：已隐藏PuzzleCanvas");
        }
        else
        {
            Debug.LogWarning("[ViewManager] PuzzleCanvas未赋值，请在Inspector面板绑定！");
        }

        // 可选：确保其他特写Canvas也保持隐藏（防止冲突）
        EnsureOtherCanvasHidden();
    }
    public void ExitClockView()
    {
        if (clockCanvas != null)
        {
            clockCanvas.gameObject.SetActive(false);
            Debug.Log("[ViewManager] 退出时钟视角：已隐藏ClockCanvas");
        }
        else
        {
            Debug.LogWarning("[ViewManager] ClockCanvas未赋值，请在Inspector面板绑定！");
        }

        EnsureOtherCanvasHidden();
    }
    public void ExitDrawersView()
    {
        if (drawersCanvas != null)
        {
            drawersCanvas.gameObject.SetActive(false);
            Debug.Log("[ViewManager] 退出柜子视角：已隐藏DrawersCanvas");
        }
        else
        {
            Debug.LogWarning("[ViewManager] DrawersCanvas未赋值，请在Inspector面板绑定！");
        }

        EnsureOtherCanvasHidden();
    }
    private void EnsureOtherCanvasHidden()
    {
        if (clockCanvas != null && clockCanvas.gameObject.activeSelf)
        {
            clockCanvas.gameObject.SetActive(false);
        }
        if (drawersCanvas != null && drawersCanvas.gameObject.activeSelf)
        {
            drawersCanvas.gameObject.SetActive(false);
        }
        if (clockV1Canvas != null && clockV1Canvas.gameObject.activeSelf)
        {
            clockV1Canvas.gameObject.SetActive(false);
        }
    }
    // 进入柜子特写视角：激活 DrawersCanvas，禁用 ClockCanvas
    public void EnterCabinetView()
    {
        Debug.LogError("EnterCabinetView被调用了！");
        drawersCanvas.gameObject.SetActive(true);
        clockCanvas.gameObject.SetActive(false);
        puzzleCanvas.gameObject.SetActive(false);
        clockV1Canvas.gameObject.SetActive(false);
        Debug.Log("进入柜子特写：DrawersCanvas 已激活，其他已禁用");
    }

    // 进入时钟特写视角：激活 ClockCanvas，禁用 DrawersCanvas
    public void EnterClockView()
    {
        Debug.LogError("EnterClockView被调用了！");
        clockCanvas.gameObject.SetActive(true);
        drawersCanvas.gameObject.SetActive(false);
        puzzleCanvas.gameObject.SetActive(false);
        clockV1Canvas.gameObject.SetActive(false);
        Debug.Log("进入时钟特写：ClockCanvas 已激活，其他已禁用");
    }
    // 进入华容道特写视角：激活 PuzzleCanvas，禁用 DrawersCanvas/ClockCanvas
    public void EnterPuzzleView()
    {
        Debug.LogError("EnterPuzzleView被调用了！");
        puzzleCanvas.gameObject.SetActive(true);
        clockCanvas.gameObject.SetActive(false);
        drawersCanvas.gameObject.SetActive(false);
        clockV1Canvas.gameObject.SetActive(false);
        Debug.Log("进入华容道特写：PuzzleCanvas 已激活，其他已禁用");
    }
    // 进入V1特写视角：激活 PuzzleCanvas，禁用 DrawersCanvas/ClockCanvas
    public void EnterClockV1View()
    {
        Debug.LogError("Enterv1View被调用了！");
        clockV1Canvas.gameObject.SetActive(true);
        clockCanvas.gameObject.SetActive(false);
        drawersCanvas.gameObject.SetActive(false);
        puzzleCanvas.gameObject.SetActive(false);
        Debug.Log("进入Video1特写：clockV1Canvas 已激活，其他已禁用");
    }
    // 退出特写视角：禁用所有特写 Canvas
    public void ExitCloseUpView()
    {
        drawersCanvas.gameObject.SetActive(false);
        clockCanvas.gameObject.SetActive(false);
        clockV1Canvas.gameObject.SetActive(false);
        puzzleCanvas.gameObject.SetActive(false);
        Debug.Log("退出特写视角：所有 Canvas 已禁用");
    }
}