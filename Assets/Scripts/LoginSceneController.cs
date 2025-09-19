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

    // Geçici database - Daha sonra gerçek database ile deðiþtirilecek
    private Dictionary<string, string> userDatabase = new Dictionary<string, string>()
    {
        {"admin", "admin123"},
        {"test", "test123"},
        {"user1", "password1"},
        {"ecologic", "ecologic2024"}
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
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(LoginUser);
        }

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
    }

    public void LoginUser()
    {
        if (usernameInput == null || passwordInput == null)
        {
            ShowErrorMessage("Giriþ alanlarý bulunamadý!");
            return;
        }

        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        // Boþ alan kontrolleri
        if (string.IsNullOrEmpty(username))
        {
            ShowErrorMessage("Lütfen kullanýcý adýnýzý giriniz!");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowErrorMessage("Lütfen þifrenizi giriniz!");
            return;
        }

        // Minimum karakter kontrolleri
        if (username.Length < 2)
        {
            ShowErrorMessage("Kullanýcý adý en az 2 karakter olmalýdýr!");
            return;
        }

        if (password.Length < 6)
        {
            ShowErrorMessage("Þifre en az 6 karakter olmalýdýr!");
            return;
        }

        // Database kontrolü
        if (ValidateUser(username, password))
        {
            // Baþarýlý giriþ
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.SetInt("IsLoggedIn", 1);

            // Beni hatýrla iþlemi
            if (rememberMeToggle != null && rememberMeToggle.isOn)
            {
                PlayerPrefs.SetString("SavedUsername", username);
                PlayerPrefs.SetString("SavedPassword", password);
                PlayerPrefs.SetInt("RememberMe", 1);
                Debug.Log($"Kullanýcý giriþ yaptý ve hatýrlanacak: {username}");
            }
            else
            {
                PlayerPrefs.DeleteKey("SavedUsername");
                PlayerPrefs.DeleteKey("SavedPassword");
                PlayerPrefs.SetInt("RememberMe", 0);
                Debug.Log($"Kullanýcý giriþ yaptý (hatýrlanmayacak): {username}");
            }

            PlayerPrefs.Save();

            // StartingScene'e geç
            SceneManager.LoadScene("StartingScene");
        }
        else
        {
            ShowErrorMessage("Kullanýcý adý veya þifre hatalý!");
        }
    }

    private bool ValidateUser(string username, string password)
    {
        // Geçici database kontrolü - Daha sonra gerçek database sorgusu ile deðiþtirilecek
        return userDatabase.ContainsKey(username) && userDatabase[username] == password;
    }

    // Gerçek database entegrasyonu için hazýr method
    // private IEnumerator ValidateUserFromDatabase(string username, string password)
    // {
    //     // Web API çaðrýsý burada yapýlacak
    //     // yield return new WaitForSeconds(1f); // API yanýt bekleme simülasyonu
    //     // return apiResponse.isValid;
    // }

    private void ShowErrorMessage(string message)
    {
        if (errorMessage != null)
        {
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);
            errorMessage.color = Color.red;

            // 3 saniye sonra hata mesajýný gizle
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
    }
}