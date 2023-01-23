using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetCounter : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField]
    TextMeshProUGUI remainText;

    [SerializeField]
    int targets;
    public static int destroyedTargets;


    public static float time;

    private void Start()
    {
        destroyedTargets = 0;
        time = 0;
    }

    private void Update()
    {
        if (!GameManager.gameManager.playerAlive) return;
        if (!GameManager.gameManager.pause) time += Time.deltaTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        text.text = timeText;

        remainText.text = destroyedTargets + "/" + targets;
        if (destroyedTargets >= targets) GameManager.gameManager.Win();
    }
}
