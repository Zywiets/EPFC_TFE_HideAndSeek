using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class SignInManager : MonoBehaviour
{
   private string _username = null;
   private string _email = null;
   private string _password = null;
   private string _passwordConfirm = null;

   public GameObject usernameError;
   public GameObject emailError;
   public GameObject passwordError;
   public GameObject passwordConfirmError;
   
   private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";
   
   private NetworkManager _networkManager;

   // public void IsValidUsername(string word)
   // {
   //    if (word.Length < 3 || word.Length > 25)
   //    {
   //       _username = null;
   //       usernameError.SetActive(true);
   //       Debug.Log(word + " is shorter than 3 or longer than 25");
   //    }
   //    else
   //    {
   //       _username = word;
   //       usernameError.SetActive(false);
   //    }
   // }
   // public void IsValidEmail(string word)
   // {
   //    if (Regex.IsMatch(word, EmailPattern))
   //    {
   //       _email = word;
   //       emailError.SetActive(false);
   //    }
   //    else
   //    {
   //       _email = null;
   //       Debug.Log(word+"Is not a valid email");
   //       emailError.SetActive(true);
   //    }
   // }
   // public void IsValidPassword(string word)
   // {
   //    bool isValid = true;
   //    if (word.Length < 3 || word.Length > 25)
   //    {
   //       isValid = false;
   //       Debug.Log(word + " is shorter than 3 or longer than 25");
   //    }
   //
   //    if (!Regex.IsMatch(word, "[A,Z]"))
   //    {
   //       isValid = false;
   //       Debug.Log(word+"doesn't have an uppercase");
   //    }
   //
   //    if (!Regex.IsMatch(word, "[!@#$%^&*()_+}{\":;\'?/>,<]"))
   //    {
   //       isValid = false;
   //       Debug.Log(word + "doesn't have a special char");
   //    }
   //    
   //    passwordError.SetActive(!isValid);
   //    
   //    if (isValid)
   //    {
   //       _password = word;
   //    }
   //    else
   //    {
   //       _password = null;
   //    }
   //    
   // }
   // public void IsValidConfirmPassword(string word)
   // {
   //    if (word == _password && word != null)
   //    {
   //       passwordConfirmError.SetActive(false);
   //       _passwordConfirm = word;
   //    }
   //    else
   //    {
   //       passwordConfirmError.SetActive(true);
   //       _passwordConfirm = null;
   //       Debug.Log("Passwords don't match");
   //    }
   // }
   //
   // public void SendSignInForm()
   // {
   //    Debug.Log("--------- signInForm");
   //    if (_username?.Length > 0 && _email?.Length > 0 && _password?.Length > 0 && _passwordConfirm?.Length > 0)
   //    {
   //       GameObject networkManagerObject = GameObject.Find("Network Manager");
   //       if (networkManagerObject != null)
   //       {
   //          Debug.Log("--------- networkManagerObject");
   //          _networkManager = networkManagerObject.GetComponent<NetworkManager>();
   //          if (_networkManager != null)
   //          {
   //             _networkManager.SendFormToDB(_username, _email, _password);
   //          }
   //          else
   //          {
   //             Debug.Log("Le _networkManger est null ");
   //          }
   //       }
   //    }
   // }
   
   
}
