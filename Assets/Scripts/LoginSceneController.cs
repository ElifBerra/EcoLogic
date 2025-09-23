using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LoginSceneController : MonoBehaviour
{
    [Header("Login UI Elements")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Toggle rememberMeToggle;
    public TextMeshProUGUI errorMessage;

    [Header("Navigation Buttons")]
    public Button registerButton;         // "Kaydolun"
    public Button forgotPasswordButton;   // "�ifremi Unuttum"

    // Ge�ici database - Daha sonra ger�ek database ile de�i�tirilecek
    private Dictionary<string, string> userDatabase = new Dictionary<string, string>()
    {
        {"admin", "admin123"},
        {"test", "test123"},
        {"user1", "password1"},
        {"ecologic", "ecologic2025"}
    };

    void Start()
    {
        InitializeUI();
        LoadSavedCredentials();
        SetupEventListeners();
    }

    private void InitializeUI()
    {
        if (errorMessage != null)
        {
            errorMessage.gameObject.SetActive(false);
        }

        // Password input'u password field yapma
        if (passwordInput != null)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            passwordInput.inputType = TMP_InputField.InputType.Password;
        }
    }

    private void LoadSavedCredentials()
    {
        bool rememberMe = PlayerPrefs.GetInt("RememberMe", 0) == 1;

        if (rememberMe && usernameInput != null && rememberMeToggle != null)
        {
            string savedUsername = PlayerPrefs.GetString("SavedUsername", "");
            string savedPassword = PlayerPrefs.GetString("SavedPassword", "");

            if (!string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
            {
                usernameInput.text = savedUsername;
                if (passwordInput != null)
                {
                    passwordInput.text = savedPassword;
                }
                rememberMeToggle.isOn = true;
            }
        }
    }

    private void SetupEventListeners()
    {
        // Giri�
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(LoginUser);
        }

        // Klavye Enter deste�i
        if (usernameInput != null)
        {
            usernameInput.onEndEdit.AddListener(delegate {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (passwordInput != null && string.IsNullOrEmpty(passwordInput.text))
                    {
                        passwordInput.Select();
                    }
                    else
                    {
                        LoginUser();
                    }
                }
            });
        }

        if (passwordInput != null)
        {
            passwordInput.onEndEdit.AddListener(delegate {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    LoginUser();
                }
            });
        }

        // --- Yeni: Navigasyon butonlar� ---
        if (registerButton != null)
        {
            registerButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(SceneNames.Register);
            });
        }

        if (forgotPasswordButton != null)
        {
            forgotPasswordButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(SceneNames.ForgotPassword);
            });
        }
    }

    public void LoginUser()
    {
        if (usernameInput == null || passwordInput == null)
        {
            ShowErrorMessage("Giri� alanlar� bulunamad�!");
            return;
        }

        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        // Bo� alan kontrolleri
        if (string.IsNullOrEmpty(username))
        {
            ShowErrorMessage("L�tfen kullan�c� ad�n�z� giriniz!");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowErrorMessage("L�tfen �ifrenizi giriniz!");
            return;
        }

        // Minimum karakter kontrolleri
        if (username.Length < 2)
        {
            ShowErrorMessage("Kullan�c� ad� en az 2 karakter olmal�d�r!");
            return;
        }

        if (password.Length < 6)
        {
            ShowErrorMessage("�ifre en az 6 karakter olmal�d�r!");
            return;
        }

        // Database kontrol� (ge�ici)
        if (ValidateUser(username, password))
        {
            // Ba�ar�l� giri�
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.SetInt("IsLoggedIn", 1);

            // Beni hat�rla i�lemi
            if (rememberMeToggle != null && rememberMeToggle.isOn)
            {
                PlayerPrefs.SetString("SavedUsername", username);
                PlayerPrefs.SetString("SavedPassword", password);
                PlayerPrefs.SetInt("RememberMe", 1);
                Debug.Log($"Kullan�c� giri� yapt� ve hat�rlanacak: {username}");
            }
            else
            {
                PlayerPrefs.DeleteKey("SavedUsername");
                PlayerPrefs.DeleteKey("SavedPassword");
                PlayerPrefs.SetInt("RememberMe", 0);
                Debug.Log($"Kullan�c� giri� yapt� (hat�rlanmayacak): {username}");
            }

            PlayerPrefs.Save();

            // StartingScene'e ge�
            SceneManager.LoadScene(SceneNames.Starting);
        }
        else
        {
            ShowErrorMessage("Kullan�c� ad� veya �ifre hatal�!");
        }
    }

    private bool ValidateUser(string username, string password)
    {
        // Ge�ici database kontrol� - Daha sonra ger�ek database sorgusu ile de�i�tirilecek
        return userDatabase.ContainsKey(username) && userDatabase[username] == password;
    }

    private void ShowErrorMessage(string message)
    {
        if (errorMessage != null)
        {
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);
            errorMessage.color = Color.red;

            // 3 saniye sonra hata mesaj�n� gizle
            CancelInvoke("HideErrorMessage");
            Invoke("HideErrorMessage", 3f);
        }
    }

    private void HideErrorMessage()
    {
        if (errorMessage != null)
        {
            errorMessage.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (loginButton != null)
        {
            loginButton.onClick.RemoveListener(LoginUser);
        }
        if (registerButton != null)
        {
            // Ayn� lambda�y� tekrar yaratmak yerine komple t�m listener�lar� kald�rmak g�venlidir
            registerButton.onClick.RemoveAllListeners();
        }
        if (forgotPasswordButton != null)
        {
            forgotPasswordButton.onClick.RemoveAllListeners();
        }
    }
}
