using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepObject : MonoBehaviour
{
    void Awake()
    {
        // 关键1：场景切换时不销毁该物体
        DontDestroyOnLoad(gameObject);

        // 关键2：避免重复创建（保留第一个实例，销毁后续重复的）
        if (FindObjectsOfType<KeepObject>().Length > 1)
        {
            Destroy(gameObject);
        }
    }
}