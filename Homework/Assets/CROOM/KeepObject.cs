using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepObject : MonoBehaviour
{
    void Awake()
    {
        // 关键代码：让该物体在场景切换时不被销毁
        DontDestroyOnLoad(gameObject);
    }
}
