using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AppSceneController : MonoBehaviour
{
    [Header("AppScene Buttons")]
    public Button pastBillsButton;
    public Button scanBillButton;
    public Button backButton;
    public Button logoutButton;

    [Header("Welcome Text")]
    public TextMeshProUGUI welcomeText;

    void Start()
    {
        DisplayWelcomeMessage();

        if (pastBillsButton == null)
        {
            Debug.LogError("PastBillsButton not found!");
        }
        else
        {
            pastBillsButton.onClick.AddListener(SkipToPastBillsScene);
        }

        if (scanBillButton != null)
        {
            scanBillButton.onClick.AddListener(ScanBillAction);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(SkipToStartingScene);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(LogoutUser);
        }
    }

    private void DisplayWelcomeMessage()
    {
        if (welcomeText != null)
        {
            string username = PlayerPrefs.GetString("Username", "Kullanýcý");
            welcomeText.text = $"Hoþ geldiniz, {username}!";
        }
    }

    public void SkipToPastBillsScene()
    {
        Debug.Log("Skip To PastBillsScene...");
        SceneManager.LoadScene("PastBillsScene");
    }

    public void SkipToStartingScene()
    {
        Debug.Log("Back To StartingScene...");
        SceneManager.LoadScene("StartingScene");
    }

    public void ScanBillAction()
    {
        Debug.Log("Fatura tarama islemi baslatiliyor...");
        // Burada fatura tarama islemi kodlari olacak
        SceneManager.LoadScene("ScanBillsScene");
    }

    public void LogoutUser()
    {
        Debug.Log("User logging out...");
        GameManager.Logout();
    }

    void OnDestroy()
    {
        if (pastBillsButton != null)
        {
            pastBillsButton.onClick.RemoveListener(SkipToPastBillsScene);
        }

        if (scanBillButton != null)
        {
            scanBillButton.onClick.RemoveListener(ScanBillAction);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(SkipToStartingScene);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveListener(LogoutUser);
        }
    }
}