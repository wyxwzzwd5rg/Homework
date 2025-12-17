using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SafeLockController : MonoBehaviour
{
    [Header("密码设置")]
    public string correctCode = "1234";
    public bool autoCloseOnSuccess = true;
    public bool disableColliderAfterUnlock = true;

    [Header("奖励设置【关键：按实际Sprite名称填】")]
    public string rewardItemId = "item_reward"; // 每个保险箱唯一ID
    public Sprite rewardSprite;    // 直接拖入场景中的jingpian/rongjieji Sprite
    public Color placeholderColor = new Color(0.7f, 0.9f, 0.7f, 1f);

    // 全局唯一UI引用（拖拽场景中的Placeholder_SafePanel）
    public static GameObject GlobalLockPanel;
    public static Text GlobalDisplayText;
    private static SafeLockController _currentTargetSafe; // 当前交互的保险柜
    private string _currentInput = "";

    private void Awake()
    {
        // 初始化全局UI（仅第一次加载时执行）
        if (GlobalLockPanel == null)
        {
            GlobalLockPanel = GameObject.Find("Placeholder_SafePanel");
            GlobalDisplayText = GlobalLockPanel.GetComponentInChildren<Text>(true);
            BindUIButtons(); // 绑定按钮逻辑
        }
        GlobalLockPanel?.SetActive(false); // 默认关闭UI
    }

    private void Start()
    {
        // 已领奖则禁用碰撞体
        if (GameData.IsItemCollected(rewardItemId))
        {
            DisableColliders();
        }
    }

    // 点击保险柜绑定当前目标
    private void OnMouseDown()
    {
        OpenLockUI();
    }

    public void OpenLockUI()
    {
        if (GameData.IsItemCollected(rewardItemId))
        {
            Debug.Log($"保险箱[{rewardItemId}]奖励已领取，无法打开");
            return;
        }
        _currentTargetSafe = this;
        GlobalLockPanel.SetActive(true);
        ResetInput();
    }

    // 绑定UI按钮（数字/清空/提交）
    private void BindUIButtons()
    {
        foreach (var btn in GlobalLockPanel.GetComponentsInChildren<Button>(true))
        {
            if (btn.name.StartsWith("Btn")) // 数字按钮：Btn1/Btn2/Btn3/Btn4
            {
                string digit = btn.name.Replace("Btn", "");
                btn.onClick.AddListener(() => AppendDigit(digit));
            }
            else if (btn.name == "BtnClear") // 清空按钮
            {
                btn.onClick.AddListener(ClearInput);
            }
            else if (btn.name == "BtnSubmit") // 提交按钮
            {
                btn.onClick.AddListener(SubmitCode);
            }
        }
    }

    // 输入数字（全局静态方法）
    public static void AppendDigit(string digit)
    {
        if (_currentTargetSafe == null || GameData.IsItemCollected(_currentTargetSafe.rewardItemId)) return;
        _currentTargetSafe._currentInput += digit;
        UpdateDisplay(_currentTargetSafe._currentInput);
    }

    // 清空输入（全局静态方法）
    public static void ClearInput()
    {
        if (_currentTargetSafe == null) return;
        _currentTargetSafe._currentInput = "";
        UpdateDisplay("");
    }

    // 提交密码（全局静态方法）
    public static void SubmitCode()
    {
        if (_currentTargetSafe == null || GameData.IsItemCollected(_currentTargetSafe.rewardItemId)) return;

        if (_currentTargetSafe._currentInput == _currentTargetSafe.correctCode)
        {
            _currentTargetSafe.OnUnlockSuccess();
        }
        else
        {
            UpdateDisplay("错误");
            _currentTargetSafe.StartCoroutine(_currentTargetSafe.ResetAfterDelay(0.8f));
        }
    }

    // 解锁成功：发放对应奖励
    private void OnUnlockSuccess()
    {
        GrantReward();
        if (autoCloseOnSuccess) GlobalLockPanel.SetActive(false);
        if (disableColliderAfterUnlock) DisableColliders();
        UpdateDisplay("解锁");
    }

    // 发放奖励【核心修正：直接使用配置的Sprite】
    private void GrantReward()
    {
        // 优先使用配置的Sprite（jingpian/rongjieji）
        Sprite spriteToUse = rewardSprite;

        // 兜底：无Sprite时生成占位图
        if (spriteToUse == null)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] colors = new Color[64 * 64];
            for (int i = 0; i < colors.Length; i++) colors[i] = placeholderColor;
            tex.SetPixels(colors);
            tex.Apply();
            spriteToUse = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            spriteToUse.name = $"placeholder_{rewardItemId}";
            Debug.LogWarning($"未配置奖励Sprite，生成占位图：{spriteToUse.name}");
        }

        // 发放到背包
        if (BackpackManager.Instance != null)
        {
            BackpackManager.Instance.CollectItem(spriteToUse);
            GameData.AddCollectedItem(rewardItemId);
            Debug.Log($"奖励发放成功：{spriteToUse.name}（ID：{rewardItemId}）");
        }
        else
        {
            Debug.LogError("BackpackManager实例不存在，无法发放奖励");
        }
    }

    // 重置输入
    private void ResetInput()
    {
        _currentInput = "";
        UpdateDisplay("");
    }

    // 更新UI显示
    private static void UpdateDisplay(string content)
    {
        if (GlobalDisplayText != null) GlobalDisplayText.text = content;
    }

    // 错误后延迟重置
    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetInput();
    }

    // 禁用碰撞体（领奖后无法点击）
    private void DisableColliders()
    {
        Collider col3D = GetComponent<Collider>();
        if (col3D != null) col3D.enabled = false;
        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null) col2D.enabled = false;
    }
}