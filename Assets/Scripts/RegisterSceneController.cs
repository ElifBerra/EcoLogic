using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Reflection;

/// <summary>
/// Kaydol (Register) sahnesi kontrolcüsü.
/// UI referanslarýný Inspector'da baðla.
/// </summary>
public class RegisterSceneController : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;

    [Header("Buttons")]
    public Button registerButton;
    public Button backButton;

    [Header("Feedback UI")]
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI errorText;
    public GameObject loadingSpinner; // opsiyonel

    // Basit email kontrolü (UX için esnek)
    private static readonly Regex EmailRx = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private void Start()
    {
        HideMessages();
        SetupListeners();
    }

    private void SetupListeners()
    {
        if (registerButton != null) registerButton.onClick.AddListener(OnRegisterClicked);
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.LogIn));
    }

    private void OnDestroy()
    {
        if (registerButton != null) registerButton.onClick.RemoveListener(OnRegisterClicked);
    }

    private void HideMessages()
    {
        if (infoText != null) infoText.gameObject.SetActive(false);
        if (errorText != null) errorText.gameObject.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
    }

    private void SetUIInteractable(bool interactable)
    {
        if (registerButton != null) registerButton.interactable = interactable;
        if (backButton != null) backButton.interactable = interactable;
        if (usernameInput != null) usernameInput.interactable = interactable;
        if (emailInput != null) emailInput.interactable = interactable;
        if (passwordInput != null) passwordInput.interactable = interactable;
        if (confirmPasswordInput != null) confirmPasswordInput.interactable = interactable;
        if (loadingSpinner != null) loadingSpinner.SetActive(!interactable);
    }

    private void SetError(string msg)
    {
        if (errorText != null)
        {
            errorText.text = msg;
            errorText.gameObject.SetActive(true);
        }
        if (infoText != null) infoText.gameObject.SetActive(false);
    }

    private void SetInfo(string msg)
    {
        if (infoText != null)
        {
            infoText.text = msg;
            infoText.gameObject.SetActive(true);
        }
        if (errorText != null) errorText.gameObject.SetActive(false);
    }

    public void OnRegisterClicked()
    {
        string username = usernameInput != null ? usernameInput.text.Trim() : "";
        string email = emailInput != null ? emailInput.text.Trim() : "";
        string password = passwordInput != null ? passwordInput.text : "";
        string confirm = confirmPasswordInput != null ? confirmPasswordInput.text : "";

        // ---- Basit doðrulamalar ----
        if (string.IsNullOrEmpty(username) || username.Length < 3)
        {
            SetError("Kullanýcý adý en az 3 karakter olmalý.");
            return;
        }
        if (string.IsNullOrEmpty(email) || !EmailRx.IsMatch(email))
        {
            SetError("Geçerli bir e-posta girin.");
            return;
        }
        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            SetError("Þifre en az 6 karakter olmalý.");
            return;
        }
        if (password != confirm)
        {
            SetError("Þifre ve doðrulama eþleþmiyor.");
            return;
        }

        HideMessages();
        SetUIInteractable(false);
        StartCoroutine(RegisterFlow(username, email, password));
    }

    private IEnumerator RegisterFlow(string username, string email, string password)
    {
        bool success = false;
        string message = null;

        var db = DatabaseManager.Instance;
        if (db != null)
        {
            // DatabaseManager.RegisterUser(username, email, password, Action<bool,string> cb) var mý bak
            MethodInfo method = db.GetType().GetMethod("RegisterUser", BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                var enumerator = method.Invoke(db, new object[] {
                    username, email, password,
                    new Action<bool, string>((ok, msg) =>
                    {
                        success = ok;
                        message = msg;
                    })
                }) as IEnumerator;

                if (enumerator != null)
                {
                    yield return StartCoroutine(enumerator);
                }
                else
                {
                    Debug.LogWarning("DatabaseManager.RegisterUser bulundu ama IEnumerator döndürmüyor. Yerel stub kullanýlacak.");
                    yield return StartCoroutine(LocalRegisterStub(username, email, password));
                    success = true;
                    message = "Kayýt baþarýlý (yerel).";
                }
            }
            else
            {
                // API yoksa yerel kayýt simülasyonu
                yield return StartCoroutine(LocalRegisterStub(username, email, password));
                success = true;
                message = "Kayýt baþarýlý (yerel).";
            }
        }
        else
        {
            // DatabaseManager yoksa da simüle et
            yield return StartCoroutine(LocalRegisterStub(username, email, password));
            success = true;
            message = "Kayýt baþarýlý (yerel).";
        }

        if (success)
        {
            SetInfo(!string.IsNullOrEmpty(message) ? message : "Kayýt baþarýlý.");
            try { GameManager.SaveUserSession(username); } catch { }
            yield return new WaitForSeconds(0.6f);
            SceneManager.LoadScene(SceneNames.LogIn);
        }
        else
        {
            SetError(!string.IsNullOrEmpty(message) ? message : "Kayýt baþarýsýz.");
            SetUIInteractable(true);
        }
    }

    // Demo/Test amaçlý yerel kayýt (PlayerPrefs)
    private IEnumerator LocalRegisterStub(string username, string email, string password)
    {
        yield return new WaitForSeconds(0.5f);

        string key = $"user:{email}".ToLowerInvariant();
        if (PlayerPrefs.HasKey(key))
        {
            SetError("Bu e-posta ile kayýt zaten var.");
            yield break;
        }
        PlayerPrefs.SetString(key, $"{username}|{password}");
        PlayerPrefs.Save();
    }
}
