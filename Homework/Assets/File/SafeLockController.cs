using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SafeLockController : MonoBehaviour
{
    [Header("【保险柜专属配置】")]
    public string safePassword; // 自定义密码（如1234、1234-）
    public bool autoCloseOnSuccess = true;
    public bool disableColliderAfterUnlock = true;

    [Header("奖励设置")]
    public string rewardItemId = "item_reward"; // 唯一ID：safe1_lens/safe2_solvent
    public Sprite rewardSprite;                // 拖入jingpian/rongjieji Sprite
    public Color placeholderColor = new Color(0.7f, 0.9f, 0.7f, 1f);

    // 全局UI引用（单例复用）
    public static GameObject GlobalLockPanel;
    public static Text GlobalDisplayText;
    private static SafeLockController _currentTargetSafe; // 当前交互的保险柜
    private string _currentInput = "";
    private const int MaxInputLength = 4; // 输入上限：4位数字

    private void Awake()
    {
        // 初始化全局UI（仅第一次加载时绑定）
        if (GlobalLockPanel == null)
        {
            GlobalLockPanel = GameObject.Find("Placeholder_SafePanel");
            GlobalDisplayText = GlobalLockPanel.GetComponentInChildren<Text>(true);
            BindUIButtons(); // 绑定按钮逻辑（仅执行一次）
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
        _currentInput = ""; // 打开时强制清空输入
        GlobalLockPanel.SetActive(true);
        UpdateDisplay(""); // 清空显示
    }

    // 绑定UI按钮（修复重复绑定+输入上限）
    private void BindUIButtons()
    {
        // 先清空所有按钮原有事件（防止重复绑定）
        foreach (var btn in GlobalLockPanel.GetComponentsInChildren<Button>(true))
        {
            btn.onClick.RemoveAllListeners();
        }

        // 重新绑定按钮逻辑
        foreach (var btn in GlobalLockPanel.GetComponentsInChildren<Button>(true))
        {
            // 数字按钮：Button1/Button2/Button3/Button4（支持任意数字）
            if (btn.name.StartsWith("Button"))
            {
                string digit = btn.name.Replace("Button", "");
                btn.onClick.AddListener(() => AppendSingleDigit(digit));
            }
            // 退出按钮（原Clear）
            else if (btn.name == "BtnClear")
            {
                btn.onClick.AddListener(() => GlobalLockPanel.SetActive(false));
            }
            // 提交按钮（OK）
            else if (btn.name == "BtnSubmit")
            {
                btn.onClick.AddListener(SubmitPassword);
            }
        }
    }

    // 核心：单次点击仅加1位数字 + 限制最多4位
    private void AppendSingleDigit(string digit)
    {
        if (_currentTargetSafe == null || GameData.IsItemCollected(_currentTargetSafe.rewardItemId)) return;

        // 输入长度未到4位时才允许输入
        if (_currentTargetSafe._currentInput.Length < MaxInputLength)
        {
            _currentTargetSafe._currentInput += digit; // 仅追加一次
            UpdateDisplay(_currentTargetSafe._currentInput);
        }
        // 超过4位时无反应（可加提示，可选）
        // else { UpdateDisplay("最多4位"); }
    }

    // 提交密码逻辑
    private void SubmitPassword()
    {
        if (_currentTargetSafe == null || GameData.IsItemCollected(_currentTargetSafe.rewardItemId)) return;

        // 对比自定义密码
        if (_currentTargetSafe._currentInput == _currentTargetSafe.safePassword)
        {
            _currentTargetSafe.OnUnlockSuccess();
        }
        else
        {
            UpdateDisplay("错误");
            _currentTargetSafe.StartCoroutine(_currentTargetSafe.ResetInputAfterDelay(0.8f));
        }
    }

    // 解锁成功：发放奖励
    private void OnUnlockSuccess()
    {
        GrantReward();
        if (autoCloseOnSuccess) GlobalLockPanel.SetActive(false);
        if (disableColliderAfterUnlock) DisableColliders();
        UpdateDisplay("解锁");
    }

    // 发放奖励
    private void GrantReward()
    {
        Sprite spriteToUse = rewardSprite;
        if (spriteToUse == null)
        {
            // 生成占位图（兜底）
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

    // 延迟清空输入（密码错误时）
    private IEnumerator ResetInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _currentInput = "";
        UpdateDisplay("");
    }

    // 更新UI显示
    private static void UpdateDisplay(string content)
    {
        if (GlobalDisplayText != null) GlobalDisplayText.text = content;
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