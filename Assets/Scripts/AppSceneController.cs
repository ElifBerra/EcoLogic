using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AppSceneController : MonoBehaviour
{
    [Header("AppScene Buttons")]
    public Button pastBillsScene;
    public Button BackButton; 

    void Start()
    {
        if (pastBillsScene == null)
        {
            Debug.LogError("Can not found pastBillsScene!");
        }
        else
        {
            pastBillsScene.onClick.AddListener(SkipToPastBillsScene);
        }

        if (BackButton != null)
        {
            BackButton.onClick.AddListener(SkipToStartingScene);
        }
    }

    public void SkipToPastBillsScene()
    {
        Debug.Log("Skip To Past BillsScene...");
        SceneManager.LoadScene("pastBillsScene");
    }

    public void SkipToStartingScene()
    {
        Debug.Log("Skip To Starting Scene...");
        SceneManager.LoadScene("startingScene");
    }

    void OnDestroy()
    {
        if (pastBillsScene != null)
        {
            pastBillsScene.onClick.RemoveListener(SkipToPastBillsScene);
        }

        if (BackButton != null)
        {
            BackButton.onClick.RemoveListener(SkipToStartingScene);
        }
    }
}