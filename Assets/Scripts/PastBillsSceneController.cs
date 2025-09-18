using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PastBillsSceneController : MonoBehaviour
{
    [Header("Past Bills Scene Buttons")]
    public Button backToAppButton;

    void Start()
    {
        if (backToAppButton != null)
        {
            backToAppButton.onClick.AddListener(BackToAppScene);
        }

    }

    public void BackToAppScene()
    {
        Debug.Log("Back To AppScene...");
        SceneManager.LoadScene("AppScene");
    }


    void OnDestroy()
    {
        if (backToAppButton != null)
        {
            backToAppButton.onClick.RemoveListener(BackToAppScene);
        }

    }
}