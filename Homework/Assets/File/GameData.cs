using System.Collections.Generic;
using UnityEngine;

// 这是一个静态类，不需要挂载到任何物体上
public static class GameData
{
    // 一个静态的、全局的列表，用于存储所有已收集物品的 "唯一ID"
    private static List<string> _collectedItemIds = new List<string>();

    // 添加物品ID到已收集列表
    public static void AddCollectedItem(string itemId)
    {
        if (!_collectedItemIds.Contains(itemId))
        {
            _collectedItemIds.Add(itemId);
            Debug.Log("物品已收集并记录: " + itemId);
        }
    }

    // 检查物品ID是否已被收集
    public static bool IsItemCollected(string itemId)
    {
        return _collectedItemIds.Contains(itemId);
    }

    // (可选) 用于调试或重置游戏
    public static void ClearAllCollectedItems()
    {
        _collectedItemIds.Clear();
        Debug.Log("所有已收集物品记录已清除。");
    }
}