using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SafeLockController : MonoBehaviour
{
    [Header("密码设置")]
    public string correctCode = "1234";
    public bool autoCloseOnSuccess = true;
    public bool disableColliderAfterUnlock = true;

    [Header("奖励设置")]
    public string rewardItemId = "item_reward";
    public string rewardSpriteName = "reward";
    public Sprite rewardSprite;
    public Color placeholderColor = new Color(0.7f, 0.9f, 0.7f, 1f);

    // 全局唯一的密码UI面板（拖拽场景中唯一的Placeholder_SafePanel）
    public static GameObject GlobalLockPanel;
    public static Text GlobalDisplayText;
    private static SafeLockController _currentTargetSafe; // 当前交互的保险柜实例

    private string _currentInput = "";


    private void Awake()
    {
        // 初始化全局UI引用（仅在第一个保险柜加载时执行）
        if (GlobalLockPanel == null)
        {
            GlobalLockPanel = GameObject.Find("Placeholder_SafePanel");
            GlobalDisplayText = GlobalLockPanel.GetComponentInChildren<Text>(true);
            // 给UI按钮绑定全局方法
            BindUIButtons();
        }
        // 初始化时关闭UI
        GlobalLockPanel?.SetActive(false);
    }

    private void Start()
    {
        if (GameData.IsItemCollected(rewardItemId))
        {
            DisableColliders();
        }
    }

    // 点击保险柜时，将当前保险柜设为UI的目标
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

        // 绑定当前保险柜为UI的目标
        _currentTargetSafe = this;
        GlobalLockPanel.SetActive(true);
        ResetInput();
    }

    // 给全局UI按钮绑定逻辑（数字、清空、提交）
    private void BindUIButtons()
    {
        // 绑定数字按钮（假设按钮名称是Btn1、Btn2...）
        foreach (var btn in GlobalLockPanel.GetComponentsInChildren<Button>(true))
        {
            if (btn.name.StartsWith("Btn"))
            {
                string digit = btn.name.Replace("Btn", "");
                btn.onClick.AddListener(() => AppendDigit(digit));
            }
            else if (btn.name == "BtnClear")
            {
                btn.onClick.AddListener(ClearInput);
            }
            else if (btn.name == "BtnSubmit")
            {
                btn.onClick.AddListener(SubmitCode);
            }
        }
    }

    // 以下方法改为“操作当前目标保险柜”
    public static void AppendDigit(string digit)
    {
        if (_currentTargetSafe == null || GameData.IsItemCollected(_currentTargetSafe.rewardItemId)) return;
        _currentTargetSafe._currentInput += digit;
        UpdateDisplay(_currentTargetSafe._currentInput);
    }

    public static void ClearInput()
    {
        if (_currentTargetSafe == null) return;
        _currentTargetSafe._currentInput = "";
        UpdateDisplay("");
    }

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

    private void OnUnlockSuccess()
    {
        GrantReward();
        if (autoCloseOnSuccess) GlobalLockPanel.SetActive(false);
        if (disableColliderAfterUnlock) DisableColliders();
        UpdateDisplay("解锁");
    }

    private void GrantReward()
    {
        Sprite spriteToUse = EnsureRewardSprite();
        if (BackpackManager.Instance != null)
        {
            BackpackManager.Instance.CollectItem(spriteToUse);
            GameData.AddCollectedItem(rewardItemId);
        }
    }

    private Sprite EnsureRewardSprite()
    {
        if (rewardSprite != null)
        {
            rewardSprite.name = string.IsNullOrEmpty(rewardSpriteName) ? rewardSprite.name : rewardSpriteName;
            return rewardSprite;
        }
        // 生成占位Sprite（同之前逻辑）
        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++) colors[i] = placeholderColor;
        tex.SetPixels(colors);
        tex.Apply();
        Sprite generated = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        generated.name = string.IsNullOrEmpty(rewardSpriteName) ? "placeholder_reward" : rewardSpriteName;
        return generated;
    }

    private void ResetInput()
    {
        _currentInput = "";
        UpdateDisplay("");
    }

    private static void UpdateDisplay(string content)
    {
        GlobalDisplayText.text = content;
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetInput();
    }

    private void DisableColliders()
    {
        Collider col3D = GetComponent<Collider>();
        if (col3D != null) col3D.enabled = false;
        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null) col2D.enabled = false;
    }
}