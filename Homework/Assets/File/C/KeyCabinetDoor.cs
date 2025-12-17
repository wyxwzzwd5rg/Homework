using UnityEngine;

public class KeyCabinetDoor : InteractableObject
{
    public GameObject tieObject; // 钥匙柜中的领带模型（默认隐藏）
    public GameObject doorObject; // 柜门模型（需要消失的对象）

    private void Start()
    {
        // 初始隐藏领带
        if (tieObject != null)
            tieObject.SetActive(false);
    }

    // 当玩家点击柜门时调用
    public void OnDoorClicked()
    {
        // 调用背包管理器的交互方法，传入柜门标签和自身实例
        BackpackManager.Instance.OnInteractWithObject("KeyCabinetDoor", this);
    }

    // 柜门打开逻辑（供背包管理器调用）
    public void OpenDoor()
    {
        if (doorObject != null)
            doorObject.SetActive(false); // 柜门消失

        if (tieObject != null)
            tieObject.SetActive(true); // 显示领带
    }
}