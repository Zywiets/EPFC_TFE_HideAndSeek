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
    private List<GameObject> _rankEntries = new List<GameObject>();

    void Awake()
    {
        GameObject networkManagerObject = GameObject.Find("Network Manager");
        if (networkManagerObject)
        {
            _networkManager = networkManagerObject.GetComponent<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("Le _networkManger est null dans le RankingTableManager");
            }
            // else
            // {
            //     _networkManager.GetRankings();
            // }
        }
        rankEntryTemplate.SetActive(false);

        
    }

    public void FillRankings(List<Score> ranks)
    {
        for(int i = 0; i < ranks.Count; ++i)
        {
            Score rank = ranks[i];
            GameObject entry = Instantiate(rankEntryTemplate, rankContentContainer, false);
            _rankEntries.Add(entry);
            RankingEntryModifier rankEnt = entry.GetComponent<RankingEntryModifier>();
            rankEnt.AddValues((i+1).ToString(), rank.username, rank.total.ToString());
            entry.gameObject.SetActive(true);
        }
    }

    public void DeleteRankingEntries()
    {
        foreach (var rank in _rankEntries)
        {
            Destroy(rank);
        }
        _rankEntries.Clear();
    }
}