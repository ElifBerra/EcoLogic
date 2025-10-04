using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StartingSceneController : MonoBehaviour
{
    [Header("Information Card")]
    public Button informationCardButton;
    public TextMeshProUGUI informationText;
    public TextMeshProUGUI welcomeText;

    [Header("Navigation Buttons")]
    public Button logoutButton;

    // Geçici information data - Database'den gelecek
    private string[] informationMessages = {
        //"EcoLogic ile faturalarýnýzý kolayca tarayýn ve analiz edin!",
        //"Enerji tüketiminizi takip edin, tasarruf edin.",
        //"AI destekli fatura analizi ile harcamalarýnýzý optimize edin.",
        //"Çevre dostu yaþam için enerji verimliliði önerilerimizi keþfedin.",
        //"Geçmiþ faturalarýnýzý karþýlaþtýrýn ve trend analizinizi görün."
        "?? Hatýrlatma: Kullandýðýnýz elektriðin büyük kýsmý kömür ve doðalgazdan geliyor; temiz bir gelecek için yenilenebilir enerjiye yönelme zamaný!"
    };

    void Start()
    {
        CheckUserAuthentication();
        InitializeUI();
        SetupEventListeners();
        LoadInformationFromDatabase();
    }

    private void CheckUserAuthentication()
    {
        if (!GameManager.IsUserLoggedIn())
        {
            Debug.Log("Kullanýcý giriþ yapmamýþ, login ekranýna yönlendiriliyor...");
            SceneManager.LoadScene("LogInScene");
            return;
        }
    }

    private void InitializeUI()
    {
        // Hoþgeldin mesajýný göster
        if (welcomeText != null)
        {
            string username = GameManager.GetUsername();
            welcomeText.text = $"Hoþ geldiniz, {username}!";
        }

        // Information card kontrolleri
        if (informationCardButton == null)
        {
            Debug.LogError("InformationCardButton bulunamadý!");
        }
    }

    private void SetupEventListeners()
    {
        if (informationCardButton != null)
        {
            informationCardButton.onClick.AddListener(OnInformationCardClicked);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(LogoutUser);
        }
    }

    private void LoadInformationFromDatabase()
    {
        // Geçici bilgi gösterimi - Database entegrasyonu sonrasý deðiþtirilecek
        if (informationText != null)
        {
            int randomIndex = Random.Range(0, informationMessages.Length);
            informationText.text = informationMessages[randomIndex];
        }
    }

    public void OnInformationCardClicked()
    {
        Debug.Log("Information Card'a basýldý, AppScene'e geçiliyor...");
        SceneManager.LoadScene("AppScene");
    }

    public void LogoutUser()
    {
        Debug.Log("Kullanýcý çýkýþ yapýyor...");
        GameManager.Logout();
    }

    // Database entegrasyonu için hazýr method
    // private IEnumerator LoadInformationFromDatabaseCoroutine()
    // {
    //     // Web API çaðrýsý burada yapýlacak
    //     string apiUrl = "https://yourapi.com/getInformation";
    //     
    //     using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
    //     {
    //         yield return webRequest.SendWebRequest();
    //         
    //         if (webRequest.result == UnityWebRequest.Result.Success)
    //         {
    //             string jsonResponse = webRequest.downloadHandler.text;
    //             InformationData data = JsonUtility.FromJson<InformationData>(jsonResponse);
    //             
    //             if (informationText != null)
    //             {
    //                 informationText.text = data.message;
    //             }
    //         }
    //         else
    //         {
    //             Debug.LogError("API çaðrýsýnda hata: " + webRequest.error);
    //             // Fallback message
    //             if (informationText != null)
    //             {
    //                 informationText.text = "Bilgiler yüklenirken bir hata oluþtu.";
    //             }
    //         }
    //     }
    // }

    void OnDestroy()
    {
        if (informationCardButton != null)
        {
            informationCardButton.onClick.RemoveListener(OnInformationCardClicked);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveListener(LogoutUser);
        }
    }
}

// Database'den gelecek veri yapýsý için örnek class
[System.Serializable]
public class InformationData
{
    public string message;
    public string title;
    public string imageUrl;
    public string date;
}