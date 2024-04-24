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
   private GameObject _currentPanel;
   private string _signInUsername;
   private string _signInPassword;
   private bool _isCheckingUserPass;
   [SerializeField] private TMP_Text signInError;
   private string _signInErrorMessage = "Invalid username or password. Please try again";
   
   
   private string _registerUsername = null;
   private string _email = null;
   private string _registerPassword = null;
   private string _passwordConfirm = null;

   public GameObject usernameError;
   public GameObject emailError;
   public GameObject passwordError;
   public GameObject passwordConfirmError;
   
   
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
      if (_signInUsername?.Length > 3 && _signInPassword?.Length > 3 && !_isCheckingUserPass)
      {
         _networkManager.CheckSignIn(_signInUsername, _signInPassword);
         _isCheckingUserPass = true;
      }
   }

   public void SetSignInErrorMessage()
   {
      signInError.SetText(_signInErrorMessage);
   }
   
   public void IsValidUsername(string word)
   {
      if (word.Length < 3 || word.Length > 25)
      {
         _registerUsername = null;
         usernameError.SetActive(true);
         Debug.Log(word + " is shorter than 3 or longer than 25");
      }
      else
      {
         _registerUsername = word;
         usernameError.SetActive(false);
      }
   }
   public void IsValidEmail(string word)
   {
      if (Regex.IsMatch(word, EmailPattern))
      {
         _email = word;
         emailError.SetActive(false);
      }
      else
      {
         _email = null;
         Debug.Log(word+"Is not a valid email");
         emailError.SetActive(true);
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
      
      passwordError.SetActive(!isValid);
      
      if (isValid)
      {
         _registerPassword = word;
      }
      else
      {
         _registerPassword = null;
      }
      
   }
   public void IsValidConfirmPassword(string word)
   {
      if (word == _registerPassword && word != null)
      {
         passwordConfirmError.SetActive(false);
         _passwordConfirm = word;
      }
      else
      {
         passwordConfirmError.SetActive(true);
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
