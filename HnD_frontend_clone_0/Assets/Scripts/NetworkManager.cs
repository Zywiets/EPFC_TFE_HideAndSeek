using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Firesplash.UnityAssets.SocketIO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    public Canvas canvas;
    private SocketIOCommunicator _sioCom;
    public string playerNameInput;
    public GameObject player;
    public List<GameObject> playerSpawnPoints;

    // private void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //     } else if (Instance != this)
    //     {
    //         Destroy(gameObject);
    //     }
    //     DontDestroyOnLoad(gameObject);
    // }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _sioCom = GetComponent<SocketIOCommunicator>();
        // subscribe to all the various websocket events  
        _sioCom.Instance.On("connect", (payload) =>
        {
            Debug.Log(payload+"     ***** LOCAL: Connected "+ _sioCom.Instance.SocketID+"  *****");
        });
        _sioCom.Instance.On("play", OnPlay);
        _sioCom.Instance.On("other player connected", OnOtherPlayerConnected);
        // _sioCom.Instance.On("player move", On);
        _sioCom.Instance.Connect();
    }

    public void JoinGame()
    {
        Debug.Log("+++++++++ Le bouton join game fonctionne ++++++++");
        StartCoroutine(ConnectToServer());
    }

    #region Commands

    private IEnumerator ConnectToServer()
    {
        
        yield return new WaitForSeconds(0.5f);
        
        _sioCom.Instance.Emit("player connect");
        
        yield return new WaitForSeconds(1f);

        string playerName = playerNameInput;
        //List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
        PlayerJSON playerJson = new PlayerJSON(playerName, playerSpawnPoints);
        string data = JsonUtility.ToJson(playerJson);
        Debug.Log(data + " Est envoyé au server ---------");
        _sioCom.Instance.Emit("play", data, false);
        canvas.gameObject.SetActive(false);
    }

    public void ReadInputName(string inpName)
    {
        playerNameInput = inpName;
    }
    
    #endregion

    #region Listening

    void OnPlay(string data)
    {
        Debug.Log("++++you joined play function ++++");
        UserJSON currentUser = UserJSON.CreateFromJSON(data);
        Vector3 position = new Vector3(currentUser.position[0], currentUser.position[1], currentUser.position[2]);
        Quaternion rotation = Quaternion.Euler(currentUser.rotation[0], currentUser.rotation[1], currentUser.rotation[2]);

        GameObject p = Instantiate(player, position, rotation);
        p.name = currentUser.name;
        PlayerRole roleManag = p.GetComponent<PlayerRole>();
        roleManag.ChangeLocalPlayerStatus();
        CinemachineFreeLook cameraPriority = p.GetComponentInChildren<CinemachineFreeLook>();
        cameraPriority.Priority = 10;
    }
    void OnOtherPlayerConnected(string data)
    {
        print("someone joined");
        UserJSON userJSON = UserJSON.CreateFromJSON(data);
        
        Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
        Quaternion rotation = Quaternion.Euler(0,0,0);
        GameObject o = GameObject.Find(userJSON.name) as GameObject;
        if (o != null)
        {
            Debug.Log("couldn't instantiate the player "+userJSON.name);
            return;
        }

        print("someone managed to instantiate");
        GameObject p = Instantiate(player, position, rotation);
        p.name = userJSON.name;
    }

    void OnOtherPlayerDisconnected(string data)
    {
        UserJSON uSerJson = UserJSON.CreateFromJSON(data);
        Destroy(GameObject.Find(uSerJson.name));
    }


    void OnPlayerMove(string data)
    {
        Debug.Log("+++++++ OnplayerMove +++");
        UserJSON userJson = UserJSON.CreateFromJSON(data);
        Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
        if (userJson.name.Equals(playerNameInput))
        {
            return;
        }

        GameObject p = GameObject.Find(userJson.name);
        if (p != null)
        {
            p.transform.position = position;
        }

    }

    void OnPlayerTurn(string data)
    {
        Debug.Log("------On player turn------");
        UserJSON userJson = UserJSON.CreateFromJSON(data);
        Quaternion rotation = Quaternion.Euler(userJson.rotation[0], userJson.rotation[1], userJson.rotation[2]);
        if (userJson.name.Equals(playerNameInput))
        {
            return;
        }
        GameObject p = GameObject.Find(userJson.name);
        if (p != null)
        {
            p.transform.rotation = rotation;
        }

    }

    #endregion

    #region JSONMessageClasses
    
    [Serializable]
    public class PlayerJSON
    {
        public string name;
        public List<PointJSON> playerSpawnPoints;

        public PlayerJSON(string _name, List<GameObject> _playerSpawnPoints)
        {
            playerSpawnPoints = new List<PointJSON>();
            name = _name;
            foreach (GameObject playerSpawnPoint in _playerSpawnPoints)
            {
                PointJSON pointJSON = new PointJSON(playerSpawnPoint);
                playerSpawnPoints.Add(pointJSON);
            }
        }
    }

    [Serializable]
    public class PointJSON
    {
        public float[] position;
        public float[] rotation;
        public PointJSON(GameObject spawnPoint)
        {
            var transform1 = spawnPoint.transform;
            var position1 = transform1.position;
            position = new float[]
            {
                position1.x,
                position1.y,
                position1.z
            };
            var rotation1 = transform1.eulerAngles; //used instead of rotation to get angles in degrees
            rotation = new float[]
            {
                rotation1.x,
                rotation1.y,
                rotation1.z
            };

        }
    }

    [Serializable]
    public class PositionJSON
    {
        public float[] position;

        public PositionJSON(Vector3 _position)
        {
            position = new float[] { _position.x, _position.y, _position.z };
        }
        
    }
    
    [Serializable]
    public class RotationJSON
    {
        public float[] rotation;

        public RotationJSON(Quaternion _rotation)
        {
            rotation = new float[] { _rotation.eulerAngles.x, _rotation.eulerAngles.y, _rotation.eulerAngles.z };
        }

    }

    [Serializable]
    public class UserJSON
    {
        public string name;
        public float[] position;
        public float[] rotation;

        public static UserJSON CreateFromJSON(string data)
        {
            return JsonUtility.FromJson<UserJSON>(data);
        }
    }
    #endregion

}
