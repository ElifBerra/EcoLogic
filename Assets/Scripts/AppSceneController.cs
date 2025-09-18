using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AppSceneController : MonoBehaviour
{
    [Header("AppScene Buttons")]
    public Button pastBillsButton;
    public Button scanBillButton;
    public Button backButton;

    void Start()
    {
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
    }
}