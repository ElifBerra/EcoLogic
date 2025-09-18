using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// StartingScene sahnesindeki GameManager objesine ekleyin
public class StartingSceneController : MonoBehaviour
{
    [Header("InformationCardButton")]
    public Button InformationCardButton;

    void Start()
    {
        if (InformationCardButton == null)
        {
            Debug.LogError("Can not found InformationCardButton!");
            return;
        }

        InformationCardButton.onClick.AddListener(SkiptoAppScene);
    }

    public void SkiptoAppScene()
    {
        Debug.Log("SkiptoAppScene...");
        SceneManager.LoadScene("appScene");
    }

    void OnDestroy()
    {
        if (InformationCardButton != null)
        {
            InformationCardButton.onClick.RemoveListener(SkiptoAppScene);
        }
    }
}
