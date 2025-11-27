using UnityEngine;

public class ItemClickHandler : MonoBehaviour
{
    public Sprite itemSprite; // 物品的图片
    public string itemId;    // 物品的唯一ID (非常重要！)

    // 【修改点1】：去掉Start()方法中的检查逻辑

    /// <summary>
    /// 当物体被鼠标点击时调用 (适用于3D物体或2D Collider)
    /// </summary>
    void OnMouseDown()
    {
        CollectItem();
    }

    /// <summary>
    /// 公共的点击处理方法，用于UI Button的OnClick事件绑定
    /// </summary>
    public void OnClick()
    {
        CollectItem();
    }

    /// <summary>
    /// 私有的核心收集逻辑方法
    /// </summary>
    private void CollectItem()
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

    // 【修改点2】：新增一个公共方法，用于在抽屉打开时检查并设置物品状态
    public void CheckAndSetActive()
    {
        // 当抽屉打开时，调用这个方法
        // 如果物品未被收集，则显示；如果已被收集，则隐藏
        gameObject.SetActive(!GameData.IsItemCollected(itemId));
    }
}