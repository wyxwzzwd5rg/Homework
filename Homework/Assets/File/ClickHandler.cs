using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnMouseDown()
    {
        // 打印调试信息（可替换为实际功能，如拾取物品）
        Debug.Log("点击了：" + gameObject.name);

        // 示例：如果是钥匙，点击后隐藏并添加到背包（后续背包功能会用到）
        if (gameObject.name == "Key")
        {
            gameObject.SetActive(false); // 隐藏钥匙
            // 这里暂时留空，后续连接背包功能
        }
    }
}
