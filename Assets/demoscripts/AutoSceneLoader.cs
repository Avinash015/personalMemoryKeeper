using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSceneLoader : MonoBehaviour
{
    void Start()
    {

        StartCoroutine(LoadNextSceneAfterDelay(3f)); // 2 seconds delay
    }

    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("getstarted"); // Make sure "Demo" is added in Build Settings
    }
}
