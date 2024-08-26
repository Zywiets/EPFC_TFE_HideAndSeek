using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TreeEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private Dictionary<string, double> _scorebord = new Dictionary<string, double>();
    private List<string> _toBeSeekerList = new List<string>();
    private NetworkManager _networkManager;
    private int _roundCmpt = 0;
    private int _endRoundNum;
    // private void Start()
    // {
    //     GameObject networkManagerObject = GameObject.Find("Network Manager");
    //     if (networkManagerObject != null)
    //     {
    //         _networkManager = networkManagerObject.GetComponent<NetworkManager>();
    //         if(_networkManager == null){ Debug.Log("Le _networkManger est null ");}
    //     }
    // }
    //
    // public void SetPlayerScorebord(List<NetworkManager.UserJson> scorersList)
    // {
    //     Debug.Log("On set le scorebord");
    //     foreach (var scorer in scorersList)
    //     {
    //         Debug.Log(scorer.name);
    //         _scorebord.Add(scorer.name, 0);
    //         _toBeSeekerList.Add(scorer.name);
    //     }
    //     _endRoundNum = _toBeSeekerList.Count;
    // }
    //
    // public void SetMainSeeker()
    // {
    //     // Debug.Log("On set le seeker : "+ _toBeSeekerList[_roundCmpt]);
    //     // _networkManager.SendSeekerRoleToPlayers(_toBeSeekerList[_roundCmpt]);
    //     ++_roundCmpt;
    // }


}
