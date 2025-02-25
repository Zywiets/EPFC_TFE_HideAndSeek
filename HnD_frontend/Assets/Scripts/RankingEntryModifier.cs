using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RankingEntryModifier : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI score;
    
    private NetworkManager _networkManager;
    public void AddValues(string ra, string na, string sc)
    {
        rank.text = ra;
        username.text = na;
        score.text = sc;
    }

    public void AddValues(string na)
    {
        username.text = na;
    }
    
    public void LobbyChosen()
    {
        GetNetworkManager();
        _networkManager.LobbyChosen(username.text);
    }
    private void GetNetworkManager()
    {
        GameObject networkManagerObject = GameObject.FindWithTag("Network Manager");
        if (networkManagerObject)
        {
            _networkManager = networkManagerObject.GetComponent<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.Log("Le _networkManger est null dans le menuManager Component");
            }
        }
    }


    
}
