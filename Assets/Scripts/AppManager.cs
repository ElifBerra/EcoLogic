using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AppManager : MonoBehaviour
{
    [Header("Buton References")]
    public Button InformationCardButton;
    public Button PastBillsButton;

    [Header("Scene Names")]
    public string appScene = "AppScene";
    public string pastBillsScene = "PastBillsScene";



    void Start()
    {
        if (InformationCardButton != null)
        {
            InformationCardButton.onClick.AddListener(SkipToAppScene);
        }

        if (PastBillsButton != null)
        {
            PastBillsButton.onClick.AddListener(SkipToPastBillsScene);
        }
    }

    public void SkipToAppScene()
    {
        Debug.Log("Skip To App Scene");
        SceneManager.LoadScene(appScene);
    }

    public void SkipToPastBillsScene()
    {
        Debug.Log("Skip To Past Bills Scene");
        SceneManager.LoadScene(pastBillsScene);
    }


    // Sahne ge�i�i �ncesi veri kaydetme
    public void VeriKaydetVeGec(string hedefSahne)
    {
        // Mevcut sahne bilgisini kaydet
        PlayerPrefs.SetString("OncekiSahne", SceneManager.GetActiveScene().name);

        // Di�er verilerinizi burada kaydedin
        // PlayerPrefs.SetString("KullaniciID", kullaniciID);

        PlayerPrefs.Save();

        // Sahneye ge�
        SceneManager.LoadScene(hedefSahne);
    }

}
