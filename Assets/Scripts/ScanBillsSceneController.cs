using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScanBillsSceneController : MonoBehaviour
{
    [Header("Scan UI Elements")]
    public Button scanButton;
    public Button backToAppButton;
    public Button galleryButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI instructionText;
    public Image previewImage;

    [Header("Processing UI")]
    public GameObject processingPanel;
    public Slider progressSlider;
    public TextMeshProUGUI progressText;

    private bool isScanning = false;
    private Texture2D scannedTexture;

    void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }

    private void InitializeUI()
    {
        if (statusText != null)
        {
            statusText.text = "Fatura taramaya hazýr";
            statusText.color = Color.green;
        }

        if (instructionText != null)
        {
            instructionText.text = "Faturanýzý net bir þekilde çekin veya galeriden seçin. AI modelimiz faturanýzý analiz edecek.";
        }

        if (processingPanel != null)
        {
            processingPanel.SetActive(false);
        }

        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }
    }

    private void SetupEventListeners()
    {
        if (scanButton != null)
        {
            scanButton.onClick.AddListener(StartScanning);
        }

        if (backToAppButton != null)
        {
            backToAppButton.onClick.AddListener(BackToAppScene);
        }

        if (galleryButton != null)
        {
            galleryButton.onClick.AddListener(SelectFromGallery);
        }
    }

    public void StartScanning()
    {
        if (isScanning) return;

        Debug.Log("Fatura tarama baþlatýlýyor...");

        if (statusText != null)
        {
            statusText.text = "Kamera açýlýyor...";
            statusText.color = Color.yellow;
        }

        // Kamera iþlemi simülasyonu - Gerçek kamera entegrasyonu burada olacak
        StartCoroutine(SimulateCameraCapture());
    }

    public void SelectFromGallery()
    {
        if (isScanning) return;

        Debug.Log("Galeriden fatura seçiliyor...");

        if (statusText != null)
        {
            statusText.text = "Galeri açýlýyor...";
            statusText.color = Color.yellow;
        }

        // Galeri seçimi simülasyonu - Gerçek galeri entegrasyonu burada olacak
        StartCoroutine(SimulateGallerySelection());
    }

    private IEnumerator SimulateCameraCapture()
    {
        isScanning = true;

        yield return new WaitForSeconds(1f);

        if (statusText != null)
        {
            statusText.text = "Fatura çekiliyor...";
        }

        yield return new WaitForSeconds(2f);

        // Simulated image capture success
        ProcessScannedBill();
    }

    private IEnumerator SimulateGallerySelection()
    {
        isScanning = true;

        yield return new WaitForSeconds(0.5f);

        if (statusText != null)
        {
            statusText.text = "Fatura seçildi, iþleniyor...";
        }

        yield return new WaitForSeconds(1f);

        // Simulated gallery selection success
        ProcessScannedBill();
    }

    private void ProcessScannedBill()
    {
        if (processingPanel != null)
        {
            processingPanel.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.text = "AI modelimiz faturanýzý analiz ediyor...";
            statusText.color = Color.blue;
        }

        StartCoroutine(SimulateAIProcessing());
    }

    private IEnumerator SimulateAIProcessing()
    {
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Random.Range(0.05f, 0.15f);
            progress = Mathf.Clamp01(progress);

            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            if (progressText != null)
            {
                progressText.text = $"Analiz ediliyor... %{Mathf.RoundToInt(progress * 100)}";
            }

            // Farklý aþamalarda farklý mesajlar
            if (progress < 0.3f && statusText != null)
            {
                statusText.text = "Fatura metni okunuyor...";
            }
            else if (progress < 0.6f && statusText != null)
            {
                statusText.text = "Veriler analiz ediliyor...";
            }
            else if (progress < 0.9f && statusText != null)
            {
                statusText.text = "Sonuçlar hazýrlanýyor...";
            }

            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }

        // Ýþlem tamamlandý
        if (statusText != null)
        {
            statusText.text = "Analiz tamamlandý!";
            statusText.color = Color.green;
        }

        if (progressText != null)
        {
            progressText.text = "Tamamlandý!";
        }

        yield return new WaitForSeconds(1f);

        // Analiz sonuçlarýný kaydet (geçici)
        SaveAnalysisResults();

        // AppScene'e geri dön
        BackToAppScene();
    }

    private void SaveAnalysisResults()
    {
        // Geçici analiz sonuçlarý - Database'e kaydedilecek
        string analysisResult = "Elektrik Faturasý - Kasým 2024\n" +
                              "Tutar: 245.67 TL\n" +
                              "Tüketim: 380 kWh\n" +
                              "AI Önerisi: %15 tasarruf potansiyeli tespit edildi.";

        // PlayerPrefs ile geçici kaydetme - Database entegrasyonu sonrasý deðiþtirilecek
        string existingBills = PlayerPrefs.GetString("ScannedBills", "");
        string newBillEntry = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "|" + analysisResult;

        if (!string.IsNullOrEmpty(existingBills))
        {
            existingBills += "###" + newBillEntry;
        }
        else
        {
            existingBills = newBillEntry;
        }

        PlayerPrefs.SetString("ScannedBills", existingBills);
        PlayerPrefs.Save();

        Debug.Log("Fatura analizi kaydedildi: " + newBillEntry);
    }

    public void BackToAppScene()
    {
        if (isScanning)
        {
            // Tarama iþlemini iptal et
            StopAllCoroutines();
            isScanning = false;
        }

        Debug.Log("AppScene'e geri dönülüyor...");
        SceneManager.LoadScene("AppScene");
    }

    void OnDestroy()
    {
        if (scanButton != null)
        {
            scanButton.onClick.RemoveListener(StartScanning);
        }

        if (backToAppButton != null)
        {
            backToAppButton.onClick.RemoveListener(BackToAppScene);
        }

        if (galleryButton != null)
        {
            galleryButton.onClick.RemoveListener(SelectFromGallery);
        }
    }
}

// AI Analysis sonuçlarý için veri yapýsý
[System.Serializable]
public class BillAnalysisData
{
    public string billType;
    public string date;
    public float amount;
    public float consumption;
    public string unit;
    public string aiSuggestion;
    public float savingPotential;
    public string analysisDetails;
}