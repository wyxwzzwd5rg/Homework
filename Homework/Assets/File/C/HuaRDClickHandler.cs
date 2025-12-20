using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuaRDClickHandler : MonoBehaviour
{
    public Camera Camera1;   // 原场景的主摄像机（如 Camera2）
    public Camera HuaRDCamera; // 柜子俯视摄像机（CabinetCamera）

    void OnMouseDown()
    {

        // 点击柜子时，禁用原摄像机，启用俯视摄像机
        Camera1.gameObject.SetActive(false);
        HuaRDCamera.gameObject.SetActive(true);
        ViewManager.Instance.EnterPuzzleView();
    }
}

