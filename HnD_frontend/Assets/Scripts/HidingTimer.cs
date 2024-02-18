using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HidingTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float _startTime;
    private bool _isNotTouched;

    // private void OnEnable()
    // {
    //     HiderCollision.Instance.touchedEvent.AddListener(StopTimer);
    // }

    private void OnDisable()
    {
        HiderCollision.Instance.touchedEvent.RemoveListener(StopTimer);
    }

    
    void Start()
    {
        // Needs to be in start to be sure hideCollision exists already
        HiderCollision.Instance.touchedEvent.AddListener(StopTimer);
        StartTimer();
    }
    
    void Update()
    {
        if (!_isNotTouched) return;
        var elapsedTime = Time.time - _startTime;
        UpdateTimerDisplay(elapsedTime);
    }

    private void StartTimer()
    {
        _startTime = Time.time;
        _isNotTouched = true;
    }

    // Stop the timer
    private void StopTimer()
    {
        _isNotTouched = false;
    }

    // Reset the timer
    public void ResetTimer()
    {
        _startTime = Time.time;
    }

    // Update the timer display
    private void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        int milliseconds = Mathf.FloorToInt((time * 100F) % 100F);

        timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}
