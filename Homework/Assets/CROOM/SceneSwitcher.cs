using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneSwitcher : MonoBehaviour
{

    public int targetSceneIndex;

    private void OnMouseDown()
    {
        // 切换到目标场景
        SceneManager.LoadScene(targetSceneIndex);
    }
}
