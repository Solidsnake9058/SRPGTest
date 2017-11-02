using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public static TitleManager instance { get; private set; }

    private void Start()
    {
        instance = this;
    }

    IEnumerator Loading()
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync("GameScene");
        ao.allowSceneActivation = false;
        while (!ao.isDone)
        {
            if (ao.progress >= 0.9f)
            {
                ao.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    public void ButtonStart()
    {
        StartCoroutine(Loading());
    }

    public void ButtonEndGame()
    {
        Application.Quit();
    }
}
