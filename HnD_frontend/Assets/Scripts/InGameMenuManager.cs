using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{
    
    [SerializeField] private float _timeLeftWaiting = 5;
    [SerializeField] private float _timeLeftHiding = 5;
    private bool _timerOn = false;
    private bool _isHiding = false;
    [SerializeField] private TextMeshProUGUI _waitTimer;
    [SerializeField] private TextMeshProUGUI _hidingTimer;
    
    [SerializeField] private GameObject _waitingTimerPanel;
    [SerializeField] private GameObject _hidingPanel;
    [SerializeField] private GameObject _inBetweenRoundsPanel;

    [SerializeField] private ThirdPersonMovement _thirdPersonMovement;
    private NetworkManager _networkManager;
    

    private void Start()
    {
        GetNetworkManager();
    }

    private void Update()
    {
        if (_timerOn) { WaitingTimerManager(); }

        if (_isHiding) { HidingTimerManager(); }

    }

    public void SetWaitingTimer()
    {
        _waitingTimerPanel.SetActive(true);
        _timerOn = true;
    }

    public void SetHidingTimer()
    {
        _hidingPanel.SetActive(true);
        _isHiding = true;
    }
    
    

    
    private void WaitingTimerManager()
    {
        if (_timeLeftWaiting > 0)
        {
            _timeLeftWaiting -= Time.deltaTime;
            UpdateWaitTImer(_timeLeftWaiting);
        }
        else
        {
            _networkManager.StartSeekingTimers();
            _timerOn = false;
            _waitingTimerPanel.SetActive(false);
        }
    }

    private void HidingTimerManager()
    {
        if (_timeLeftHiding > 0)
        {
            _timeLeftHiding -= Time.deltaTime;
            UpdateHideTImer(_timeLeftHiding);
        }
        else
        {
            _networkManager.HasFinishedHiding();
            _isHiding = false;
            _hidingPanel.SetActive(false);
        }
    }

    private void SetEndOfRound()
    {
        _thirdPersonMovement.enabled = false;
    }
    private void GetNetworkManager()
    {
        GameObject networkManagerObject = GameObject.FindWithTag("Network Manager");
        if (networkManagerObject)
        {
            _networkManager = networkManagerObject.GetComponent<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.Log("Le _networkManger est null dans le seekingWaitingTimer Component");
            }
        }
    }

    void UpdateWaitTImer(float currentTime)
    {
        ++currentTime;
        float seconds = Mathf.FloorToInt(currentTime % 60);
        _waitTimer.text = $"{seconds:00}";
    }
    void UpdateHideTImer(float currentTime)
    {
        ++currentTime;
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);
        _hidingTimer.text = $"{minutes:00} : {seconds:00}";
    }
}
