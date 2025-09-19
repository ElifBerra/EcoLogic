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
        // TEST VERÝSÝ EKLEME(Sadece test için -sonra silinecek)
        AddTestDataIfNeeded();
        LoadPastBills();
        SetupEventListeners();
    }

    private void AddTestDataIfNeeded()
    {
        // Eðer hiç veri yoksa test verisi ekle
        string existingData = PlayerPrefs.GetString("ScannedBills", "");

        if (string.IsNullOrEmpty(existingData))
        {
            string testBills = "15/11/2024 14:30|Elektrik Faturasý - Kasým 2024\nTutar: 245.67 TL\nTüketim: 380 kWh\nAI Önerisi: %15 tasarruf potansiyeli tespit edildi.###" +
                              "10/11/2024 09:15|Su Faturasý - Kasým 2024\nTutar: 89.50 TL\nTüketim: 25 m³\nAI Önerisi: Normal tüketim seviyesi.###" +
                              "05/11/2024 16:45|Doðalgaz Faturasý - Kasým 2024\nTutar: 156.30 TL\nTüketim: 45 m³\nAI Önerisi: Yalýtým iyileþtirmesi önerilir.";

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
        // PlayerPrefs'den kayýtlý faturalarý yükle (geçici)
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

        // Tarihe göre sýrala (en yeni önce)
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
            noBillsText.text = "Henüz taranmýþ fatura bulunmamaktadýr.\n\nFatura taramak için Ana Ekran'dan 'Fatura Okut' seçeneðini kullanabilirsiniz.";
            noBillsText.gameObject.SetActive(true);
        }
    }

    private void DisplayBills()
    {
        if (billsContainer == null || billItemPrefab == null)
        {
            Debug.LogError("Bills container veya bill item prefab bulunamadý!");
            return;
        }

        // Önceki bill item'larý temizle
        foreach (Transform child in billsContainer)
        {
            Destroy(child.gameObject);
        }

        // Her fatura için bir item oluþtur
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
        // Bill item UI bileþenlerini bul ve ayarla
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
            // Analiz sonucundan özet çýkar
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

        // Fatura tipine göre ikon ayarla (gelecekte database'den gelecek)
        if (billTypeIcon != null)
        {
            // Default ikon rengi - fatura tipine göre deðiþecek
            if (billData.analysisResult.Contains("Elektrik"))
            {
                billTypeIcon.color = Color.yellow;
            }
            else if (billData.analysisResult.Contains("Su"))
            {
                billTypeIcon.color = Color.blue;
            }
            else if (billData.analysisResult.Contains("Doðalgaz"))
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
        formattedDetails += $"<b>Analiz Sonuçlarý:</b>\n{billData.analysisResult}\n\n";

        // AI önerileri ekle
        formattedDetails += "<b>AI Önerileri ve Yorumlar:</b>\n";

        if (billData.analysisResult.Contains("Elektrik"))
        {
            formattedDetails += "• Elektrik tüketiminizde artýþ gözlemlendi.\n";
            formattedDetails += "• LED ampul kullanýmýna geçiþ önerilir.\n";
            formattedDetails += "• Cihazlarý standby modda býrakmamaya dikkat edin.\n";
            formattedDetails += "• Enerji verimli A++ cihaz kullanýmý tavsiye edilir.\n\n";
        }
        else if (billData.analysisResult.Contains("Su"))
        {
            formattedDetails += "• Su tüketiminiz normal seviyede.\n";
            formattedDetails += "• Duþ sürenizi kýsaltarak tasarruf saðlayabilirsiniz.\n";
            formattedDetails += "• Musluk arýzalarýný kontrol edin.\n\n";
        }
        else if (billData.analysisResult.Contains("Doðalgaz"))
        {
            formattedDetails += "• Doðalgaz tüketiminiz mevsim ortalamasýnda.\n";
            formattedDetails += "• Ev yalýtýmýný iyileþtirerek tasarruf saðlayabilirsiniz.\n";
            formattedDetails += "• Kombi bakýmýný yaptýrmayý unutmayýn.\n\n";
        }

        formattedDetails += "<b>Çevresel Etki:</b>\n";
        formattedDetails += "• Bu ay karbon ayak iziniz: ~125 kg CO2\n";
        formattedDetails += "• Önceki aya göre %8 azalma\n";
        formattedDetails += "• Hedefinize %92 yakýnsýnýz\n\n";

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
        Debug.Log("AppScene'e geri dönülüyor...");
        SceneManager.LoadScene("AppScene");
    }

    // Database entegrasyonu için hazýr method
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
    //             Debug.LogError("Geçmiþ faturalar yüklenirken hata: " + webRequest.error);
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

// Fatura verisi için data class
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

// Database API response için data structure
[System.Serializable]
public class PastBillsResponse
{
    public BillData[] bills;
    public int totalCount;
    public bool success;
}