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
            statusText.text = "Fatura taramaya haz�r";
            statusText.color = Color.green;
        }

        if (instructionText != null)
        {
            instructionText.text = "Faturan�z� net bir �ekilde �ekin veya galeriden se�in. AI modelimiz faturan�z� analiz edecek.";
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

        Debug.Log("Fatura tarama ba�lat�l�yor...");

        if (statusText != null)
        {
            statusText.text = "Kamera a��l�yor...";
            statusText.color = Color.yellow;
        }

        // Kamera i�lemi sim�lasyonu - Ger�ek kamera entegrasyonu burada olacak
        StartCoroutine(SimulateCameraCapture());
    }

    public void SelectFromGallery()
    {
        if (isScanning) return;

        Debug.Log("Galeriden fatura se�iliyor...");

        if (statusText != null)
        {
            statusText.text = "Galeri a��l�yor...";
            statusText.color = Color.yellow;
        }

        // Galeri se�imi sim�lasyonu - Ger�ek galeri entegrasyonu burada olacak
        StartCoroutine(SimulateGallerySelection());
    }

    private IEnumerator SimulateCameraCapture()
    {
        isScanning = true;

        yield return new WaitForSeconds(1f);

        if (statusText != null)
        {
            statusText.text = "Fatura �ekiliyor...";
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
            statusText.text = "Fatura se�ildi, i�leniyor...";
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
            statusText.text = "AI modelimiz faturan�z� analiz ediyor...";
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

            // Farkl� a�amalarda farkl� mesajlar
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
                statusText.text = "Sonu�lar haz�rlan�yor...";
            }

            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }

        // ��lem tamamland�
        if (statusText != null)
        {
            statusText.text = "Analiz tamamland�!";
            statusText.color = Color.green;
        }

        if (progressText != null)
        {
            progressText.text = "Tamamland�!";
        }

        yield return new WaitForSeconds(1f);

        // Analiz sonu�lar�n� kaydet (ge�ici)
        SaveAnalysisResults();

        // AppScene'e geri d�n
        BackToAppScene();
    }

    private void SaveAnalysisResults()
    {
        // Ge�ici analiz sonu�lar� - Database'e kaydedilecek
        string analysisResult = "Elektrik Faturas� - Kas�m 2024\n" +
                              "Tutar: 245.67 TL\n" +
                              "T�ketim: 380 kWh\n" +
                              "AI �nerisi: %15 tasarruf potansiyeli tespit edildi.";

        // PlayerPrefs ile ge�ici kaydetme - Database entegrasyonu sonras� de�i�tirilecek
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
            // Tarama i�lemini iptal et
            StopAllCoroutines();
            isScanning = false;
        }

        Debug.Log("AppScene'e geri d�n�l�yor...");
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

// AI Analysis sonu�lar� i�in veri yap�s�
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