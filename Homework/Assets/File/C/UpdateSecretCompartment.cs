using UnityEngine;

public class UpdateSecretCompartment : MonoBehaviour
{
    public GameObject blade; // 引用刀片物体

    void Start()
    {
        // 检查华容道是否通关
        if (GameData.IsItemCollected("carpet_puzzle_completed"))
        {
            OpenSecretCompartment();
        }
    }

    // 打开暗格（显示刀片）
    void OpenSecretCompartment()
    {
        blade.SetActive(true);
        // 可选：添加暗格打开的动画
    }
}