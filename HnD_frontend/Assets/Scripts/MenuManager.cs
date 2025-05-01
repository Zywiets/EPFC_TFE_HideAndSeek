using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    [SerializeField] private GameObject signInPanel;
    [SerializeField] private GameObject optionsMenuPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject rankingsPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject endGameScorePanel;
    [SerializeField] private GameObject hostsPanel;

    [SerializeField] private TMP_InputField UsernameInputField;
    [SerializeField] private TMP_InputField PasswordInputField;


    private GameObject _currentPanel;

    private string _signInUsername;
    private string _signInPassword;
    private bool _isCheckingUserPass;

    [SerializeField] private TMP_Text signInError;
    private const string SignInErrorMessage = "Invalid username or password. Please try again";

    private string _registerUsername = null;
    private string _email = null;
    private string _registerPassword = null;
    private string _passwordConfirm = null;

    [SerializeField] private TMP_Text usernameError;
    private const string SignUpUsernameError = "Username must be longer than 3 and shorter than 25";
    [SerializeField] private TMP_Text emailError;
    private const string SignUpEmailError = "Must be a valid email                 !!!!";
    [SerializeField] private TMP_Text passwordError;

    private const string SignUpPasswordError =
        "Must be longer than 3, shorter than 25, contain a special character and an uppercase";

    [SerializeField] private TMP_Text passwordConfirmError;
    private const string SignUpPasswordConfirm = "Confirm Password be the same as the password";

    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";
    private Hash128 _hash;

    private Dictionary<string, GameObject> _lobbyEntries = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _hostLobbyEntries = new Dictionary<string, GameObject>();
    private List<GameObject> _endGameEntries = new List<GameObject>();
    [SerializeField] private GameObject hostPlayButton;
    [SerializeField] private Transform lobbyContentContainer;
    [SerializeField] private GameObject lobbyEntryTemplate;

    [SerializeField] private Transform lobbyHostContentContainer;
    [SerializeField] private GameObject lobbyHostEntryTemplate;

    [SerializeField] private Transform endGameScoreContainer;
    [SerializeField] private GameObject endGameScoreTemplate;
    private Dictionary<int, string> _endGameScores = new Dictionary<int, string>();

    private NetworkManager _networkManager;

    private void Start() {
        ClearHash();
        GetNetworkManager();
        SetPanel(signInPanel);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            EventSystem system = EventSystem.current;
            GameObject currentSelected = system.currentSelectedGameObject;

            if (currentSelected)
            {
                bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                Selectable next = isShiftPressed 
                    ? currentSelected.GetComponent<Selectable>().FindSelectableOnUp()  // Previous element
                    : currentSelected.GetComponent<Selectable>().FindSelectableOnDown(); // Next element

                if (next)
                {
                    // Handle regular InputField
                    InputField inputField = next.GetComponent<InputField>();
                    if (inputField)
                    {
                        inputField.OnPointerClick(new PointerEventData(system));
                        inputField.ActivateInputField();
                    }

                    // Handle TMP_InputField (TextMeshPro)
                    TMP_InputField tmpInputField = next.GetComponent<TMP_InputField>();
                    if (tmpInputField)
                    {
                        tmpInputField.OnPointerClick(new PointerEventData(system));
                        tmpInputField.ActivateInputField();
                    }

                    system.SetSelectedGameObject(next.gameObject);
                }
            }
        }
    }

    private void GetNetworkManager() {
        GameObject networkManagerObject = GameObject.FindWithTag("Network Manager");
        if (networkManagerObject) {
            _networkManager = networkManagerObject.GetComponent<NetworkManager>();
            if (_networkManager == null) {
            }
        }
    }

    #region Panel Setting

    private void SetPanel(GameObject panel) {
        if (_currentPanel) {
            _currentPanel.SetActive(false);
        }

        panel.SetActive(true);
        _currentPanel = panel;
    }

    public void SetOptionMenuPanel() {
        SetPanel(optionsMenuPanel);
    }

    public void SetRegisterPanel() {
        SetPanel(registerPanel);
    }

    public void SetHostsPanel() {
        SetPanel(hostsPanel);
    }

    public void SetSignInPanel() {
        UsernameInputField.text = "";
        PasswordInputField.text = "";
        SetPanel(signInPanel);
        _isCheckingUserPass = false;
        _networkManager.LogOut();
    }

    public void SetLobbyPanel() {
        SetPanel(lobbyPanel);
    }

    public void SetEndGamePanel() {
        SetPanel(endGameScorePanel);
    }

    public void SetRankingsPanel() {
        SetPanel(rankingsPanel);
    }

    #endregion

    #region Sign In and Sign Up Logic

    public void SetSignInUsername(string u) {
        _signInUsername = u;
    }

    public void SetSignInPassword(string p) {
        _signInPassword = p;
    }

    public void CheckUsernamePassword() {
        if (_isCheckingUserPass) return;
        if (string.IsNullOrWhiteSpace(_signInUsername) || string.IsNullOrWhiteSpace(_signInPassword)) {
            SetSignInErrorMessage();
            return;
        }

        _hash.Append(_signInPassword);
        _signInPassword = _hash.ToString();
        _networkManager.Login(_signInUsername, _signInPassword);
        _isCheckingUserPass = true;
        ClearHash();
    }

    public void SetSignInErrorMessage() {
        _isCheckingUserPass = false;
        signInError.SetText(SignInErrorMessage);
    }

    public void DisableSignInErrorMessage() {
        _isCheckingUserPass = false;
        signInError.SetText(string.Empty);
        _signInUsername = string.Empty;
        _signInPassword = string.Empty;
    }

    public void IsValidUsername(string word) {
        if (word.Length < 3 || word.Length > 25) {
            _registerUsername = null;
            usernameError.SetText(SignUpUsernameError);
        } else {
            _registerUsername = word;
            usernameError.SetText(" ");
        }
    }

    public void IsValidEmail(string word) {
        if (Regex.IsMatch(word, EmailPattern)) {
            _email = word;
            emailError.SetText("");
        } else {
            _email = null;
            emailError.SetText(SignUpEmailError);
        }
    }

    public void IsValidPassword(string word) {
        bool isValid = true;
        if (word.Length < 3 || word.Length > 25) {
            isValid = false;
        }

        if (!Regex.IsMatch(word, "[A-Z]")) {
            isValid = false;
        }

        if (!Regex.IsMatch(word, "[!@#$%^&*()_+}{\":;\'?/>,<]")) {
            isValid = false;
        }

        if (isValid) {
            _registerPassword = word;
            passwordError.SetText("");
        } else {
            _registerPassword = null;
            passwordError.SetText(SignUpPasswordError);
        }
    }

    public void IsValidConfirmPassword(string word) {
        if (word == _registerPassword && word != null) {
            passwordConfirmError.SetText("");
            _passwordConfirm = word;
        } else {
            passwordConfirmError.SetText(SignUpPasswordConfirm);
            _passwordConfirm = null;
        }
    }

    #endregion

    public void AddToEndGameScore(List<Score> scores)
    {
       scores.Sort((a, b) => b.total.CompareTo(a.total));
    
       for (var i = 0; i < scores.Count; ++i)
       {
          var scoreValues = scores[i];
          var scoreEntry = Instantiate(endGameScoreTemplate, endGameScoreContainer);
          var modifyScoreEntry = scoreEntry.GetComponent<RankingEntryModifier>();
          modifyScoreEntry.AddValues((i+1).ToString(), scoreValues.username, scoreValues.total.ToString());
          var prevPos = scoreEntry.transform.position;
          scoreEntry.transform.position = new Vector3(prevPos.x, prevPos.y - 50.0f * i);
          _endGameEntries.Add(scoreEntry);
       }
    }


    public void DeleteAllFromEndGameScore() {
        var entriesToRemove = new List<GameObject>(_endGameEntries);
        foreach (var entry in entriesToRemove) {
            _endGameEntries.Remove(entry);
            Destroy(entry);
        }
    }

    public void AddLobbyToHost(string username, string id) {
        if (_hostLobbyEntries.ContainsKey(id)) return;
        var lobbyHost = Instantiate(lobbyHostEntryTemplate, lobbyHostContentContainer, false);
        var rankEnt = lobbyHost.GetComponent<RankingEntryModifier>();
        rankEnt.AddValues(username, id);
        lobbyHost.gameObject.SetActive(true);
        _hostLobbyEntries.Add(id, lobbyHost);
    }

    public bool AddToLobby(string name, string id) {
        if (_lobbyEntries.ContainsKey(id)) return false;
        GameObject entry = Instantiate(lobbyEntryTemplate, lobbyContentContainer, false);
        RankingEntryModifier rankEnt = entry.GetComponent<RankingEntryModifier>();
        rankEnt.AddValues(name, id);
        entry.gameObject.SetActive(true);
        _lobbyEntries.Add(id, entry);
        return true;
    }

    public bool removeFromLobby(string name, string id) {
        var entry = Instantiate(lobbyEntryTemplate, lobbyContentContainer, false);
        var rankEnt = entry.GetComponent<RankingEntryModifier>();
        rankEnt.AddValues(name, id);
        entry.gameObject.SetActive(true);
        _lobbyEntries.Add(id, entry);
        return true;
    }

    public void SendMessageToDeleteHost() {
        SetPlayButton(false);
        _networkManager.LobbyDelete();
    }

    public void DeleteHostFromHostsLobby(string host) {
        if (!_hostLobbyEntries.ContainsKey(host)) return;
        var entry = _hostLobbyEntries[host];
        Destroy(entry);
        _hostLobbyEntries.Remove(host);
    }

    public void SetPlayButton(bool active) {
        hostPlayButton.SetActive(active);
    }

    public void DeleteUserFromLobby(string player) {
        GameObject entry = _lobbyEntries[player];
        Destroy(entry);
        _lobbyEntries.Remove(player);
    }

    public void DeleteAllFromLobby() {
        List<string> playersToRemove = new List<string>(_lobbyEntries.Keys);
        foreach (var entry in playersToRemove) {
            DeleteUserFromLobby(entry);
        }

        _lobbyEntries.Clear();
    }


    public void SendSignUpForm() {
        if (_registerUsername?.Length > 0 && _email?.Length > 0 && _registerPassword?.Length > 0 &&
            _passwordConfirm?.Length > 0) {
            var password = _hash;
            password.Append(_registerPassword);
            _registerPassword = password.ToString();
            _networkManager.Register(_registerUsername, _email, _registerPassword);
            ClearSignUpForm();
        }
    }

    public void GetRankingsFromDB() {
        _networkManager.GetRankings();
    }

    public void ClearSignUpForm() {
        usernameError.SetText("");
        _registerUsername = null;
        emailError.SetText("");
        _email = null;
        passwordError.SetText("");
        _registerPassword = null;
        passwordConfirmError.SetText("");
        _passwordConfirm = null;
    }

    private void ClearHash() {
        _hash = new Hash128();
        _hash.Append(27);
        _hash.Append(19.0f);
        _hash.Append("Hello");
        _hash.Append(new int[] { 1, 2, 3, 4, 5 });
    }
}