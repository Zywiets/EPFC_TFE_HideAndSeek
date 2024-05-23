using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MenuManager : MonoBehaviour
{
   [SerializeField] private GameObject signInPanel;
   [SerializeField] private GameObject optionsMenuPanel;
   [SerializeField] private GameObject registerPanel;
   [SerializeField] private GameObject rankingsPanel;
   
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
   
   private NetworkManager _networkManager;

   private void Start()
   {
      GetNetworkManager();
      SetPanel(signInPanel);
   }

   private void GetNetworkManager()
   {
      GameObject networkManagerObject = GameObject.Find("Network Manager");
      if (networkManagerObject)
      {
         Debug.Log("--------- networkManagerObject");
         _networkManager = networkManagerObject.GetComponent<NetworkManager>();
         if (_networkManager == null)
         {
            Debug.Log("Le _networkManger est null ");
         }
      }
   }

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

   public void SetSignInPanel()
   {
      SetPanel(signInPanel);
      _networkManager.SignOut();
   }

   public void SetRankingsPanel()
   {
      SetPanel(rankingsPanel);
   }


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
      _networkManager.CheckSignIn(_signInUsername, _signInPassword);
      _isCheckingUserPass = true;
   }

   public void SetSignInErrorMessage()
   {
      _isCheckingUserPass = false;
      signInError.SetText(SignInErrorMessage);
   }
   
   public void IsValidUsername(string word)
   {
      if (word.Length < 3 || word.Length > 25)
      {
         _registerUsername = null;
         usernameError.SetText(SignUpUsernameError);
         Debug.Log(word + " is shorter than 3 or longer than 25");
      }
      else
      {
         _registerUsername = word;
         usernameError.SetText("");
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

      if (!Regex.IsMatch(word, "[A,Z]"))
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

   public void SendSignUpForm()
   {
      Debug.Log("--------- signUpForm");
      if (_registerUsername?.Length > 0 && _email?.Length > 0 && _registerPassword?.Length > 0 && _passwordConfirm?.Length > 0)
      {
         if (_networkManager)
         {
            _networkManager.SendFormToDB(_registerUsername, _email, _registerPassword);
         }
         else
         {
            Debug.Log("Le _networkManger est null ");
         }
      }
   }
}
