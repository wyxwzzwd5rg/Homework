using UnityEngine;

public class ItemClickHandler : MonoBehaviour
{
    public Sprite itemSprite; // 物品的图片
    public string itemId;    // 物品的唯一ID (非常重要！)

    private void Start()
    {
        // 【关键】在场景加载时，检查这个物品是否已经被收集过
        if (GameData.IsItemCollected(itemId))
        {
            // 如果已经被收集，则在场景加载时就隐藏它
            gameObject.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        // 检查是否已经收集过，避免重复操作
        if (GameData.IsItemCollected(itemId))
        {
            Debug.Log("已收集过该物品！");
            // 即使被误点（比如场景加载bug导致显示了），也再次隐藏
            gameObject.SetActive(false);
            return;
        }

        // 通知背包管理器收集物品
        if (BackpackManager.Instance != null && itemSprite != null)
        {
            BackpackManager.Instance.CollectItem(itemSprite);

            // 【关键】通知全局数据管理器记录这个物品已被收集
            GameData.AddCollectedItem(itemId);

            // 隐藏场景中的物品
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("BackpackManager.Instance 或 itemSprite 未赋值！");
        }
    }
}