using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneSwitcher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public int targetSceneIndex;

    private void OnMouseDown()
    {
        // 切换到目标场景
        SceneManager.LoadScene(targetSceneIndex);
    }
}
