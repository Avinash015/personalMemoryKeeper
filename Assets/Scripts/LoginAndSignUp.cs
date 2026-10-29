using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginAndSign : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button registerButton;
    public Button loginButton;
    public TMP_Text errorMessage; // Display error messages in UI

    private string baseURL = "http://192.168.29.142:8000/api/users/";
    private string loginURL => baseURL + "login";
    private string registerURL => baseURL + "register";
    public static string LoggedInUserEmail = "Anonymous";


    public class User
    {
        public string email;
        public string password;
    }

    public void OnLoginSubmitButtonClick()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (!ValidateInput(email, password))
            return;

        LoginUser(email, password);
    }

    public void LoginUser(string email, string password)
    {
        StartCoroutine(LoginUserHandler(new User { email = email, password = password }));
    }

    IEnumerator LoginUserHandler(User user)
    {
        string json = JsonUtility.ToJson(user);

        using (UnityWebRequest request = new UnityWebRequest(loginURL, "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.certificateHandler = new BypassCertificate();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                ShowError("Login Failed: " + request.error);
            }
            else
            {
                Debug.Log("Login Successful!");
                LoggedInUserEmail = user.email;
                SceneManager.LoadScene("startanimation"); // Redirect after successful login
            }
        }
    }

    public void OnRegisterSubmitButtonClick()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (!ValidateInput(email, password))
            return;

        RegisterUser(email, password);
    }

    public void RegisterUser(string email, string password)
    {
        StartCoroutine(RegisterUserHandler(new User { email = email, password = password }));
    }

    IEnumerator RegisterUserHandler(User user)
    {
        string json = JsonUtility.ToJson(user);

        using (UnityWebRequest request = new UnityWebRequest(registerURL, "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.certificateHandler = new BypassCertificate();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                ShowError("Registration Failed: " + request.error);
            }
            else
            {
                Debug.Log("User registered successfully!");
                LoggedInUserEmail = user.email;
                SceneManager.LoadScene("startanimation"); // Redirect after successful signup
            }
        }
    }

    private bool ValidateInput(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Email and password cannot be empty.");
            return false;
        }

        if (password.Length < 6)
        {
            ShowError("Password must be at least 6 characters long.");
            return false;
        }

        if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$"))
        {
            ShowError("Password must contain at least one uppercase letter, one lowercase letter, and one number.");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
    {
        if (errorMessage != null)
        {
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);
        }
        Debug.LogError(message);
    }

    private class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}
