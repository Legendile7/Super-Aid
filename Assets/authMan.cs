using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using System.Net.Mail;
using VoxelBusters.CoreLibrary;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class authMan : MonoBehaviour
{
    public TMP_InputField emailText, passwordText, fullnameText;
    public TMP_InputField loginEmailText, loginPasswordText;
    public Toggle eighteen;
    public Button SignUpButton, LoginButton;

    public GameObject errorBox;
    public TMP_Text errorText;

    private FirebaseAuth auth;

    private string fullName, email, password;

    Firebase.Auth.FirebaseUser user;
    void Start()
    {
        // Initialize Firebase Auth
        auth = FirebaseAuth.DefaultInstance;
    }
    public void Update()
    {
        if (emailText.text != "" && passwordText.text != "" && fullnameText.text != "" && eighteen.isOn)
        {
            SignUpButton.interactable = true;
        }
        else
        {
            SignUpButton.interactable = false;
        }
        if (loginEmailText.text != "" && loginPasswordText.text != "")
        {
            LoginButton.interactable = true;
        }
        else
        {
            LoginButton.interactable = false;
        }
    }

    public void SignUp()
    {
        AuthResult result = new AuthResult();
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
            UserProfile profile = new UserProfile
            {
                DisplayName = fullName,
            };
            result.User.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
            });
        });
        PlayerPrefs.SetString("SavedName", auth.CurrentUser.DisplayName);
        PlayerPrefs.SetString("UID", auth.CurrentUser.UserId);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void Login()
    {
        AuthResult result = new AuthResult();
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
        PlayerPrefs.SetString("SavedName", auth.CurrentUser.DisplayName);
        PlayerPrefs.SetString("UID", auth.CurrentUser.UserId);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void SetEmail(string email)
    {
        this.email = email;
    }
    public void SetPassword(string password)
    {
        this.password = password;
    }
    public void SetName(string name)
    {
        this.fullName = name;
    }
}
