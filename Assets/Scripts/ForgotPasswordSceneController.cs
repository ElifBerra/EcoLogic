using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Reflection;

/// <summary>
/// Þifremi Unuttum sahnesi kontrolcüsü.
/// UI referanslarýný Inspector'da baðla.
/// </summary>
public class ForgotPasswordSceneController : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField emailInput;

    [Header("Buttons")]
    public Button sendResetButton;
    public Button backButton;

    [Header("Feedback UI")]
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI errorText;
    public GameObject loadingSpinner; // opsiyonel

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
        if (sendResetButton != null) sendResetButton.onClick.AddListener(OnSendClicked);
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.LogIn));
    }

    private void OnDestroy()
    {
        if (sendResetButton != null) sendResetButton.onClick.RemoveListener(OnSendClicked);
    }

    private void HideMessages()
    {
        if (infoText != null) infoText.gameObject.SetActive(false);
        if (errorText != null) errorText.gameObject.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
    }

    private void SetUIInteractable(bool interactable)
    {
        if (sendResetButton != null) sendResetButton.interactable = interactable;
        if (backButton != null) backButton.interactable = interactable;
        if (emailInput != null) emailInput.interactable = interactable;
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

    public void OnSendClicked()
    {
        string email = emailInput != null ? emailInput.text.Trim() : "";

        if (string.IsNullOrEmpty(email) || !EmailRx.IsMatch(email))
        {
            SetError("Geçerli bir e-posta girin.");
            return;
        }

        HideMessages();
        SetUIInteractable(false);
        StartCoroutine(ForgotPasswordFlow(email));
    }

    private IEnumerator ForgotPasswordFlow(string email)
    {
        bool success = false;
        string message = null;

        var db = DatabaseManager.Instance;
        if (db != null)
        {
            // DatabaseManager.RequestPasswordReset(email, Action<bool,string> cb) var mý bak
            var method = db.GetType().GetMethod("RequestPasswordReset", BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                var enumerator = method.Invoke(db, new object[] {
                    email,
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
                    Debug.LogWarning("DatabaseManager.RequestPasswordReset bulundu ama IEnumerator döndürmüyor. Simülasyon kullanýlacak.");
                    yield return new WaitForSeconds(0.6f);
                    success = true;
                    message = "E-posta gönderildi (simülasyon).";
                }
            }
            else
            {
                // API yoksa simülasyon
                yield return new WaitForSeconds(0.6f);
                success = true;
                message = "E-posta gönderildi (simülasyon).";
            }
        }
        else
        {
            // DatabaseManager yoksa simülasyon
            yield return new WaitForSeconds(0.6f);
            success = true;
            message = "E-posta gönderildi (simülasyon).";
        }

        if (success)
        {
            SetInfo(!string.IsNullOrEmpty(message) ? message : "E-posta adresinize þifre sýfýrlama linki gönderildi.");
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene(SceneNames.LogIn);
        }
        else
        {
            SetError(!string.IsNullOrEmpty(message) ? message : "Ýþlem baþarýsýz. Lütfen tekrar deneyin.");
            SetUIInteractable(true);
        }
    }
}
