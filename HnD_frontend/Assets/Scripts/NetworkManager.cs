using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Firesplash.UnityAssets.SocketIO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
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
    [SerializeField] private GameObject joinGameCanvas;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject backgroundSound;
    private MenuManager _menuManagerComponent;

    private GameObject _playerGameObject;
    private PlayerRole _localPlayerRoleComp;
    private InGameMenuManager _localPlayerInGameMenuManager;
    private string _userId;
    private bool _isSignedIn;
    private bool _isLobbyHost;
    
    private int _roundCompt = 0;
    private int _numOfSeekers;
    private List<UserJson> _playersPlayingList = new List<UserJson>();
    private List<ScoreJson> _scorersList = new List<ScoreJson>();

    private float _beginTimer;
    private float _timeSpentHiding;
    private float _totalTimeSpentHiding;
    private Vector3 _waitingPosition = new Vector3(-27, 5, -20);
    

    [SerializeField] private GameObject _waitingZone;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _sioCom = GetComponent<SocketIOCommunicator>();
        _menuManagerComponent = joinGameCanvas.GetComponent<MenuManager>();
        // subscribe to all the various websocket events  
        _sioCom.Instance.On("connect", (payload) => { Debug.Log(payload+"     ***** LOCAL: Connected "+ _sioCom.Instance.SocketID+"  *****"); });
        _sioCom.Instance.On("play", OnPlay);
        _sioCom.Instance.On("round over", OnStartNewRound);
        _sioCom.Instance.On("other player connected", OnOtherPlayerConnected);
        _sioCom.Instance.On("other player disconnected", OnOtherPlayerDisconnected);
        _sioCom.Instance.On("player move", OnPlayerMove);
        _sioCom.Instance.On("player jump", OnPlayerJump);
        _sioCom.Instance.On("other player in lobby", OnOtherPlayerInLobby);
        _sioCom.Instance.On("new host", OnNewHost);
        _sioCom.Instance.On("test", OnTest);
        _sioCom.Instance.On("others in lobby", OnOthersInLobby);
        _sioCom.Instance.On("delete host lobby", OnDeleteHost);
        _sioCom.Instance.On("sign in", OnSignIn);
        _sioCom.Instance.On("sign up", OnSignUp);
        _sioCom.Instance.On("lobby host", OnLobbyHost);
        _sioCom.Instance.On("hosts data", OnHostsLobbyList );
        _sioCom.Instance.On("rankings", OnRankingsReceived);
        _sioCom.Instance.On("started seeking", OnSeekerReleased);
        _sioCom.Instance.On("player found", OnSetSeeker);
        _sioCom.Instance.On("player score", OnSetOtherPlayerFinalScore);
        _sioCom.Instance.On("user_id", OnSetUserId);
        _sioCom.Instance.On("user_socket", OnSetUserSocket);
        _sioCom.Instance.Connect();
    }

    public void JoinGame()
    {
        Shuffle(playerSpawnPoints);
        UserJson playAndSpawns = new UserJson(_currentUser.id, playerSpawnPoints, _currentUser.lobby);
        string data = JsonUtility.ToJson(playAndSpawns);
        _sioCom.Instance.Emit("play", data, false);
    }
    private IEnumerator ConnectToServer()
    {
        yield return new WaitForSeconds(0.5f);
        
        _sioCom.Instance.Emit("player connect");
        
        yield return new WaitForSeconds(1f);
    }

    public void SignOut()
    {
        _currentUser = null;
        _isSignedIn = false;
    }

    #region Commands

    public void JoinLobby()
    {
        // this is where the player communicates <ith the backend for the lobby logic
        UserJson player = new UserJson(_currentUser.id);
        string data = JsonUtility.ToJson(player);
        _sioCom.Instance.Emit("join lobby", data, false);
    }

    public void LobbyChosen(string lobbyName)
    {
        _currentUser.lobby = lobbyName;
        string data = JsonUtility.ToJson(_currentUser);
        _sioCom.Instance.Emit("lobby chosen", data, false);
        _menuManagerComponent.SetLobbyPanel();
        
    }
    public void BecomeLobbyHost()
    {
        _currentUser.lobby = _currentUser.id;
        string data = JsonUtility.ToJson(_currentUser);
        _sioCom.Instance.Emit("new lobby host", data, false);
        _menuManagerComponent.AddToLobby(_currentUser.displayName, _currentUser.id);
    }
    public void GetLobbies()
    {
        _sioCom.Instance.Emit("get lobbies");
    }
    public void SendFormToDB(string us, string em, string pa)
    
    {
        _signUpForm = new SignUpFormJson(us, em, pa);
        string data = JsonUtility.ToJson(_signUpForm);
        //Debug.Log("--------- SendFormToDB in Network Manager " + data);
        _sioCom.Instance.Emit("sign up", data, false);
    }
    
    public void CheckSignIn(string signInUsername, string signInPassword)
    {
        //Debug.Log("++++++++++ Sending message to DB");
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
        string data = JsonUtility.ToJson(_currentUser);
        Debug.Log(" the info sent to the other for the release " + _currentUser);
        _sioCom.Instance.Emit("started seeking", data, false);
        if (_isLobbyHost)
        {
            _localPlayerInGameMenuManager._isRoundTimerOn = true;
        }
        _waitingZone.SetActive(false);
    }

    public void RoundTimerOver()
    {
        _sioCom.Instance.Emit("round timer over");
        if (_localPlayerInGameMenuManager._isHiding)
        {
            _totalTimeSpentHiding += _timeSpentHiding;
        }
        OnStartRound("f");
    }

    public void HasBeenFound() {
        _totalTimeSpentHiding += _timeSpentHiding;
        
        string data = JsonUtility.ToJson(_currentUser);
        _localPlayerRoleComp.SetSeekerMaterial();
        //Debug.Log("Le player a été trouvé"+ _numOfSeekers +"  "+ _playersPlayingList.Count);
        _sioCom.Instance.Emit("has been found", data, false);
        _numOfSeekers++;
        if (_numOfSeekers >= _playersPlayingList.Count)
        {
            OnStartRound("l");
            //Debug.Log("Restart the round because has been found");
        }
    }

    public void SendMessageToDeleteHost()
    {
        string data = JsonUtility.ToJson(_currentUser);
        _sioCom.Instance.Emit("delete lobby", data, false);
        //_menuManagerComponent.DeleteHostFromHostsLobby(_currentUser.name);
    }

    private void SendScoresToDB(List<ScoreJson> scores)
    {
        string data = JsonConvert.SerializeObject(scores);
        _sioCom.Instance.Emit("add scores", data, false);
    }

    private void SendScoreToPlayers()
    {
        int sc = Mathf.RoundToInt(_totalTimeSpentHiding) * 10;
        ScoreJson finalScore = new ScoreJson(_currentUser.id, sc, _userId);
        string data = JsonUtility.ToJson(finalScore);
        _sioCom.Instance.Emit("final score", data, false);
        AddValueToEndScoreTable(finalScore);
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

    void OnNewHost(string data)
    {
        UserJson host = JsonUtility.FromJson<UserJson>(data);
        //Debug.Log("new host added to lobby ====   " + host.name);
        _menuManagerComponent.AddToHostsLobby(host.displayName,host.lobby);
    }
    void OnHostsLobbyList(string data)
    {
        var hostsList = JsonHelper.FromJson<UserJson>(data);
        foreach (var host in hostsList)
        {
            //Debug.Log(host);
            _menuManagerComponent.AddToHostsLobby(host.displayName, host.id);
        }
    }

    void OnDeleteHost(string data)
    {
        Debug.Log("the data received to delete the lobby "+data);
        _menuManagerComponent.DeleteHostFromHostsLobby(data);
    }

    void OnOthersInLobby(string data)
    {
        //Debug.Log(data+ "\n the other players in the lobby");
        UserJson[] othLobbyList = JsonHelper.FromJson<UserJson>(data);
        //Debug.Log(othLobbyList.Length + "\n the lenght of the lobby");
        foreach (var users in othLobbyList)
        {
            _menuManagerComponent.AddToLobby(users.displayName,users.id);
        }
        _menuManagerComponent.AddToLobby(_currentUser.displayName,_currentUser.id);
    }
    void OnOtherPlayerInLobby(string data)
    {
        if (!_isSignedIn) return;
        // Debug.Log("other player in lobby : "+ data);
        UserJson lobbyPlayer = UserJson.CreateFromJSON(data);
        if (lobbyPlayer.id.Equals(_currentUser.id)) return;
        _menuManagerComponent.AddToLobby(lobbyPlayer.displayName,lobbyPlayer.id);
    }
    void OnSignIn(string data)
    {
        bool res = bool.Parse(data);
        _isSignedIn = res;
        if (_isSignedIn)
        {
            _currentUser = new UserJson(_signUpForm.username);
            _menuManagerComponent.SetOptionMenuPanel();
            _menuManagerComponent.DisableSignInErrorMessage();
        }
        else
        {
            _menuManagerComponent.SetSignInErrorMessage();
        }
    }
    void OnSignUp(string data)
    {
        bool res = bool.Parse(data);
        _isSignedIn = res;
        if (res)
        {
            _currentUser = new UserJson(_signUpForm.username);
            _menuManagerComponent.SetOptionMenuPanel();
        }
    }
    void OnPlay(string data)
    {
        joinGameCanvas.gameObject.SetActive(false);
        UserJson[] usersJson = JsonHelper.FromJson<UserJson>(data);
        _playersPlayingList = new List<UserJson>(usersJson);
        //Debug.Log(data);
        backgroundSound.SetActive(false);
        StartRound();
        
    }

    void OnStartNewRound(string data)
    {
        if (_localPlayerInGameMenuManager._isHiding)
        {
            _totalTimeSpentHiding += _timeSpentHiding;
        }
        OnStartRound("f");
    }
    void OnStartRound(string data)
    {
        RemoveAllPlayersGameObject();
        
        if (_roundCompt < _playersPlayingList.Count)
        {
            //Debug.Log("Le compteur est à "+_roundCompt+" et le playersList est à "+ _playersPlayingList.Count);
            _waitingZone.SetActive(true);
            StartRound();
        }
        else
        {
            //send score and set score scene
            // Send message to DB to reset lobbyhost 
            SendScoreToPlayers();
            Debug.Log("\n The game has ended");
        }
    }

    void EndGame()
    {
        _playersPlayingList.Clear();
        _playersGameObjectDict.Clear();
        _menuManagerComponent.DeleteAllFromLobby();
        _isLobbyHost = false;
        _numOfSeekers = 0;
        _roundCompt = 0;
        _totalTimeSpentHiding = 0;
        
        joinGameCanvas.gameObject.SetActive(true);
        backgroundSound.SetActive(true);
        _waitingZone.SetActive(true);
        _menuManagerComponent.SetEndGamePanel();
        
        string data = JsonUtility.ToJson(_currentUser);
        _sioCom.Instance.Emit("end lobby", data, false);
    }

    void OnSetOtherPlayerFinalScore(string data)
    {
        //Debug.Log("Le message reçu par le network pour le score est "+data);
        ScoreJson sco = ScoreJson.CreateFromJSON(data);
        AddValueToEndScoreTable(sco);
    }

    void AddValueToEndScoreTable(ScoreJson scorer)
    {
        _scorersList.Add(scorer);
        // Debug.Log("Value added to scoreList "+scorer.name);
        // Debug.Log("la valeur du scoreCount est "+ _scorersList.Count+ " et le _playerPLaying "+_playersPlayingList.Count);
        if (_scorersList.Count == _playersPlayingList.Count)
        {
            _menuManagerComponent.AddToEndGameScore(_scorersList);
            if (_isLobbyHost)
            {
                SendScoresToDB(_scorersList);
                SendMessageToDeleteHost();
            }
            _scorersList.Clear();
            EndGame();
        }
    }
    private void StartRound()
    {
        _numOfSeekers = 0;
        for(int i = 0; i < _playersPlayingList.Count; ++i)
        {
            UserJson plyrs = _playersPlayingList[i];
            Debug.Log("le player est dans la _playerPlayinglist : "+plyrs.id);
            if (!_playersGameObjectDict.ContainsKey(plyrs.id))
            {
                Vector3 position = new Vector3(plyrs.position[0], plyrs.position[1], plyrs.position[2]);
                Quaternion rotation = Quaternion.Euler(0,0,0);
                _playerGameObject = Instantiate(plyrs.id.Equals(_currentUser.id) ? localplayerGameObject : nonLocalPlayerGameObject, i == _roundCompt ? _waitingPosition : position, rotation);
                _playerGameObject.name = plyrs.id;
                _playersGameObjectDict.Add(_playerGameObject.name, _playerGameObject);
                if (plyrs.id.Equals(_currentUser.id))
                {
                    _localPlayerRoleComp = _playerGameObject.GetComponent<PlayerRole>();
                    _localPlayerInGameMenuManager = _playerGameObject.GetComponentInChildren<InGameMenuManager>();
                    _localPlayerRoleComp.ChangeLocalPlayerStatus();
                    CinemachineFreeLook cameraPriority = _playerGameObject.GetComponentInChildren<CinemachineFreeLook>();
                    cameraPriority.Priority = 10;
                }
                if (i == _roundCompt)
                {
                    SetSeekerInNewGame(plyrs.id);
                }
            }
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
            GameObject o = GameObject.Find(userJson.id);
            if (o != null)
            {
                Debug.Log("couldn't instantiate the player "+userJson.id);
                return;
            }
            print("someone managed to instantiate");
            GameObject p = Instantiate(nonLocalPlayerGameObject, position, rotation);
            p.name = userJson.id;
            _playersGameObjectDict.Add(p.name, p);
        }


    void OnSetSeeker(string data)
    {
        ++_numOfSeekers;
        if (_numOfSeekers >= _playersPlayingList.Count)
        {
            //Debug.Log("nombre max de seeker atteint");
            OnStartRound("l");
        }
        else
        {
            //Debug.Log("On set les seeker details");
           GameObject seeker = _playersGameObjectDict[data];
           PlayerRole seekerRole = seeker.GetComponent<PlayerRole>();
           seekerRole.SetSeekerMaterial(); 
        }
        
    }
    void OnLobbyHost(string data)
    {
        startGameButton.SetActive(true);
        _isLobbyHost = true;
    }

    void OnSeekerReleased(string data)
    {
        Debug.Log("The seeker is being released 111111111111");
        if (_isLobbyHost)
        {
            _localPlayerInGameMenuManager._isRoundTimerOn = true;
        }
        Debug.Log("The seeker is being released 222222222");
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
        Destroy(GameObject.Find(userJson.id));
        _playersGameObjectDict.Remove(userJson.id);
        _menuManagerComponent.DeleteUserFromLobby(userJson.id);
    }


    void OnPlayerMove(string data)
    {
        UserJson userJson = UserJson.CreateFromJSON(data);
        if (userJson.id.Equals(_currentUser.id))
        {
            return;
        }
        Vector2 movement = new Vector2(userJson.movement[0], userJson.movement[1]);
        Quaternion rotation = Quaternion.Euler(userJson.rotation[0],userJson.rotation[1],userJson.rotation[2]);
        Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
        if (_playersGameObjectDict.ContainsKey(userJson.id))
        {
            GameObject p = _playersGameObjectDict[userJson.id];
            if (p != null) {
                ThirdPersonMovement thirdMove =  p.GetComponentInChildren<ThirdPersonMovement>(); 
                thirdMove.HandleOtherPlayerMovement(movement, rotation, position);
            }else {
                Debug.Log("--------- Couldn't find Player ( " + userJson.id+" ) to move");
            }
        }
        
    }

    void OnSetUserId(string data)
    {
        _userId = data;
    }

    void OnSetUserSocket(string data)
    {
        _currentUser.id = data;
    }
    void OnPlayerJump(string data)
    {
        Debug.Log("°°°°°° OnplayerJump °°°°°");
        UserJson userJson = UserJson.CreateFromJSON(data);
        if (userJson.id.Equals(playerNameInput))
        {
            return;
        }
        GameObject p = GameObject.Find(userJson.id);
        if (p != null)
        {
            ThirdPersonMovement thirdMove =  p.GetComponentInChildren<ThirdPersonMovement>();
            thirdMove.HandleJump();
        }
    }

    #endregion

    private void SetSeekerInNewGame(string seekerName)
    {
        //Debug.Log("setting the seeker "+ seekerName);
        if (seekerName.Equals(_currentUser.id))
        {
            _localPlayerRoleComp.SetSeekerMaterial();
            _localPlayerInGameMenuManager.SetWaitingTimer();
            _localPlayerInGameMenuManager._isHiding = false;
            _numOfSeekers++;
        }
        else
        {
            OnSetSeeker(seekerName);
        }
    }

    public void SetBeginTimer(float time)
    {
        //Debug.Log("le time de départ est "+ time);
        _beginTimer = time;
    }

    public void SetTimeSpentHiding(float time)
    {
        _timeSpentHiding = _beginTimer - time;
    }

    #region JSONMessageClasses

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
    public class ScoreJson
    {
        public string name;
        public int score;
        public string id;

        public ScoreJson(string na, int sc, string i)
        {
            name = na;
            score = sc;
            id = i;

        }
        public static ScoreJson CreateFromJSON(string data)
        {
            return JsonUtility.FromJson<ScoreJson>(data);
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
        public string id;
        public string displayName;
        public float[] position;
        public float[] rotation;
        public float[] movement;
        public List<PointJson> playerSpawnPoints;
        public string lobby;

        public UserJson(string _id, List<GameObject> _playerSpawnPoints, string _lobby)
        {
            playerSpawnPoints = new List<PointJson>();
            id = _id;
            lobby = _lobby;
            foreach (GameObject playerSpawnPoint in _playerSpawnPoints)
            {
                PointJson pointJSON = new PointJson(playerSpawnPoint);
                playerSpawnPoints.Add(pointJSON);
            }
        }

        public static UserJson CreateFromJSON(string data)
        {
            return JsonUtility.FromJson<UserJson>(data);
        }

        public UserJson(string n)
        {
            displayName = n;
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
    
   
    public static class JsonHelper
    { // To help with the limitations of JsonUtility
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
