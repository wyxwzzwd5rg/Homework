using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SafeLockController : MonoBehaviour
{
    [Header("【保险柜状态图片】")]
    public Sprite safeClosedSprite; // 保险柜关闭图（自己选）
    public Sprite safeOpenedSprite;  // 保险柜打开图（自己选）
    private SpriteRenderer _safeSpriteRenderer; // 控制保险柜图片的组件

    [Header("【保险柜密码配置】")]
    public string safePassword; // 自定义密码（如1234）
    public bool autoCloseUIPanel = true;

    [Header("【奖励道具配置（复用原有道具）】")]
    public string rewardItemId = "item_reward"; // 原有ID：safe1_lens/safe2_solvent
    public Sprite rewardSprite; // 原有Sprite：jingpian/rongjieji
    public Vector3 rewardPropOffset = new Vector3(0, 1f, 0); // 道具显示在保险柜上方的偏移
    private GameObject _rewardPropObj; // 临时显示的道具物体

    [Header("【全局密码UI引用】")]
    public static GameObject GlobalLockPanel;
    public static Text GlobalDisplayText;
    private static SafeLockController _currentTargetSafe;
    private string _currentInput = "";
    private const int MaxInputLength = 4;
    private bool _isSafeOpened = false; // 保险柜是否已打开


    private void Awake()
    {
        // 1. 初始化保险柜图片组件（确保能显示关闭/打开图）
        _safeSpriteRenderer = GetComponent<SpriteRenderer>();
        if (_safeSpriteRenderer == null)
        {
            _safeSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        // 默认显示关闭状态的图片
        if (safeClosedSprite != null)
        {
            _safeSpriteRenderer.sprite = safeClosedSprite;
        }

        // 2. 初始化全局密码UI（复用原有逻辑）
        if (GlobalLockPanel == null)
        {
            GlobalLockPanel = GameObject.Find("Placeholder_SafePanel");
            GlobalDisplayText = GlobalLockPanel.GetComponentInChildren<Text>(true);
            BindUIButtons();
        }
        GlobalLockPanel?.SetActive(false);
    }

    private void Start()
    {
        // 已领奖则直接显示打开状态（复用GameData原有逻辑）
        if (GameData.IsItemCollected(rewardItemId))
        {
            SetSafeToOpenedState();
            if (_rewardPropObj != null) Destroy(_rewardPropObj); // 道具已领则隐藏
        }
    }

    private void OnMouseDown()
    {
        if (!_isSafeOpened && !GameData.IsItemCollected(rewardItemId))
        {
            OpenLockUI(); // 未打开时弹出密码界面
        }
    }

    public void OpenLockUI()
    {
        _currentTargetSafe = this;
        _currentInput = "";
        GlobalLockPanel.SetActive(true);
        UpdateDisplay("");
    }

    private void BindUIButtons()
    {
        // 清空原有按钮事件（防止重复绑定）
        foreach (var btn in GlobalLockPanel.GetComponentsInChildren<Button>(true))
        {
            btn.onClick.RemoveAllListeners();
        }

        // 绑定数字/退出/提交按钮（复用原有逻辑）
        foreach (var btn in GlobalLockPanel.GetComponentsInChildren<Button>(true))
        {
            if (btn.name.StartsWith("Button"))
            {
                string digit = btn.name.Replace("Button", "");
                btn.onClick.AddListener(() => AppendSingleDigit(digit));
            }
            else if (btn.name == "BtnClear")
            {
                btn.onClick.AddListener(() => GlobalLockPanel.SetActive(false));
            }
            else if (btn.name == "BtnSubmit")
            {
                btn.onClick.AddListener(SubmitPassword);
            }
        }
    }

    private void AppendSingleDigit(string digit)
    {
        if (_currentTargetSafe == null || GameData.IsItemCollected(_currentTargetSafe.rewardItemId)) return;
        if (!int.TryParse(digit, out int num))
        {
            Debug.LogError($"数字按钮命名错误：{digit}");
            return;
        }

        if (_currentTargetSafe._currentInput.Length < MaxInputLength)
        {
            _currentTargetSafe._currentInput += digit;
            UpdateDisplay(_currentTargetSafe._currentInput);
        }
    }

    private void SubmitPassword()
    {
        if (_currentTargetSafe == null || GameData.IsItemCollected(_currentTargetSafe.rewardItemId)) return;

        if (_currentTargetSafe._currentInput == _currentTargetSafe.safePassword)
        {
            _currentTargetSafe.OnPasswordCorrect(); // 密码正确逻辑
        }
        else
        {
            UpdateDisplay("错误");
            _currentTargetSafe.StartCoroutine(_currentTargetSafe.ResetInputAfterDelay(0.8f));
        }
    }

    // 密码正确：切换保险柜图片+显示可点击道具（核心修改）
    private void OnPasswordCorrect()
    {
        // 1. 切换保险柜为打开状态（仅换图，不发道具）
        SetSafeToOpenedState();

        // 2. 关闭密码UI（可选）
        if (autoCloseUIPanel)
        {
            GlobalLockPanel.SetActive(false);
        }

        // 3. 生成可点击的道具（单独显示，复用原有拾取逻辑）
        SpawnRewardProp();
    }

    // 切换保险柜图片为打开状态（永久保持）
    private void SetSafeToOpenedState()
    {
        _isSafeOpened = true;
        if (_safeSpriteRenderer != null && safeOpenedSprite != null)
        {
            _safeSpriteRenderer.sprite = safeOpenedSprite;
        }
        // 禁用保险柜点击（防止重复弹密码界面）
        Collider col3D = GetComponent<Collider>();
        if (col3D != null) col3D.enabled = false;
        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null) col2D.enabled = false;
    }

    // 生成可点击的奖励道具（彻底解决onClick报错，复用原有背包逻辑）
    private void SpawnRewardProp()
    {
        if (rewardSprite == null || _rewardPropObj != null || GameData.IsItemCollected(rewardItemId)) return;

        // 创建临时物体显示道具（复用原有Sprite）
        _rewardPropObj = new GameObject($"RewardProp_{rewardItemId}");
        _rewardPropObj.transform.position = transform.position + rewardPropOffset;

        // 添加SpriteRenderer显示道具图片（显示在保险柜上层）
        SpriteRenderer propRenderer = _rewardPropObj.AddComponent<SpriteRenderer>();
        propRenderer.sprite = rewardSprite;
        propRenderer.sortingOrder = _safeSpriteRenderer.sortingOrder + 1;

        // 添加碰撞体（用于点击）
        Collider2D propCollider = _rewardPropObj.AddComponent<BoxCollider2D>();
        propCollider.isTrigger = false;

        // 直接添加点击逻辑（无需ItemClickHandler的onClick）
        PropClickLogic clickLogic = _rewardPropObj.AddComponent<PropClickLogic>();
        clickLogic.Init(rewardItemId, rewardSprite, this);
    }

    // 道具拾取后调用（内部逻辑）
    public void OnPropCollected()
    {
        // 1. 确保Sprite名字正确（关键修复：镜片必须叫"jingpian"才能和藤蔓交互）
        if (rewardSprite != null)
        {
            // 如果rewardSpriteName配置了，就用配置的名字；否则保持原名字
            // 但如果是镜片（rewardItemId包含"mirror"或"lens"），强制改为"jingpian"
            if (rewardItemId.Contains("mirror") || rewardItemId.Contains("lens") || rewardItemId.Contains("safe1"))
            {
                rewardSprite.name = "jingpian";
                Debug.Log($"[镜片修复] 强制设置Sprite名字为：jingpian");
            }
        }

        // 2. 调用原有背包收集方法（完全复用）
        if (BackpackManager.Instance != null)
        {
            BackpackManager.Instance.CollectItem(rewardSprite);
            GameData.AddCollectedItem(rewardItemId);
            Debug.Log($"[保险柜] 获得道具：{rewardSprite?.name}，ID：{rewardItemId}");
        }
        // 3. 销毁道具物体（道具消失）
        if (_rewardPropObj != null)
        {
            Destroy(_rewardPropObj);
        }
    }

    private IEnumerator ResetInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _currentInput = "";
        UpdateDisplay("");
    }

    private static void UpdateDisplay(string content)
    {
        if (GlobalDisplayText != null) GlobalDisplayText.text = content;
    }

    // 防止场景切换时残留道具物体
    private void OnDestroy()
    {
        if (_rewardPropObj != null)
        {
            Destroy(_rewardPropObj);
        }
    }
}

// 道具点击辅助类（内嵌，无需单独创建脚本）
public class PropClickLogic : MonoBehaviour
{
    private string _rewardItemId;
    private Sprite _rewardSprite;
    private SafeLockController _safeController;

    // 初始化道具信息
    public void Init(string itemId, Sprite sprite, SafeLockController safeController)
    {
        _rewardItemId = itemId;
        _rewardSprite = sprite;
        _safeController = safeController;
    }

    // 点击道具触发收集
    private void OnMouseDown()
    {
        _safeController.OnPropCollected();
    }
}