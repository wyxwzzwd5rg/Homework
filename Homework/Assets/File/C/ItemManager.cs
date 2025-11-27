using UnityEngine;

// 物品管理类：全局记录选中的物品
public class ItemManager : MonoBehaviour
{
    // 静态变量：所有脚本都能直接访问（记录当前选中的物品名称，比如“螺丝刀”）
    public static string selectedItem = ""; 

    // 单例模式：确保场景中只有一个ItemManager（避免状态混乱）
    public static ItemManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景也不销毁
        }
        else
        {
            Destroy(gameObject); // 重复创建则删除
        }
    }

    // 背包选中物品时，调用这个方法（让其他脚本通知ItemManager“选中了什么”）
    public void SetSelectedItem(string itemName)
    {
        selectedItem = itemName;
        Debug.LogError("当前选中物品：" + selectedItem); // 用Error日志，确保能看到
    }

    // 清空选中状态（比如使用物品后）
    public void ClearSelectedItem()
    {
        selectedItem = "";
        Debug.LogError("已清空选中物品");
    }
}