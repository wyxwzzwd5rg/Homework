using UnityEngine;
using UnityEngine.UI;

public class DrawerController : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openSprite;

    private bool isOpen = false;
    private Image buttonImage; // 这个变量将存储找到的 Image 组件

    void Start()
    {
        // 尝试通过 Button 组件来获取其内部的 Image 组件
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            buttonImage = btn.image;
            Debug.Log("通过 Button 组件成功获取了 Image 组件。");
        }
        else
        {
            Debug.LogError("在 " + gameObject.name + " 上没有找到 Button 组件！");
        }

        if (buttonImage != null && closedSprite != null)
        {
            buttonImage.sprite = closedSprite;
        }
    }

    public void ToggleDrawer()
    {
        // 首先检查 buttonImage 是否存在，避免空引用错误
        if (buttonImage == null)
        {
            Debug.LogError("buttonImage 为空，无法切换图片！");
            return;
        }

        isOpen = !isOpen;

        if (isOpen)
        {
            buttonImage.sprite = openSprite;
            Debug.Log(gameObject.name + " 被打开了。");
        }
        else
        {
            buttonImage.sprite = closedSprite;
            Debug.Log(gameObject.name + " 被关闭了。");
        }
    }
}