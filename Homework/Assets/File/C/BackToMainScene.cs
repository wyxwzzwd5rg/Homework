using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainScene : MonoBehaviour
{
    public void OnClick()
    {
        // 返回主场景（确保场景名称正确）
        SceneManager.LoadScene("Test2");
    }
}