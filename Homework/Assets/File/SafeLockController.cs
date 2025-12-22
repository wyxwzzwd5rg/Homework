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

    [Header("【奖励道具配置（场景中预先放置的道具）】")]
    public string rewardItemId = "item_reward"; // 道具唯一ID：safe1_lens/safe2_solvent
    public GameObject rewardPropObject; // 场景中预先放置的道具物体（必须挂ItemClickHandler，初始隐藏）

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
        // 初始化：确保道具物体初始隐藏（如果未收集）
        if (rewardPropObject != null)
        {
            ItemClickHandler itemHandler = rewardPropObject.GetComponent<ItemClickHandler>();
            if (itemHandler != null)
            {
                // 使用CheckAndSetActive逻辑：如果已收集则隐藏，未收集则隐藏（等解锁后显示）
                rewardPropObject.SetActive(false);
            }
        }

        // 已领奖则直接显示打开状态（复用GameData原有逻辑）
        if (GameData.IsItemCollected(rewardItemId))
        {
            SetSafeToOpenedState();
            // 道具已收集，确保隐藏
            if (rewardPropObject != null) rewardPropObject.SetActive(false);
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

    // 密码正确：切换保险柜图片+显示场景中的道具（核心修改）
    private void OnPasswordCorrect()
    {
        // 1. 切换保险柜为打开状态（仅换图）
        SetSafeToOpenedState();

        // 2. 关闭密码UI（可选）
        if (autoCloseUIPanel)
        {
            GlobalLockPanel.SetActive(false);
        }

        // 3. 显示场景中预先放置的道具（复用小刀逻辑）
        ShowRewardProp();
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

    // 显示场景中预先放置的道具（复用小刀逻辑）
    private void ShowRewardProp()
    {
        if (rewardPropObject == null)
        {
            Debug.LogWarning($"[保险柜] 未设置rewardPropObject，无法显示道具！");
            return;
        }

        // 检查道具是否已收集
        if (GameData.IsItemCollected(rewardItemId))
        {
            Debug.Log($"[保险柜] 道具已收集过，不显示：{rewardItemId}");
            rewardPropObject.SetActive(false);
            return;
        }

        // 获取ItemClickHandler组件
        ItemClickHandler itemHandler = rewardPropObject.GetComponent<ItemClickHandler>();
        if (itemHandler == null)
        {
            Debug.LogError($"[保险柜] 道具物体 {rewardPropObject.name} 缺少ItemClickHandler组件！请添加该组件。");
            return;
        }

        // 使用CheckAndSetActive逻辑：如果未收集则显示，已收集则隐藏
        itemHandler.CheckAndSetActive();
        
        // 确保道具Sprite名字正确（关键修复：镜片必须叫"jingpian"才能和藤蔓交互）
        if (itemHandler.itemSprite != null)
        {
            // 如果是镜片（rewardItemId包含"mirror"或"lens"或"safe1"），强制改为"jingpian"
            if (rewardItemId.Contains("mirror") || rewardItemId.Contains("lens") || rewardItemId.Contains("safe1"))
            {
                itemHandler.itemSprite.name = "jingpian";
                Debug.Log($"[镜片修复] 强制设置Sprite名字为：jingpian");
            }
            // 如果是溶解剂（rewardItemId包含"solvent"或"safe2"），确保名字为"rongjieji"
            else if (rewardItemId.Contains("solvent") || rewardItemId.Contains("safe2"))
            {
                itemHandler.itemSprite.name = "rongjieji";
                Debug.Log($"[溶解剂修复] 强制设置Sprite名字为：rongjieji");
            }
        }

        // 确保道具可见且可点击（关键修复：解决游戏画面看不到的问题）
        EnsurePropVisibleAndClickable();

        Debug.Log($"[保险柜] 道具已显示：{rewardPropObject.name}，点击可收集");
    }

    // 确保道具可见且可点击（修复游戏画面看不到的问题）
    private void EnsurePropVisibleAndClickable()
    {
        if (rewardPropObject == null) return;

        // 1. 确保物体是激活的
        rewardPropObject.SetActive(true);

        // 2. 确保SpriteRenderer可见（设置正确的Sorting Order，确保在保险柜前面）
        SpriteRenderer propRenderer = rewardPropObject.GetComponent<SpriteRenderer>();
        if (propRenderer != null)
        {
            // 确保SpriteRenderer是启用的
            propRenderer.enabled = true;
            
            // 设置Sorting Order，确保道具显示在保险柜前面（保险柜的sortingOrder + 1）
            if (_safeSpriteRenderer != null)
            {
                propRenderer.sortingOrder = _safeSpriteRenderer.sortingOrder + 1;
                Debug.Log($"[保险柜] 道具Sorting Order设置为：{propRenderer.sortingOrder}（保险柜：{_safeSpriteRenderer.sortingOrder}）");
            }
            else
            {
                // 如果保险柜没有SpriteRenderer，设置一个默认值
                propRenderer.sortingOrder = 10;
            }

            // 确保颜色不透明
            Color propColor = propRenderer.color;
            propColor.a = 1f;
            propRenderer.color = propColor;
        }

        // 3. 确保Collider是启用的（用于点击检测）
        Collider2D propCollider2D = rewardPropObject.GetComponent<Collider2D>();
        if (propCollider2D != null)
        {
            propCollider2D.enabled = true;
            Debug.Log($"[保险柜] 道具Collider2D已启用");
        }
        else
        {
            // 如果没有Collider2D，添加一个
            BoxCollider2D newCollider = rewardPropObject.AddComponent<BoxCollider2D>();
            newCollider.isTrigger = false; // 不是触发器，用于点击检测
            Debug.Log($"[保险柜] 已为道具添加BoxCollider2D");
        }

        // 4. 确保Z坐标正确（2D场景通常Z=0）
        Vector3 propPos = rewardPropObject.transform.position;
        propPos.z = 0f; // 确保在摄像机前面
        rewardPropObject.transform.position = propPos;

        Debug.Log($"[保险柜] 道具位置：{propPos}，已确保可见且可点击");
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
}
