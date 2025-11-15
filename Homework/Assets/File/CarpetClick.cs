using UnityEngine;
using UnityEngine.SceneManagement;

public class CarpetClick : MonoBehaviour
{
    void OnMouseDown()
    {
        // 跳转到华容道场景
        SceneManager.LoadScene("CarpetPuzzleScene");
    }
}