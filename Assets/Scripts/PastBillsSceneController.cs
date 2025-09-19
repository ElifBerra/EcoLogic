using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PastBillsSceneController : MonoBehaviour
{
    [Header("Past Bills UI")]
    public Button backToAppButton;
    public ScrollRect billsScrollView;
    public GameObject billItemPrefab;
    public Transform billsContainer;
    public TextMeshProUGUI noBillsText;

    [Header("Bill Details Panel")]
    public GameObject billDetailsPanel;
    public TextMeshProUGUI billDetailsText;
    public Button closeBillDetailsButton;

    private List<BillData> pastBills = new List<BillData>();

    void Start()
    {
        InitializeUI();
        // TEST VER�S� EKLEME(Sadece test i�in -sonra silinecek)
        AddTestDataIfNeeded();
        LoadPastBills();
        SetupEventListeners();
    }

    private void AddTestDataIfNeeded()
    {
        // E�er hi� veri yoksa test verisi ekle
        string existingData = PlayerPrefs.GetString("ScannedBills", "");

        if (string.IsNullOrEmpty(existingData))
        {
            string testBills = "15/11/2024 14:30|Elektrik Faturas� - Kas�m 2024\nTutar: 245.67 TL\nT�ketim: 380 kWh\nAI �nerisi: %15 tasarruf potansiyeli tespit edildi.###" +
                              "10/11/2024 09:15|Su Faturas� - Kas�m 2024\nTutar: 89.50 TL\nT�ketim: 25 m�\nAI �nerisi: Normal t�ketim seviyesi.###" +
                              "05/11/2024 16:45|Do�algaz Faturas� - Kas�m 2024\nTutar: 156.30 TL\nT�ketim: 45 m�\nAI �nerisi: Yal�t�m iyile�tirmesi �nerilir.";

            PlayerPrefs.SetString("ScannedBills", testBills);
            PlayerPrefs.Save();

            Debug.Log("Test fatura verileri eklendi!");
        }
    }

    private void InitializeUI()
    {
        if (billDetailsPanel != null)
        {
            billDetailsPanel.SetActive(false);
        }

        if (noBillsText != null)
        {
            noBillsText.gameObject.SetActive(false);
        }
    }

    private void SetupEventListeners()
    {
        if (backToAppButton != null)
        {
            backToAppButton.onClick.AddListener(BackToAppScene);
        }

        if (closeBillDetailsButton != null)
        {
            closeBillDetailsButton.onClick.AddListener(CloseBillDetails);
        }
    }

    private void LoadPastBills()
    {
        // PlayerPrefs'den kay�tl� faturalar� y�kle (ge�ici)
        string savedBills = PlayerPrefs.GetString("ScannedBills", "");

        if (string.IsNullOrEmpty(savedBills))
        {
            ShowNoBillsMessage();
            return;
        }

        // Fatura verilerini parse et
        string[] billEntries = savedBills.Split(new string[] { "###" }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string entry in billEntries)
        {
            string[] parts = entry.Split('|');
            if (parts.Length >= 2)
            {
                BillData billData = new BillData
                {
                    date = parts[0],
                    analysisResult = parts[1],
                    id = System.Guid.NewGuid().ToString()
                };

                pastBills.Add(billData);
            }
        }

        // Tarihe g�re s�rala (en yeni �nce)
        pastBills = pastBills.OrderByDescending(b => System.DateTime.Parse(b.date)).ToList();

        if (pastBills.Count == 0)
        {
            ShowNoBillsMessage();
        }
        else
        {
            DisplayBills();
        }
    }

    private void ShowNoBillsMessage()
    {
        if (noBillsText != null)
        {
            noBillsText.text = "Hen�z taranm�� fatura bulunmamaktad�r.\n\nFatura taramak i�in Ana Ekran'dan 'Fatura Okut' se�ene�ini kullanabilirsiniz.";
            noBillsText.gameObject.SetActive(true);
        }
    }

    private void DisplayBills()
    {
        if (billsContainer == null || billItemPrefab == null)
        {
            Debug.LogError("Bills container veya bill item prefab bulunamad�!");
            return;
        }

        // �nceki bill item'lar� temizle
        foreach (Transform child in billsContainer)
        {
            Destroy(child.gameObject);
        }

        // Her fatura i�in bir item olu�tur
        foreach (BillData bill in pastBills)
        {
            GameObject billItem = Instantiate(billItemPrefab, billsContainer);
            SetupBillItem(billItem, bill);
        }

        if (noBillsText != null)
        {
            noBillsText.gameObject.SetActive(false);
        }
    }

    private void SetupBillItem(GameObject billItem, BillData billData)
    {
        // Bill item UI bile�enlerini bul ve ayarla
        TextMeshProUGUI dateText = billItem.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI summaryText = billItem.transform.Find("SummaryText")?.GetComponent<TextMeshProUGUI>();
        Button viewDetailsButton = billItem.transform.Find("ViewDetailsButton")?.GetComponent<Button>();
        Image billTypeIcon = billItem.transform.Find("BillTypeIcon")?.GetComponent<Image>();

        if (dateText != null)
        {
            dateText.text = billData.date;
        }

        if (summaryText != null)
        {
            // Analiz sonucundan �zet ��kar
            string[] lines = billData.analysisResult.Split('\n');
            string summary = lines.Length > 0 ? lines[0] : "Fatura Analizi";
            if (lines.Length > 1 && lines[1].Contains("Tutar:"))
            {
                summary += " - " + lines[1];
            }
            summaryText.text = summary;
        }

        if (viewDetailsButton != null)
        {
            viewDetailsButton.onClick.AddListener(() => ShowBillDetails(billData));
        }

        // Fatura tipine g�re ikon ayarla (gelecekte database'den gelecek)
        if (billTypeIcon != null)
        {
            // Default ikon rengi - fatura tipine g�re de�i�ecek
            if (billData.analysisResult.Contains("Elektrik"))
            {
                billTypeIcon.color = Color.yellow;
            }
            else if (billData.analysisResult.Contains("Su"))
            {
                billTypeIcon.color = Color.blue;
            }
            else if (billData.analysisResult.Contains("Do�algaz"))
            {
                billTypeIcon.color = Color.red;
            }
            else
            {
                billTypeIcon.color = Color.gray;
            }
        }
    }

    private void ShowBillDetails(BillData billData)
    {
        if (billDetailsPanel != null && billDetailsText != null)
        {
            billDetailsText.text = FormatBillDetails(billData);
            billDetailsPanel.SetActive(true);
        }
    }

    private string FormatBillDetails(BillData billData)
    {
        string formattedDetails = $"<b>Tarih:</b> {billData.date}\n\n";
        formattedDetails += $"<b>Analiz Sonu�lar�:</b>\n{billData.analysisResult}\n\n";

        // AI �nerileri ekle
        formattedDetails += "<b>AI �nerileri ve Yorumlar:</b>\n";

        if (billData.analysisResult.Contains("Elektrik"))
        {
            formattedDetails += "� Elektrik t�ketiminizde art�� g�zlemlendi.\n";
            formattedDetails += "� LED ampul kullan�m�na ge�i� �nerilir.\n";
            formattedDetails += "� Cihazlar� standby modda b�rakmamaya dikkat edin.\n";
            formattedDetails += "� Enerji verimli A++ cihaz kullan�m� tavsiye edilir.\n\n";
        }
        else if (billData.analysisResult.Contains("Su"))
        {
            formattedDetails += "� Su t�ketiminiz normal seviyede.\n";
            formattedDetails += "� Du� s�renizi k�saltarak tasarruf sa�layabilirsiniz.\n";
            formattedDetails += "� Musluk ar�zalar�n� kontrol edin.\n\n";
        }
        else if (billData.analysisResult.Contains("Do�algaz"))
        {
            formattedDetails += "� Do�algaz t�ketiminiz mevsim ortalamas�nda.\n";
            formattedDetails += "� Ev yal�t�m�n� iyile�tirerek tasarruf sa�layabilirsiniz.\n";
            formattedDetails += "� Kombi bak�m�n� yapt�rmay� unutmay�n.\n\n";
        }

        formattedDetails += "<b>�evresel Etki:</b>\n";
        formattedDetails += "� Bu ay karbon ayak iziniz: ~125 kg CO2\n";
        formattedDetails += "� �nceki aya g�re %8 azalma\n";
        formattedDetails += "� Hedefinize %92 yak�ns�n�z\n\n";

        formattedDetails += "<color=green><b>EcoLogic Skoru: 7.2/10</b></color>";

        return formattedDetails;
    }

    public void CloseBillDetails()
    {
        if (billDetailsPanel != null)
        {
            billDetailsPanel.SetActive(false);
        }
    }

    public void BackToAppScene()
    {
        Debug.Log("AppScene'e geri d�n�l�yor...");
        SceneManager.LoadScene("AppScene");
    }

    // Database entegrasyonu i�in haz�r method
    // private IEnumerator LoadPastBillsFromDatabase()
    // {
    //     string username = GameManager.GetUsername();
    //     string apiUrl = $"https://yourapi.com/getPastBills?user={username}";
    //     
    //     using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
    //     {
    //         yield return webRequest.SendWebRequest();
    //         
    //         if (webRequest.result == UnityWebRequest.Result.Success)
    //         {
    //             string jsonResponse = webRequest.downloadHandler.text;
    //             PastBillsResponse response = JsonUtility.FromJson<PastBillsResponse>(jsonResponse);
    //             
    //             pastBills = response.bills.ToList();
    //             DisplayBills();
    //         }
    //         else
    //         {
    //             Debug.LogError("Ge�mi� faturalar y�klenirken hata: " + webRequest.error);
    //             ShowNoBillsMessage();
    //         }
    //     }
    // }

    void OnDestroy()
    {
        if (backToAppButton != null)
        {
            backToAppButton.onClick.RemoveListener(BackToAppScene);
        }

        if (closeBillDetailsButton != null)
        {
            closeBillDetailsButton.onClick.RemoveListener(CloseBillDetails);
        }
    }
}

// Fatura verisi i�in data class
[System.Serializable]
public class BillData
{
    public string id;
    public string date;
    public string analysisResult;
    public string billType;
    public float amount;
    public float consumption;
    public string aiSuggestions;
    public float ecoScore;
}

// Database API response i�in data structure
[System.Serializable]
public class PastBillsResponse
{
    public BillData[] bills;
    public int totalCount;
    public bool success;
}