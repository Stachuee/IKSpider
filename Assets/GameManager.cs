using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    public bool playerAlive;

    [SerializeField]
    GameObject panel;
    [SerializeField]
    GameObject timer;

    [SerializeField]
    TextMeshProUGUI endTimer;
    [SerializeField]
    GameObject endPanel;
        
    private void Awake()
    {
        if (gameManager == null) gameManager = this;
        else Destroy(gameObject);

        playerAlive = true;
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void KillPlayer()
    {
        timer.SetActive(false);
        panel.SetActive(true);
        playerAlive = false;
    }

    public void Win()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time - TargetCounter.startTime);
        string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        endTimer.text = timeText;
        endPanel.SetActive(true);
        playerAlive = false;
    }
}
