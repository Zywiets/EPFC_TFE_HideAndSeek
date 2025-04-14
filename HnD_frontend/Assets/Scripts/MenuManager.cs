using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
   [SerializeField] private GameObject signInPanel;
   [SerializeField] private GameObject optionsMenuPanel;
   [SerializeField] private GameObject registerPanel;
   [SerializeField] private GameObject rankingsPanel;
   [SerializeField] private GameObject lobbyPanel;
   [SerializeField] private GameObject endGameScorePanel;
   [SerializeField] private GameObject hostsPanel;
   
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
   private const string SignUpPasswordError = "Must be longer than 3, shorter than 25, contain a special character and an uppercase";
   [SerializeField] private TMP_Text passwordConfirmError;
   private const string SignUpPasswordConfirm = "Confirm Password be the same as the password";

   private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";
   private Hash128 _hash;

   private Dictionary<string,GameObject> _lobbyEntries = new Dictionary<string,GameObject>();
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

   private void Start()
   {
      ClearHash();
      GetNetworkManager();
      SetPanel(signInPanel);
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
   #region Panel Setting
   private void SetPanel(GameObject panel)
   {
      if(_currentPanel){ _currentPanel.SetActive(false);}
      panel.SetActive(true);
      _currentPanel = panel;
   }

   public void SetOptionMenuPanel()
   {
      SetPanel(optionsMenuPanel);
   }

   public void SetRegisterPanel()
   {
      SetPanel(registerPanel);
   }

   public void SetHostsPanel()
   {
      SetPanel(hostsPanel);
   }

   public void SetSignInPanel()
   {
      SetPanel(signInPanel);
      _networkManager.SignOut();
   }

   public void SetLobbyPanel()
   {
      SetPanel(lobbyPanel);
   }

   public void SetEndGamePanel()
   {
      SetPanel(endGameScorePanel);
   }

   public void SetRankingsPanel()
   {
      SetPanel(rankingsPanel);
   }
   
   #endregion
   
   #region Sign In and Sign Up Logic
   public void SetSignInUsername(string u)
   {
      _signInUsername = u;
   }

   public void SetSignInPassword(string p)
   {
      _signInPassword = p;
   }

   public void CheckUsernamePassword()
   {
      if (_isCheckingUserPass) return;
      if(string.IsNullOrWhiteSpace(_signInUsername) || string.IsNullOrWhiteSpace(_signInPassword))
      {
         SetSignInErrorMessage();
         return;
      }
      _hash.Append(_signInPassword);
      _signInPassword = _hash.ToString();
      _networkManager.CheckSignIn(_signInUsername, _signInPassword);
      _isCheckingUserPass = true;
      ClearHash();
   }

   public void SetSignInErrorMessage()
   {
      _isCheckingUserPass = false;
      signInError.SetText(SignInErrorMessage);
   }

   public void DisableSignInErrorMessage()
   {
      _isCheckingUserPass = false;
      signInError.SetText(string.Empty);
      _signInUsername = string.Empty;
      _signInPassword = string.Empty;
   }
   
   public void IsValidUsername(string word)
   {
      Debug.Log("the username for signup is : "+word);
      if (word.Length < 3 || word.Length > 25)
      {
         _registerUsername = null;
         usernameError.SetText(SignUpUsernameError);
         Debug.Log(word + "  \n "+ SignUpUsernameError);
      }
      else
      {
         _registerUsername = word;
         usernameError.SetText(" ");
      }
   }
   public void IsValidEmail(string word)
   {
      if (Regex.IsMatch(word, EmailPattern))
      {
         _email = word;
         emailError.SetText("");
      }
      else
      {
         _email = null;
         Debug.Log(word+"Is not a valid email");
         emailError.SetText(SignUpEmailError);
      }
   }
   public void IsValidPassword(string word)
   {
      bool isValid = true;
      if (word.Length < 3 || word.Length > 25)
      {
         isValid = false;
         Debug.Log(word + " is shorter than 3 or longer than 25");
      }

      if (!Regex.IsMatch(word, "[A-Z]"))
      {
         isValid = false;
         Debug.Log(word+"doesn't have an uppercase");
      }

      if (!Regex.IsMatch(word, "[!@#$%^&*()_+}{\":;\'?/>,<]"))
      {
         isValid = false;
         Debug.Log(word + "doesn't have a special char");
      }
      
      if (isValid)
      {
         _registerPassword = word;
         passwordError.SetText("");
      }
      else
      {
         _registerPassword = null;
         passwordError.SetText(SignUpPasswordError);
      }
      
   }
   public void IsValidConfirmPassword(string word)
   {
      if (word == _registerPassword && word != null)
      {
         passwordConfirmError.SetText("");
         _passwordConfirm = word;
      }
      else
      {
         passwordConfirmError.SetText(SignUpPasswordConfirm);
         _passwordConfirm = null;
         Debug.Log("Passwords don't match");
      }
   }
   #endregion
   public void AddToEndGameScore(List<NetworkManager.ScoreJson> entries)
   {
      entries.Sort((a, b) => b.score.CompareTo(a.score));

      int rank = 1;
      for (int i = 0; i < entries.Count; ++i)
      {
         var scoreValues = entries[i];
         if (i > 0 && entries[i].score != entries[i - 1].score)
         {
            rank = i + 1; 
         }
         GameObject scoreEntry = Instantiate(endGameScoreTemplate, endGameScoreContainer);
         var modifyScoreEntry = scoreEntry.GetComponent<RankingEntryModifier>();
         modifyScoreEntry.AddValues(rank.ToString(), scoreValues.name, scoreValues.score.ToString());
         var prevPos = scoreEntry.transform.position;
         scoreEntry.transform.position = new Vector3(prevPos.x, prevPos.y - 50.0f * i);
         _endGameEntries.Add(scoreEntry);
      }
   }
   
   
   public void DeleteAllFromEndGameScore()
   {
      var entriesToRemove = new List<GameObject>(_endGameEntries);
      foreach (var entry in entriesToRemove)
      {
         _endGameEntries.Remove(entry);
         Destroy(entry);
      }
   }

   public void AddToHostsLobby(string host)
   {
      if (_hostLobbyEntries.ContainsKey(host)) return;
      Debug.Log("Adding this to the Host lobby  " + host);
      GameObject lobbyHost = Instantiate(lobbyHostEntryTemplate, lobbyHostContentContainer, false);
      RankingEntryModifier rankEnt = lobbyHost.GetComponent<RankingEntryModifier>();
      rankEnt.AddValues(host);
      lobbyHost.gameObject.SetActive(true);
      _hostLobbyEntries.Add(host, lobbyHost);
   }
   public void AddToLobby(string player)
   {
      if (_lobbyEntries.ContainsKey(player)) return; 
      GameObject entry = Instantiate(lobbyEntryTemplate, lobbyContentContainer, false);
      Debug.Log("Added the user to the lobby : "+ player + "    *************");
      RankingEntryModifier rankEnt = entry.GetComponent<RankingEntryModifier>();
      rankEnt.AddValues(player);
      entry.gameObject.SetActive(true);
      _lobbyEntries.Add(player, entry);
   }

   public void SendMessageToDeleteHost()
   {
      _networkManager.SendMessageToDeleteHost();
   }
   public void DeleteHostFromHostsLobby(string host)
   {
      //if (!_hostLobbyEntries.ContainsKey(host)) return;
      Debug.Log("Deleting the fuck out of this shit °°°°°°°°°°°°  "+host);
      GameObject entry = _hostLobbyEntries[host];
      Destroy(entry);
      _hostLobbyEntries.Remove(host);
      Debug.Log("the number of host is "+_hostLobbyEntries.Count);
   }
   public void AddCurrentUserToLobby()
   {
      // GameObject entry = Instantiate(lobbyEntryTemplate, lobbyContentContainer, false);
      // Debug.Log("Added the Currentuser to the lobby : "+ _signInUsername + "    *************");
      // RankingEntryModifier lobEnt = entry.GetComponent<RankingEntryModifier>();
      // lobEnt.AddValues(_signInUsername);
      // _lobbyEntries.Add(_signInUsername, entry);
      // entry.gameObject.SetActive(true);
      //AddToLobby(_signInUsername);
   }
   public void SetPlayButton(bool active)
      {
         hostPlayButton.SetActive(active);
      }
   public void DeleteUserFromLobby(string player)
   {
      GameObject entry = _lobbyEntries[player];
      Destroy(entry);
      _lobbyEntries.Remove(player);
   }

   public void DeleteAllFromLobby()
   {
      List<string> playersToRemove = new List<string>(_lobbyEntries.Keys);
      // Debug.Log("players to remove "+playersToRemove.Count);
      foreach (var entry in playersToRemove)
      {
         DeleteUserFromLobby(entry);
      }
      _lobbyEntries.Clear();
   }
   

   public void SendSignUpForm()
   {
      if (_registerUsername?.Length > 0 && _email?.Length > 0 && _registerPassword?.Length > 0 && _passwordConfirm?.Length > 0)
      {
         var password = _hash;
         password.Append(_registerPassword);
//            _hash.Append(_registerPassword);
         _registerPassword = password.ToString();
  //          _registerPassword = _hash.ToString();
            _networkManager.SendFormToDB(_registerUsername, _email, _registerPassword);
            ClearHash();
            ClearSignUpForm();
      }
   }

   public void GetRankingsFromDB()
   {
      _networkManager.GetAllRankings();
   }

   public void ClearSignUpForm()
   {
      usernameError.SetText("");
      _registerUsername = null;
      emailError.SetText("");
      _email = null;
      passwordError.SetText("");
      _registerPassword = null;
      passwordConfirmError.SetText("");
      _passwordConfirm = null;
   }
   private void ClearHash()
   {
      _hash = new Hash128();
      _hash.Append(27);
      _hash.Append(19.0f);
      _hash.Append("Hello");
      _hash.Append(new int[] {1, 2, 3, 4, 5});
   }
}
