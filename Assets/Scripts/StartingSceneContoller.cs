using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartingSceneController : MonoBehaviour
{
    [Header("Information Card Button")]
    public Button InformationCardButton;

    void Start()
    {
        if (InformationCardButton == null)
        {
            Debug.LogError("InformationCardButton not found");
            return;
        }

        InformationCardButton.onClick.AddListener(SkipToAppScene);
    }

    public void SkipToAppScene()
    {
        Debug.Log("Skip To AppScene...");
        SceneManager.LoadScene("AppScene");
    }

    void OnDestroy()
    {
        if (InformationCardButton != null)
        {
            InformationCardButton.onClick.RemoveListener(SkipToAppScene);
        }
    }
}