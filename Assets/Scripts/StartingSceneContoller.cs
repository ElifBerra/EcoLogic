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

    // Ge�ici information data - Database'den gelecek
    private string[] informationMessages = {
        //"EcoLogic ile faturalar�n�z� kolayca taray�n ve analiz edin!",
        //"Enerji t�ketiminizi takip edin, tasarruf edin.",
        //"AI destekli fatura analizi ile harcamalar�n�z� optimize edin.",
        //"�evre dostu ya�am i�in enerji verimlili�i �nerilerimizi ke�fedin.",
        //"Ge�mi� faturalar�n�z� kar��la�t�r�n ve trend analizinizi g�r�n."
        "?? Hat�rlatma: Kulland���n�z elektri�in b�y�k k�sm� k�m�r ve do�algazdan geliyor; temiz bir gelecek i�in yenilenebilir enerjiye y�nelme zaman�!"
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
            Debug.Log("Kullan�c� giri� yapmam��, login ekran�na y�nlendiriliyor...");
            SceneManager.LoadScene("LogInScene");
            return;
        }
    }

    private void InitializeUI()
    {
        // Ho�geldin mesaj�n� g�ster
        if (welcomeText != null)
        {
            string username = GameManager.GetUsername();
            welcomeText.text = $"Ho� geldiniz, {username}!";
        }

        // Information card kontrolleri
        if (informationCardButton == null)
        {
            Debug.LogError("InformationCardButton bulunamad�!");
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
        // Ge�ici bilgi g�sterimi - Database entegrasyonu sonras� de�i�tirilecek
        if (informationText != null)
        {
            int randomIndex = Random.Range(0, informationMessages.Length);
            informationText.text = informationMessages[randomIndex];
        }
    }

    public void OnInformationCardClicked()
    {
        Debug.Log("Information Card'a bas�ld�, AppScene'e ge�iliyor...");
        SceneManager.LoadScene("AppScene");
    }

    public void LogoutUser()
    {
        Debug.Log("Kullan�c� ��k�� yap�yor...");
        GameManager.Logout();
    }

    // Database entegrasyonu i�in haz�r method
    // private IEnumerator LoadInformationFromDatabaseCoroutine()
    // {
    //     // Web API �a�r�s� burada yap�lacak
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
    //             Debug.LogError("API �a�r�s�nda hata: " + webRequest.error);
    //             // Fallback message
    //             if (informationText != null)
    //             {
    //                 informationText.text = "Bilgiler y�klenirken bir hata olu�tu.";
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

// Database'den gelecek veri yap�s� i�in �rnek class
[System.Serializable]
public class InformationData
{
    public string message;
    public string title;
    public string imageUrl;
    public string date;
}