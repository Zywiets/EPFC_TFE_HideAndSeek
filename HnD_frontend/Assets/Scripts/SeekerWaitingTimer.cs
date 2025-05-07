using TMPro;
using UnityEngine;

public class SeekerWaitingTimer : MonoBehaviour
{
    [SerializeField] private float _timeLeft;
    private bool _timerOn = false;
    [SerializeField] private TextMeshProUGUI _waitTimer;
    [SerializeField] private GameObject _waitingZone;
    [SerializeField] private GameObject _panel;

    private NetworkManager _networkManager;

    private void Start()
    {
        _timerOn = true;
        GetNetworkManager();
    }

    private void Update()
    {
        if (_timerOn)
        {
            if (_timeLeft > 0)
            {
                _timeLeft -= Time.deltaTime;
                UpdateWaitTImer(_timeLeft);
            }
            else
            {
                _networkManager.StartSeeking();
                _waitingZone.SetActive(false);
                _panel.SetActive(false);
                _timerOn = false;
            }
        }
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
}