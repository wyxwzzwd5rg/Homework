using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用密码锁控制器：点击打开UI，输入密码后发放奖励道具。
/// 适配场景A两个保险柜：设置 correctCode 与 rewardSprite/Id 即可。
/// </summary>
public class SafeLockController : MonoBehaviour
{
    [Header("密码设置")]
    public string correctCode = "1234";
    public bool autoCloseOnSuccess = true;
    public bool disableColliderAfterUnlock = true;

    [Header("UI引用")]
    public GameObject lockPanel;   // 包含输入UI的面板
    public Text displayText;       // 显示当前输入/提示

    [Header("奖励设置")]
    public string rewardItemId = "item_reward";
    public string rewardSpriteName = "reward";
    public Sprite rewardSprite;    // 若为空将生成占位素材
    public Color placeholderColor = new Color(0.7f, 0.9f, 0.7f, 1f);

    private string _currentInput = "";
    private bool _isUnlocked = false;

    private void Start()
    {
        if (lockPanel != null) lockPanel.SetActive(false);
    }

    private void OnMouseDown()
    {
        OpenLockUI();
    }

    public void OpenLockUI()
    {
        if (_isUnlocked) return;
        if (lockPanel != null) lockPanel.SetActive(true);
        ResetInput();
    }

    public void AppendDigit(string digit)
    {
        if (_isUnlocked) return;
        _currentInput += digit;
        UpdateDisplay(_currentInput);
    }

    public void ClearInput()
    {
        if (_isUnlocked) return;
        ResetInput();
    }

    public void SubmitCode()
    {
        if (_isUnlocked) return;
        if (_currentInput == correctCode)
        {
            OnUnlockSuccess();
        }
        else
        {
            UpdateDisplay("错误");
            StartCoroutine(ResetAfterDelay(0.8f));
        }
    }

    private void OnUnlockSuccess()
    {
        if (_isUnlocked) return;
        _isUnlocked = true;

        GrantReward();

        if (autoCloseOnSuccess && lockPanel != null) lockPanel.SetActive(false);

        if (disableColliderAfterUnlock)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
        UpdateDisplay("解锁");
    }

    private void GrantReward()
    {
        if (GameData.IsItemCollected(rewardItemId))
        {
            Debug.Log($"奖励已领取过：{rewardItemId}");
            return;
        }

        Sprite spriteToUse = EnsureRewardSprite();
        if (BackpackManager.Instance != null)
        {
            BackpackManager.Instance.CollectItem(spriteToUse);
            GameData.AddCollectedItem(rewardItemId);
            Debug.Log($"保险柜奖励已发放：{spriteToUse.name}");
        }
        else
        {
            Debug.LogError("找不到BackpackManager实例，无法发放奖励");
        }
    }

    private Sprite EnsureRewardSprite()
    {
        if (rewardSprite != null)
        {
            rewardSprite.name = string.IsNullOrEmpty(rewardSpriteName) ? rewardSprite.name : rewardSpriteName;
            return rewardSprite;
        }

        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++) colors[i] = placeholderColor;
        tex.SetPixels(colors);
        tex.Apply();
        Sprite generated = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        generated.name = string.IsNullOrEmpty(rewardSpriteName) ? "placeholder_reward" : rewardSpriteName;
        Debug.LogWarning($"未找到奖励素材，生成占位Sprite：{generated.name}");
        return generated;
    }

    private void ResetInput()
    {
        _currentInput = "";
        UpdateDisplay(_currentInput);
    }

    private void UpdateDisplay(string content)
    {
        if (displayText != null) displayText.text = content;
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetInput();
    }
}

