using UnityEngine;
using UnityEngine.UI;

public class SceneNavigator : MonoBehaviour
{
    [Header("背景图（UI Image）")]
    public Image backgroundImage;

    [Header("场景图片（Sprite 数组）")]
    public Sprite[] sceneSprites;

    [Header("左右箭头按钮")]
    public Button leftButton;
    public Button rightButton;

    private int currentIndex = 0;

    void Start()
    {
        // 初始化第一张
        UpdateScene();
    }

    public void NextScene()
    {
        if (currentIndex < sceneSprites.Length - 1)
        {
            currentIndex++;
            UpdateScene();
        }
    }

    public void PreviousScene()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateScene();
        }
    }

    private void UpdateScene()
    {
        // 更新显示的图片
        backgroundImage.sprite = sceneSprites[currentIndex];

        // 根据索引隐藏或显示箭头
        leftButton.gameObject.SetActive(currentIndex > 0);
        rightButton.gameObject.SetActive(currentIndex < sceneSprites.Length - 1);
    }
}
