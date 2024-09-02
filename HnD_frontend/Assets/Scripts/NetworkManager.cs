using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Firesplash.UnityAssets.SocketIO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    private SocketIOCommunicator _sioCom;
    public string playerNameInput;
    public GameObject localplayerGameObject;
    public GameObject nonLocalPlayerGameObject;
    public List<GameObject> playerSpawnPoints;
    private UserJson _currentUser;
    private SignUpFormJson _signUpForm;
    private Dictionary<string,GameObject> _playersGameObjectDict = new Dictionary<string, GameObject>();
    private List<string> _nonLocalPlayersNames = new List<string>();
    [SerializeField] private GameObject joinGameCanvas;
    [SerializeField] private GameObject startGameButton;
    private MenuManager _menuManagerComponent;

    private GameObject _playerGameObject;
    private PlayerRole _localPlayerRoleComp;
    private InGameMenuManager _localPlayerInGameMenuManager;
    private bool _isSignedIn;
    private bool _isLobbyHost;

    private Dictionary<string, double> _scorebord = new Dictionary<string, double>();
    private List<string> _toBeSeekerList = new List<string>();
    private int _roundCompt = 0;
    private int _numOfSeekers;
    private List<UserJson> _playersPlayingList = new List<UserJson>();

    private Vector3 _waitingPosition = new Vector3(-27, 5, -20);

    [SerializeField] private GameObject _waitingZone;
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
        _menuManagerComponent = joinGameCanvas.GetComponent<MenuManager>();
        // subscribe to all the various websocket events  
        _sioCom.Instance.On("connect", (payload) => { Debug.Log(payload+"     ***** LOCAL: Connected "+ _sioCom.Instance.SocketID+"  *****"); });
        _sioCom.Instance.On("play", OnPlay);
        _sioCom.Instance.On("start round", OnStartRound);
        _sioCom.Instance.On("other player connected", OnOtherPlayerConnected);
        _sioCom.Instance.On("other player disconnected", OnOtherPlayerDisconnected);
        _sioCom.Instance.On("player move", OnPlayerMove);
        _sioCom.Instance.On("player jump", OnPlayerJump);
        _sioCom.Instance.On("other player in lobby", OnOtherPlayerInLobby);
        _sioCom.Instance.On("test", OnTest);
        _sioCom.Instance.On("sign in", OnSignIn);
        _sioCom.Instance.On("sign up", OnSignUp);
        _sioCom.Instance.On("lobby host", OnLobbyHost);
        _sioCom.Instance.On("rankings", OnRankingsReceived);
        _sioCom.Instance.On("started seeking", OnSeekerReleased);
        _sioCom.Instance.Connect();
    }

    public void JoinGame()
    {
        Debug.Log("+++++++++ Le bouton join game fonctionne ++++++++");
        Shuffle(playerSpawnPoints);
        PlayerJson playAndSpawns = new PlayerJson(_currentUser.name, playerSpawnPoints);
        string data = JsonUtility.ToJson(playAndSpawns);
        _sioCom.Instance.Emit("play", data, false);
    }
    private IEnumerator ConnectToServer()
    {
        
        yield return new WaitForSeconds(0.5f);
        
        _sioCom.Instance.Emit("player connect");
        
        yield return new WaitForSeconds(1f);

        // Shuffle(playerSpawnPoints);
        // PlayerJson playAndSpawns = new PlayerJson(_currentUser.name, playerSpawnPoints);
        // string data = JsonUtility.ToJson(playAndSpawns);
        // // Debug.Log(data + " Est envoyé au server ---------");
        // _sioCom.Instance.Emit("play", data, false);
        // joinGameCanvas.gameObject.SetActive(false);
    }

    public void SignOut()
    {
        _currentUser = null;
        _isSignedIn = false;
    }

    #region Commands

    public void JoinLobby()
    {
        PlayerJson player = new PlayerJson(_currentUser.name);
        string data = JsonUtility.ToJson(player);
        _sioCom.Instance.Emit("join lobby", data, false);
    }

    public void SendFormToDB(string us, string em, string pa)
    {
        _signUpForm = new SignUpFormJson(us, em, pa);
        string data = JsonUtility.ToJson(_signUpForm);
        Debug.Log("--------- SendFormToDB in Network Manager " + data);
        _sioCom.Instance.Emit("sign up", data, false);
    }
    
    public void CheckSignIn(string signInUsername, string signInPassword)
    {
        Debug.Log("++++++++++ Sending message to DB");
        _signUpForm = new SignUpFormJson(signInUsername, signInPassword);
        string data = JsonUtility.ToJson(_signUpForm);
        _sioCom.Instance.Emit("sign in", data, false);
    }

    public void GetAllRankings()
    {
        _sioCom.Instance.Emit("rankings");
    }
    
    public void CommandMove(Vector2 vec2, Quaternion newRot, Vector3 newPos)
    {
        _currentUser.movement = new[] { vec2.x, vec2.y };
        _currentUser.rotation = new[] {newRot.eulerAngles.x, newRot.eulerAngles.y, newRot.eulerAngles.z };
        _currentUser.position = new[] { newPos.x, newPos.y, newPos.z }; 
        string data = JsonUtility.ToJson(_currentUser);
        _sioCom.Instance.Emit("player move", data, false);
    }

    public void StartSeekingTimers()
    {
        string data = _currentUser.name;
        _sioCom.Instance.Emit("started seeking", data, true);
        _waitingZone.SetActive(false);
    }

    public void HasFinishedHiding()
    {
        _sioCom.Instance.Emit("finished hiding");
        OnStartRound("f");
        
        
    }

    public void HasBeenFound() {
        string data = _currentUser.name;
        _sioCom.Instance.Emit("has been found", data, true);
    }

    public void CommandJump()
    {
        string data = _currentUser.ToString();
        _sioCom.Instance.Emit("player jump", data, false);
    }
    
    public void ReadInputName(string inpName)
    {
        playerNameInput = inpName;
    }
    
    #endregion

    #region Listening

    void OnTest(string data)
    {
        Debug.Log(data);
    }

    void OnOtherPlayerInLobby(string data)
    {
        if (!_isSignedIn) return;
        Debug.Log("other player in lobby : "+ data);
        UserJson lobbyPlayer = UserJson.CreateFromJSON(data);
        if (lobbyPlayer.name.Equals(_currentUser.name)) return;
        _menuManagerComponent.AddToLobby(lobbyPlayer.name);
    }
    void OnSignIn(string data)
    {
        Debug.Log("Recienve message from DB +++++++++++++");
        bool res = bool.Parse(data);
        _isSignedIn = res;
        if (res)
        {
            Debug.Log("Result positive from DB ++++++++++");
            _currentUser = new UserJson(_signUpForm.username);
            _menuManagerComponent.SetOptionMenuPanel();
        }
        else
        {
            _menuManagerComponent.SetSignInErrorMessage();
        }
    }
    void OnSignUp(string data)
    {
        Debug.Log(data);
    }
    void OnPlay(string data)
    {
        joinGameCanvas.gameObject.SetActive(false);
        Debug.Log("++++you joined OnPlay function ++++");
        UserJson[] usersJson = JsonHelper.FromJson<UserJson>(data);
        _playersPlayingList = new List<UserJson>(usersJson);
        StartRound();
        
    }

    void OnStartRound(string data)
    {
        RemoveAllPlayersGameObject();
        if (_roundCompt < _playersPlayingList.Count)
        {
            _waitingZone.SetActive(true);
            StartRound();
        }
        else
        {
            //send score and set score scene
            
            Debug.Log("\n The game has ended");
        }
    }

    private void StartRound()
    {
        for(int i = 0; i < _playersPlayingList.Count; ++i)
        {
            UserJson plyrs = _playersPlayingList[i];
            Vector3 position = new Vector3(plyrs.position[0], plyrs.position[1], plyrs.position[2]);
            Quaternion rotation = Quaternion.Euler(0,0,0);
            _playerGameObject = Instantiate(plyrs.name.Equals(_currentUser.name) ? localplayerGameObject : nonLocalPlayerGameObject, i == _roundCompt ? _waitingPosition : position, rotation);
            _playerGameObject.name = plyrs.name;
            _playersGameObjectDict.Add(_playerGameObject.name, _playerGameObject);
            if (plyrs.name.Equals(_currentUser.name))
            {
                _localPlayerRoleComp = _playerGameObject.GetComponent<PlayerRole>();
                _localPlayerInGameMenuManager = _playerGameObject.GetComponentInChildren<InGameMenuManager>();
                _localPlayerRoleComp.ChangeLocalPlayerStatus();
                CinemachineFreeLook cameraPriority = _playerGameObject.GetComponentInChildren<CinemachineFreeLook>();
                cameraPriority.Priority = 10;
            }
            // _scorebord.Add(plyrs.name, 0);
            _toBeSeekerList.Add(plyrs.name);
            if (i == _roundCompt) SetSeekerInNewGame();
        }
        ++_roundCompt;
    }

    private void RemoveAllPlayersGameObject()
    {
        foreach (var key in new List<string>(_playersGameObjectDict.Keys)) {
            GameObject obj = _playersGameObjectDict[key];
            Destroy(obj);
            _playersGameObjectDict.Remove(key);
        }
        _playersGameObjectDict.Clear();
    }
    void OnOtherPlayerConnected(string data)
        {
            print("someone joined");
            UserJson userJson = UserJson.CreateFromJSON(data);
            
            Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
            Quaternion rotation = Quaternion.Euler(0,0,0);
            GameObject o = GameObject.Find(userJson.name);
            if (o != null)
            {
                Debug.Log("couldn't instantiate the player "+userJson.name);
                return;
            }
            print("someone managed to instantiate");
            GameObject p = Instantiate(nonLocalPlayerGameObject, position, rotation);
            p.name = userJson.name;
            _playersGameObjectDict.Add(p.name, p);
        }
    
    

    void OnLobbyHost(string data)
    {
        startGameButton.SetActive(true);
        _isLobbyHost = true;
    }

    void OnSeekerReleased(string data)
    {
        _localPlayerInGameMenuManager.SetHidingTimer();
        _waitingZone.SetActive(false);
    }
    void OnRankingsReceived(string data)
    {
        RankingJson[] rankings = JsonHelper.FromJson<RankingJson>(data);
        List<RankingJson> userRankings = new List<RankingJson>(rankings);

        GameObject rankingManagerObject = GameObject.Find("Rankings Panel");
        if (rankingManagerObject)
        {
            RankingTableManager ranMan = rankingManagerObject.GetComponent<RankingTableManager>();
            if (ranMan)
            {
                ranMan.FillRankings(userRankings);
            }
        }

    }
    

    void OnOtherPlayerDisconnected(string data)
    {
        UserJson userJson = UserJson.CreateFromJSON(data);
        Destroy(GameObject.Find(userJson.name));
        _playersGameObjectDict.Remove(userJson.name);
        _menuManagerComponent.DeleteUserFromLobby(userJson.name);
    }


    void OnPlayerMove(string data)
    {
        Debug.Log("+++++++ OnplayerMove +++"+ data);
        UserJson userJson = UserJson.CreateFromJSON(data);
        if (userJson.name.Equals(_currentUser.name))
        {
            return;
        }
        Vector2 movement = new Vector2(userJson.movement[0], userJson.movement[1]);
        Quaternion rotation = Quaternion.Euler(userJson.rotation[0],userJson.rotation[1],userJson.rotation[2]);
        Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
        // GameObject p = GameObject.Find(userJson.name);
        if (_playersGameObjectDict.ContainsKey(userJson.name))
        {
            GameObject p = _playersGameObjectDict[userJson.name];
            if (p != null) {
                ThirdPersonMovement thirdMove =  p.GetComponentInChildren<ThirdPersonMovement>(); 
                thirdMove.HandleOtherPlayerMovement(movement, rotation, position);
            }else {
                Debug.Log("--------- Couldn't find Player ( " + userJson.name+" ) to move");
            }
        }
        
    }

    void OnPlayerJump(string data)
    {
        Debug.Log("°°°°°° OnplayerJump °°°°°");
        UserJson userJson = UserJson.CreateFromJSON(data);
        if (userJson.name.Equals(playerNameInput))
        {
            return;
        }
        GameObject p = GameObject.Find(userJson.name);
        if (p != null)
        {
            ThirdPersonMovement thirdMove =  p.GetComponentInChildren<ThirdPersonMovement>();
            thirdMove.HandleJump();
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

    private void SetSeekerInNewGame()
    {
        string seekerName = _toBeSeekerList[_roundCompt];
        _numOfSeekers = 1;
        if (seekerName.Equals(_currentUser.name))
        {
            _localPlayerRoleComp.SetSeekerMaterial();
            _localPlayerInGameMenuManager.SetWaitingTimer();
        }
        else
        {
            GameObject seeker = _playersGameObjectDict[seekerName];
            PlayerRole seekerRole = seeker.GetComponent<PlayerRole>();
            seekerRole.SetSeekerMaterial();
        }
    }

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

        public PlayerJson(string na)
        {
            name = na;
        }
    }

    [Serializable]
    public class RankingJson
    {
        public string username;
        public double totalScore;

        public RankingJson(string na, double sc)
        {
            username = na;
            totalScore = sc;
        }
    }

    [Serializable]
    public class SignUpFormJson
    {
        public string username;
        public string email;
        public string password;

        public SignUpFormJson(string us, string em, string pa)
        {
            username = us;
            email = em;
            password = pa;
        }

        public SignUpFormJson(string us, string pa)
        {
            username = us;
            password = pa;
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

        public UserJson(string n)
        {
            name = n;
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
    
    // To help with the limitations of JsonUtility
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"Items\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
    
    public static void Shuffle<T>(List<T> list)
    {
        Random rng = new Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
