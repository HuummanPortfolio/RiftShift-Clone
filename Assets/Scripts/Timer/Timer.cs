using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Timer : MonoBehaviour
{
    public Text timerText;

    public bool isRunning { get; private set; } = false;
    public bool isPaused { get; private set; } = false;

    public string formattedTime { get; private set; } = string.Empty;
    private float time = 0f;
    public TimeSpan convertedTimeSpan { get; private set; }
    private IEnumerator timerRunner;

    private void Awake()
    {
        time = 0f;
        SetTimerText(0);
    }

    private void OnEnable()
    {
        EventManager.OnTimerStart += OnTimerStart;
        EventManager.OnTimerPause += OnTimerPause;
        EventManager.OnTimerEnd += OnTimerEnd;
    }

    private void OnDisable()
    {
        EventManager.OnTimerStart -= OnTimerStart;
        EventManager.OnTimerPause -= OnTimerPause;
        EventManager.OnTimerEnd -= OnTimerEnd;
    }

    public void OnTimerStart()
    {
        isRunning = true;
        timerRunner = RunTimer();
        StartCoroutine(timerRunner);
        
    }

    public void OnTimerPause(bool paused)
    {
        isPaused = paused;
    }

    public void OnTimerEnd()
    {
        isRunning = false;
        StopCoroutine(timerRunner);
    }

    public void SetTimerText(float timeInSeconds)
    {
        convertedTimeSpan = TimeSpan.FromSeconds(time);
        if (convertedTimeSpan.TotalMinutes < 60)
            formattedTime = string.Format("{0:00}:{1:00}.{2:000}", convertedTimeSpan.Minutes, convertedTimeSpan.Seconds, convertedTimeSpan.Milliseconds);
        else
            formattedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", convertedTimeSpan.Hours, convertedTimeSpan.Minutes, convertedTimeSpan.Seconds, convertedTimeSpan.Milliseconds);


        timerText.text = formattedTime;
    }

    IEnumerator RunTimer()
    {
        while (!isPaused)
        {
            time += Time.deltaTime;
            SetTimerText(time);
            yield return null;
        }
    }

    #region TEST
    
    #endregion
}
