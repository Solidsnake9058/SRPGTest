using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameEndManager : MonoBehaviour {

    public Text gameOver;
    public Text gameEnd;

    private void Start()
    {
        int endTpye = PlayerPrefs.GetInt("endTpye");
        gameOver.gameObject.SetActive(false);
        gameEnd.gameObject.SetActive(false);

        if (endTpye == 0)
        {
            gameEnd.gameObject.SetActive(true);
        }
        else
        {
            gameOver.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.anyKey)
        {
            SceneManager.LoadScene("Title");
        }
    }
}
