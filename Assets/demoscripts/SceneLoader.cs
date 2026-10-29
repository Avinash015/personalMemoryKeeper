using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {

        Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }
    }
    public void LoadDemoScene()
    {
        SceneManager.LoadScene("Demo"); // Make sure "Demo" is spelled exactly like your scene
    }
}
