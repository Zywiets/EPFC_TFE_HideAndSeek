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
    public static NetworkManager instance;
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
        _sioCom.Instance.On("connect", (payload) => { Debug.Log(payload+"     ***** LOCAL: Connected "+ _sioCom.Instance.SocketID+"  *****"); });
        _sioCom.Instance.On("play", OnPlay);
        _sioCom.Instance.On("other player connected", OnOtherPlayerConnected);
        _sioCom.Instance.On("player move", OnPlayerMove);
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
        PlayerJson playerJson = new PlayerJson(playerName, playerSpawnPoints);
        string data = JsonUtility.ToJson(playerJson);
        Debug.Log(data + " Est envoyé au server ---------");
        _sioCom.Instance.Emit("play", data, false);
        canvas.gameObject.SetActive(false);
    }
    
    public void CommandMove(Vector2 vec2)
    {
        string data = JsonUtility.ToJson(new MovementJson(vec2));
        _sioCom.Instance.Emit("player move", data, false);
        // socket.Emit("player move", new JSONObject(data));
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
        UserJson currentUser = UserJson.CreateFromJSON(data);
        Vector3 position = new Vector3(currentUser.position[0], currentUser.position[1], currentUser.position[2]);
        // Quaternion rotation = Quaternion.Euler(currentUser.rotation[0], currentUser.rotation[1], currentUser.rotation[2]);
        Quaternion rotation = Quaternion.Euler(0,0,0);

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
        UserJson userJSON = UserJson.CreateFromJSON(data);
        
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
        UserJson uSerJson = UserJson.CreateFromJSON(data);
        Destroy(GameObject.Find(uSerJson.name));
    }


    void OnPlayerMove(string data)
    {
        Debug.Log("+++++++ OnplayerMove +++");
        UserJson userJson = UserJson.CreateFromJSON(data);
        Vector2 movement = new Vector2(userJson.movement[0], userJson.movement[1]);
        if (userJson.name.Equals(playerNameInput))
        {
            return;
        }

        GameObject p = GameObject.Find(userJson.name);
        if (p != null)
        {
           ThirdPersonMovement thirdMove =  p.GetComponentInChildren<ThirdPersonMovement>();
           thirdMove.HandleOtherPlayerMovement(movement, userJson.name);
        }

    }

    void OnPlayerTurn(string data)
    {
        Debug.Log("------On player turn------");
        UserJson userJson = UserJson.CreateFromJSON(data);
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
    public class PlayerJson
    {
        public string name;
        public List<PointJson> playerSpawnPoints;

        public PlayerJson(string _name, List<GameObject> _playerSpawnPoints)
        {
            playerSpawnPoints = new List<PointJson>();
            name = _name;
            foreach (GameObject playerSpawnPoint in _playerSpawnPoints)
            {
                PointJson pointJSON = new PointJson(playerSpawnPoint);
                playerSpawnPoints.Add(pointJSON);
            }
        }
    }

    [Serializable]
    public class PointJson
    {
        public float[] position;
        public float[] rotation;
        public PointJson(GameObject spawnPoint)
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
    public class PositionJson
    {
        public float[] position;

        public PositionJson(Vector3 _position)
        {
            position = new float[] { _position.x, _position.y, _position.z };
        }
        
    }
    
    [Serializable]
    public class MovementJson
    {
        public float[] movement;

        public MovementJson(Vector2 mvt)
        {
            movement = new float[] { mvt.x, mvt.y };
        }
    }
    
    [Serializable]
    public class RotationJson
    {
        public float[] rotation;

        public RotationJson(Quaternion _rotation)
        {
            rotation = new float[] { _rotation.eulerAngles.x, _rotation.eulerAngles.y, _rotation.eulerAngles.z };
        }

    }

    [Serializable]
    public class UserJson
    {
        public string name;
        public float[] position;
        public float[] rotation;
        public float[] movement;

        public static UserJson CreateFromJSON(string data)
        {
            return JsonUtility.FromJson<UserJson>(data);
        }
    }

    [Serializable]
    public class PlayerMovementJson
    {
        public string name;
        public float[] movement;

        public static PlayerMovementJson CreateFromJSON(string data)
        {
            return JsonUtility.FromJson<PlayerMovementJson>(data);
        }
    }
    #endregion

}
