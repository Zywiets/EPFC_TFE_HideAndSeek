using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

public class RankingTableManager : MonoBehaviour
{
    [SerializeField] private Transform rankContentContainer;
    [SerializeField] private GameObject rankEntryTemplate;

    private NetworkManager _networkManager;
    void Awake()
    {
        GameObject networkManagerObject = GameObject.Find("Network Manager");
        if (networkManagerObject)
        {
            Debug.Log("--------- networkManagerObject");
            _networkManager = networkManagerObject.GetComponent<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("Le _networkManger est null dans le RankingTableManager");
            }
            else
            {
                _networkManager.GetAllRankings();
            }
        }
        rankEntryTemplate.SetActive(false);

        
    }

    public void FillRankings(List<NetworkManager.RankingJson> ranks)
    {
        for(int i = 0; i < ranks.Count; ++i)
        {
            NetworkManager.RankingJson rank = ranks[i];
            GameObject entry = Instantiate(rankEntryTemplate, rankContentContainer, false);
            RankingEntryModifier rankEnt = entry.GetComponent<RankingEntryModifier>();
            rankEnt.AddValues(i+1.ToString(), rank.username, rank.totalScore.ToString());
            entry.gameObject.SetActive(true);
        }
    }
}
