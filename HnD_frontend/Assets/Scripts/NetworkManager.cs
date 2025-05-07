using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Cinemachine;
using Firesplash.UnityAssets.SocketIO;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager instance;
    private SocketIOCommunicator _sioCom;
    [SerializeField] private GameObject joinGameCanvas;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject backgroundSound;
    private MenuManager _menuManagerComponent;
    public List<GameObject> playerSpawnPoints;
    private bool isInGame = false;

    private GameObject _playerGameObject;
    private PlayerRole _localPlayerRoleComp;
    private string _userId;
    private bool _isSignedIn;
    private bool _isLobbyHost;

    private int _roundCompt = 0;
    private int _numOfSeekers;

    private float _beginTimer;
    private float _timeSpentHiding;
    private float _totalTimeSpentHiding;

    private Lobby currentLobby;
    private List<Score> scores = new();

    private InGameMenuManager _localPlayerInGameMenuManager;

    private User currentUser = new();

    public GameObject localplayerGameObject;
    public GameObject nonLocalPlayerGameObject;
    private Dictionary<string, GameObject> _playersGameObjectDict = new();

    private Vector3 _waitingPosition = new Vector3(-27, 5, -20);


    [SerializeField] private GameObject _waitingZone;

    private void Start() {
        DontDestroyOnLoad(gameObject);
        _sioCom = GetComponent<SocketIOCommunicator>();
        _menuManagerComponent = joinGameCanvas.GetComponent<MenuManager>();
        _sioCom.Instance.On("connect",
            (payload) => {
                Debug.Log(payload + "     ***** LOCAL: Connected " + _sioCom.Instance.SocketID + "  *****");
            });
        _sioCom.Instance.Connect();

        _sioCom.Instance.On("loginSucceeded", OnLoginSucceeded);
        _sioCom.Instance.On("loginFailed", OnLoginFailed);
        _sioCom.Instance.On("registerFailed", OnRegisterFailed);
        _sioCom.Instance.On("registerSucceeded", OnRegisterSucceeded);
        _sioCom.Instance.On("rankingsReceived", OnRankingsReceived);
        _sioCom.Instance.On("lobbiesReceived", OnLobbiesReceived);
        _sioCom.Instance.On("lobbyCreated", OnLobbyCreated);
        _sioCom.Instance.On("lobbyJoined", OnLobbyJoined);
        _sioCom.Instance.On("roundStarted", OnRoundStarted);
        _sioCom.Instance.On("userMoved", OnUserMoved);
        _sioCom.Instance.On("userFound", OnUserFound);
        _sioCom.Instance.On("seekingStarted", OnSeekingStarted);
        _sioCom.Instance.On("lobbyDeleted", OnLobbyDeleted);
        _sioCom.Instance.On("scoreAdded", OnScoreAdded);
        _sioCom.Instance.On("gameEnded", OnGameEnded);
        _sioCom.Instance.On("hostDisconnected", OnHostDisconnected);
        _sioCom.Instance.On("disconnected", OnDisconnected);
    }

    #region Account

    public void Login(string username, string password) {
        currentUser.username = username;
        currentUser.password = password;
        string payload = JsonUtility.ToJson(currentUser);
        _sioCom.Instance.Emit("login", payload, false);
    }

    public void OnLoginSucceeded(string payload) {
        var user = JsonUtility.FromJson<User>(payload);
        currentUser.socketId = user.socketId;
        currentUser.id = user.id;
        _menuManagerComponent.SetOptionMenuPanel();
    }

    public void OnLoginFailed(string _) {
        _menuManagerComponent.SetSignInErrorMessage();
    }

    public void LogOut() {
        currentUser = new User();
    }

    public void Register(string username, string email, string password) {
        currentUser.username = username;
        currentUser.email = email;
        currentUser.password = password;
        string payload = JsonUtility.ToJson(currentUser);
        _sioCom.Instance.Emit("register", payload, false);
    }

    public void OnRegisterSucceeded(string payload) {
        var user = JsonUtility.FromJson<User>(payload);
        currentUser.username = user.username;
        currentUser.email = user.email;
        currentUser.socketId = user.socketId;
        currentUser.id = user.id;
        _menuManagerComponent.SetOptionMenuPanel();
    }

    // TODO: to implement
    public void OnRegisterFailed(string _) {
        throw new NotImplementedException();
    }

    #endregion

    #region Lobby

    public void GetLobbies() {
        _sioCom.Instance.Emit("getLobbies");
    }

    public void OnLobbiesReceived(string payload)
    {
        var lobbies = JsonConvert.DeserializeObject<List<Lobby>>(payload);
        //WrapperLobbies wrapper = JsonUtility.FromJson<WrapperLobbies>(payload);
        //var lobbies = wrapper.lobbies;
        _menuManagerComponent.DeleteAllFromHostsLobby();
        foreach (var hostLobby in lobbies)
        {
            _menuManagerComponent.AddLobbyToHost(hostLobby.name, hostLobby.id);
        }
    }

    public void CreateLobby() {
        string payload = JsonUtility.ToJson(currentUser);
        _isLobbyHost = true;
        _sioCom.Instance.Emit("createLobby", payload, false);
    }

    public void OnLobbyCreated(string payload) {
        var lobby = JsonUtility.FromJson<Lobby>(payload);
        _menuManagerComponent.AddLobbyToHost(lobby.name, lobby.id);
    }

    public void joinLobby(string id) {
        currentUser.lobbyId = id;
        string payload = JsonUtility.ToJson(currentUser);
        _sioCom.Instance.Emit("joinLobby", payload, false);
    }

    public void OnLobbyJoined(string payload) {
        var lobby = JsonConvert.DeserializeObject<Lobby>(payload);
        foreach (var user in lobby.users) {
            _menuManagerComponent.AddToLobby(user.username, user.socketId);
        }

        currentLobby = lobby;
        currentUser.lobbyId = lobby.id;
        _menuManagerComponent.SetLobbyPanel();
    }

    public void LobbyDelete() {
        var data = JsonUtility.ToJson(currentUser);
        _menuManagerComponent.DeleteHostFromHostsLobby(currentLobby.id);
    }

    public void OnLobbyDeleted(string lobbyId) { }

    #endregion

    #region TouchLogic

    public void UserFound() {
        var data = JsonUtility.ToJson(currentUser);
        _sioCom.Instance.Emit("userFound", data, false);
    }

    private void OnUserFound(string payload) {
        var user = JsonUtility.FromJson<User>(payload);
        _numOfSeekers++;
        if (currentUser.socketId == user.socketId) {
            _totalTimeSpentHiding += _timeSpentHiding;
        }

        if (_numOfSeekers >= currentLobby.users.Count) {
            StartRound();
        } else {
            if (currentUser.socketId == user.socketId) {
                _localPlayerRoleComp.SetSeekerMaterial();
            } else {
                var seeker = _playersGameObjectDict[user.socketId];
                var seekerRole = seeker.GetComponent<PlayerRole>();
                seekerRole.SetSeekerMaterial();
            }
        }
    }

    #endregion

    #region Ranking

    public void GetRankings() {
        _sioCom.Instance.Emit("rankings");
    }

    public void OnRankingsReceived(string data) {
        // we use wrapper, because JSONUtility can't deserialize to a list
        RankingWrapper wrapper = JsonUtility.FromJson<RankingWrapper>(data);
        List<Score> rankings = wrapper.rankings;

        var rankingManagerObject = GameObject.Find("Rankings Panel");

        if (!rankingManagerObject) return;
        var ranMan = rankingManagerObject.GetComponent<RankingTableManager>();

        if (!ranMan) return;
        ranMan.FillRankings(rankings);
    }

    #endregion

    #region Utils

    private static void Shuffle<T>(List<T> list) {
        var rng = new Random();
        var n = list.Count;

        while (n > 1) {
            n--;
            var k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    #endregion

    #region Movement

    public void CommandJump() {
        throw new NotImplementedException();
    }

    public void CommandMove(Vector2 vec2, Quaternion newRot, Vector3 newPos) {
        currentUser.movement = new Movement(vec2.x, vec2.y);
        currentUser.point.rotation = new[] { newRot.eulerAngles.x, newRot.eulerAngles.y, newRot.eulerAngles.z };
        currentUser.point.position = new[] { newPos.x, newPos.y, newPos.z };
        string data = JsonUtility.ToJson(currentUser);
        _sioCom.Instance.Emit("userMove", data, false);
    }

    private void OnUserMoved(string payload) {
        var user = JsonConvert.DeserializeObject<User>(payload);
        if (user.id.Equals(currentUser.id)) {
            return;
        }

        var point = user.point;
        var movement = new Vector2(user.movement?.x ?? 0, user.movement?.y ?? 0);
        var rotation = Quaternion.Euler(point.rotation[0], point.rotation[1], point.rotation[2]);
        var position = new Vector3(point.position[0], point.position[1], point.position[2]);

        if (!_playersGameObjectDict.ContainsKey(user.socketId)) return;

        var p = _playersGameObjectDict[user.socketId];

        if (p == null) return;

        var thirdMove = p.GetComponentInChildren<ThirdPersonMovement>();
        thirdMove.HandleOtherPlayerMovement(movement, rotation, position);
    }

    #endregion

    #region GettersSetters

    public void SetBeginTimer(float time) {
        _beginTimer = time;
    }

    #endregion
    
    public void StartGame() {
        Shuffle(playerSpawnPoints);
        foreach (var point in playerSpawnPoints) {
            currentLobby.spawnPoints.Add(new Point(point));
        }

        var payload = JsonConvert.SerializeObject(currentLobby);
        _sioCom.Instance.Emit("start", payload, false);
    }

    private void OnRoundStarted(string payload) {
        isInGame = true;
        var lobby = JsonConvert.DeserializeObject<Lobby>(payload);
        currentLobby = lobby;
        var count = 0;
        _numOfSeekers = 0;

        foreach (var user in lobby.users) {
            var position = new Vector3(user.point.position[0], user.point.position[1], user.point.position[2]);
            _playerGameObject =
                Instantiate(
                    user.socketId.Equals(currentUser.socketId) ? localplayerGameObject : nonLocalPlayerGameObject,
                    count == _roundCompt ? _waitingPosition : position, Quaternion.Euler(0, 0, 0));
            _playerGameObject.name = user.socketId;
            _playersGameObjectDict.Add(_playerGameObject.name, _playerGameObject);

            if (user.socketId.Equals(currentUser.socketId)) {
                joinGameCanvas.gameObject.SetActive(false);
                _localPlayerRoleComp = _playerGameObject.GetComponent<PlayerRole>();
                _localPlayerInGameMenuManager = _playerGameObject.GetComponentInChildren<InGameMenuManager>();
                _localPlayerRoleComp.ChangeLocalPlayerStatus();
                var cameraPriority = _playerGameObject.GetComponentInChildren<CinemachineFreeLook>();
                cameraPriority.Priority = 10;
                currentUser.point = (Point)user.point.Clone();
            }

            if (count == _roundCompt) {
                if (user.socketId.Equals(currentUser.socketId)) {
                    _localPlayerRoleComp.SetSeekerMaterial();
                    _localPlayerInGameMenuManager.SetWaitingTimer();
                    _localPlayerInGameMenuManager._isHiding = false;
                    _numOfSeekers++;
                } else {
                    ++_numOfSeekers;
                    if (_numOfSeekers >= currentLobby.users.Count) {
                        StartRound("l");
                    } else {
                        var seeker = _playersGameObjectDict[user.socketId];
                        var seekerRole = seeker.GetComponent<PlayerRole>();
                        seekerRole.SetSeekerMaterial();
                    }
                }
            }

            count++;
        }

        ++_roundCompt;
    }

    private void clearRound() {
        foreach (var key in new List<string>(_playersGameObjectDict.Keys)) {
            var obj = _playersGameObjectDict[key];
            Destroy(obj);
            _playersGameObjectDict.Remove(key);
        }

        _playersGameObjectDict.Clear();
        _timeSpentHiding = 0;
    }

    private void StartRound(string _ = "") {
        clearRound();

        if (_roundCompt < currentLobby.users.Count) {
            _waitingZone.SetActive(true);
            OnRoundStarted(JsonConvert.SerializeObject(currentLobby));
        } else {
            ScoreAdd();
        }
    }

    public void RoundTimerOver() {
        if (_localPlayerInGameMenuManager._isHiding) {
            _totalTimeSpentHiding += _timeSpentHiding;
        }

        _localPlayerInGameMenuManager.ResetTimers();
        StartRound("f");
    }

    public void StartSeeking() {
        var data = JsonUtility.ToJson(currentUser);
        _sioCom.Instance.Emit("seekingStart", data, false);
    }

    private void OnSeekingStarted(string _) {
        _localPlayerInGameMenuManager._isHiding = true;
        _localPlayerInGameMenuManager._isRoundTimerOn = true;
        _localPlayerInGameMenuManager._hidingPanel.SetActive(true);
        _waitingZone.SetActive(false);
    }

    public void SetTimeSpentHiding(float time) {
        _timeSpentHiding = _beginTimer - time;
    }

    private void ScoreAdd() {
        currentUser.scoreTotal = Mathf.RoundToInt(_totalTimeSpentHiding) * 10;
        var payload = JsonUtility.ToJson(currentUser);
        _sioCom.Instance.Emit("scoreAdd", payload, false);
    }

    private void OnScoreAdded(string payload) {
        var user = JsonConvert.DeserializeObject<User>(payload);
        scores.Add(new Score(user.username, user.scoreTotal, user.id));
        if (scores.Count != currentLobby.users.Count) return;

        joinGameCanvas.gameObject.SetActive(true);
        _menuManagerComponent.AddToEndGameScore(scores);
        _menuManagerComponent.SetEndGamePanel();

        if (!_isLobbyHost) return;

        var data = JsonConvert.SerializeObject(scores);
        _sioCom.Instance.Emit("saveScores", data, false);
        var parameters = new Dictionary<string, string> {
            { "lobbyId", currentLobby.id },
            { "errorMessage", null }
        };
        var res = JsonConvert.SerializeObject(parameters);
        _sioCom.Instance.Emit("gameEnd", res, false);
    }

    private void OnGameEnded(string errorMessage) {
        isInGame = false;
        clearRound();
        _menuManagerComponent.DeleteHostFromHostsLobby(currentLobby.id);
        currentLobby = null;
        scores.Clear();
        _playersGameObjectDict.Clear();
        _menuManagerComponent.DeleteAllFromLobby();
        _isLobbyHost = false;
        _numOfSeekers = 0;
        _roundCompt = 0;
        _totalTimeSpentHiding = 0;

        joinGameCanvas.gameObject.SetActive(true);
        backgroundSound.SetActive(true);
        _waitingZone.SetActive(true);
        Debug.Log(errorMessage);
        if (errorMessage != null) {
            Debug.Log(errorMessage);
        } else {
            _menuManagerComponent.SetEndGamePanel();
        }
    }

    private void OnDisconnected(string data) {
        var lobbyHost = JsonConvert.DeserializeObject<LobbyIsHost>(data);
        var lobby = lobbyHost.lobby;
        Destroy(GameObject.Find(lobby.id));
        _playersGameObjectDict.Remove(lobby.id);
        _menuManagerComponent.DeleteUserFromLobby(lobby.id);
        if (!isInGame) {
            if (lobbyHost.isLobbyHost) {
                _menuManagerComponent.DeleteAllFromLobby();
                GetLobbies();
                _menuManagerComponent.SetHostsPanel();
                return;
            }
        }

        var payload = JsonConvert.SerializeObject(new Dictionary<string, string> {
            { "lobbyId", currentLobby.id },
            { "errorMessage", "Someone left the game" }
        });
        _sioCom.Instance.Emit("gameEnd", payload, false);
        _menuManagerComponent.SetDisconnectErrorMessage(true);
    }

    private void OnHostDisconnected(string socketId) {
        _menuManagerComponent.DeleteHostFromHostsLobby(socketId);
    }
}