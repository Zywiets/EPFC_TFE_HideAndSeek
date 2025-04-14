using System.Collections.Generic;
using UnityEngine;

public class RankingTableManager : MonoBehaviour
{
    [SerializeField] private Transform rankContentContainer;
    [SerializeField] private GameObject rankEntryTemplate;

    private List<GameObject> _rankEntries = new List<GameObject>();

    void Awake()
    {
        rankEntryTemplate.SetActive(false);
    }

    public void FillRankings(List<NetworkManager.RankingJson> ranks)
    {
        for(int i = 0; i < ranks.Count; ++i)
        {
            NetworkManager.RankingJson rank = ranks[i];
            GameObject entry = Instantiate(rankEntryTemplate, rankContentContainer, false);
            _rankEntries.Add(entry);
            RankingEntryModifier rankEnt = entry.GetComponent<RankingEntryModifier>();
            rankEnt.AddValues((i+1).ToString(), rank.username, rank.totalScore.ToString());
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
