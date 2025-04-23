using TMPro;
using UnityEngine;

public class RankingEntryModifier : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private string id;
    
    private NetworkManager _networkManager;
    public void AddValues(string ra, string na, string sc)
    {
        rank.text = ra;
        username.text = na;
        score.text = sc;
    }

    public void AddValues(string name, string i)
    {
        username.text = name;
        id = i;
    }
    
    public void LobbyChosen()
    {
        GetNetworkManager();
        _networkManager.LobbyChosen(id);
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
