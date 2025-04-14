
using TMPro;
using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{
    
    [SerializeField] private float _timeLeftWaiting = 5;
    [SerializeField] private float _timeLeftHiding = 5;
    private float _roundHostTimer;
    private bool _isSeekerWaiting = false;
    public bool _isHiding = false;
    public bool _isRoundTimerOn;
    [SerializeField] private TextMeshProUGUI _waitTimer;
    [SerializeField] private TextMeshProUGUI _hidingTimer;
    
    
    [SerializeField] private GameObject _waitingTimerPanel;
    [SerializeField] private GameObject _hidingPanel;
    [SerializeField] private GameObject _inBetweenRoundsPanel;
    
    
    [SerializeField] private ThirdPersonMovement _thirdPersonMovement;
    private NetworkManager _networkManager;
    [SerializeField] private HiderCollision _hiderCollision;
    

    private void Start()
    {
        GetNetworkManager();
        _roundHostTimer = _timeLeftHiding;
        _networkManager.SetBeginTimer(_timeLeftHiding);
    }

    private void Update()
    {
        if (_isSeekerWaiting) { WaitingTimerManager(); }

        if (_isHiding) { HidingTimerManager(); }

        if (_isRoundTimerOn)
        {
            RoundTimeManager();
        }

    }

    public void SetWaitingTimer()
    {
        _waitingTimerPanel.SetActive(true);
        _isSeekerWaiting = true;
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
            _isSeekerWaiting = false;
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
            //_networkManager.HasFinishedHiding();
            //_isHiding = false;
            _hidingPanel.SetActive(false);
        }
    }

    private void RoundTimeManager()
    {
        if (_roundHostTimer > 0)
        {
            _roundHostTimer -= Time.deltaTime;
        }
        else
        {
            _networkManager.RoundTimerOver();
            _isRoundTimerOn = false;
        }
    }

    public void OnCollisionWithSeeker()
    {
        Debug.Log("on est dans le InGameMenuManager OnCollisionEnter");
        if (!_isHiding) return;
        _isHiding = false;
        _hidingPanel.SetActive(false);
        _networkManager.HasBeenFound();
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
        _networkManager.SetTimeSpentHiding(currentTime);
    }
}
