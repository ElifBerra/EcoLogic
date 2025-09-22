using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScanBillsSceneController : MonoBehaviour
{
    [Header("Scan UI Elements")]
    public Button scanButton;
    public Button captureButton;          // Foto�raf �ekme butonu
    public Button backToAppButton;
    public Button galleryButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI instructionText;
    public RawImage cameraPreview;        // Kamera g�r�nt�s� i�in

    [Header("Processing UI")]
    public GameObject processingPanel;
    public Slider progressSlider;
    public TextMeshProUGUI progressText;

    private bool isScanning = false;
    private WebCamTexture webCamTexture;

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
        if (scanButton != null) scanButton.onClick.AddListener(StartScanning);
        if (captureButton != null) captureButton.onClick.AddListener(CapturePhoto);
        if (backToAppButton != null) backToAppButton.onClick.AddListener(BackToAppScene);
        if (galleryButton != null) galleryButton.onClick.AddListener(SelectFromGallery);
    }

    public void StartScanning()
    {
        if (isScanning) return;

        Debug.Log("Kamera a��l�yor...");
        if (statusText != null)
        {
            statusText.text = "Kamera a��l�yor...";
            statusText.color = Color.yellow;
        }

        StartCamera();
    }

    private void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            if (statusText != null)
                statusText.text = "Taray�c� kameraya eri�emedi. L�tfen izin verin.";
            return;
        }

        webCamTexture = new WebCamTexture(devices[0].name, 1280, 720, 30);
        cameraPreview.texture = webCamTexture;
        cameraPreview.material.mainTexture = webCamTexture;
        webCamTexture.Play();

        StartCoroutine(CheckCameraPermission());
    }

    private IEnumerator CheckCameraPermission()
    {
        // Taray�c� izin popup�una yan�t verene kadar bekle
        float timeout = 5f;
        while (!webCamTexture.isPlaying && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!webCamTexture.isPlaying)
        {
            if (statusText != null)
                statusText.text = "Kamera ba�lat�lamad� (izin reddedilmi� olabilir).";
        }
        else
        {
            if (statusText != null)
            {
                statusText.text = "Kamera a��k";
                statusText.color = Color.green;
            }
            isScanning = true;
        }
    }



    private void StopCamera()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            Destroy(webCamTexture);
            webCamTexture = null;
        }
        if (cameraPreview != null) cameraPreview.texture = null;
        isScanning = false;
    }

    public void CapturePhoto()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying)
        {
            Debug.LogWarning("Kamera �al��m�yor!");
            return;
        }

        // WebCamTexture�den Texture2D �ret
        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        // JPG olarak encode et
        byte[] bytes = photo.EncodeToJPG();

        // Kaydetme path�i
        string filename = "Bill_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
        System.IO.File.WriteAllBytes(path, bytes);

        Debug.Log("Foto�raf kaydedildi: " + path);

        if (statusText != null)
        {
            statusText.text = "Foto�raf �ekildi ve kaydedildi!";
            statusText.color = Color.green;
        }

        // �stersen burada DatabaseManager.UploadBillForAnalysis() �a��rabilirsin
        // StartCoroutine(DatabaseManager.Instance.UploadBillForAnalysis(bytes, OnAnalysisResult));

        // Test i�in foto�raf i�leme sim�lasyonu
        ProcessScannedBill();
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

        // �imdilik sim�lasyon
        StartCoroutine(SimulateGallerySelection());
    }

    private IEnumerator SimulateGallerySelection()
    {
        isScanning = true;

        yield return new WaitForSeconds(0.5f);
        if (statusText != null) statusText.text = "Fatura se�ildi, i�leniyor...";

        yield return new WaitForSeconds(1f);
        ProcessScannedBill();
    }

    private void ProcessScannedBill()
    {
        if (processingPanel != null) processingPanel.SetActive(true);

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

            if (progressSlider != null) progressSlider.value = progress;
            if (progressText != null) progressText.text = $"Analiz ediliyor... %{Mathf.RoundToInt(progress * 100)}";

            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }

        if (statusText != null)
        {
            statusText.text = "Analiz tamamland�!";
            statusText.color = Color.green;
        }
        if (progressText != null) progressText.text = "Tamamland�!";

        yield return new WaitForSeconds(1f);

        SaveAnalysisResults();
        BackToAppScene();
    }

    private void SaveAnalysisResults()
    {
        string analysisResult = "Elektrik Faturas� - Kas�m 2024\n" +
                              "Tutar: 245.67 TL\n" +
                              "T�ketim: 380 kWh\n" +
                              "AI �nerisi: %15 tasarruf potansiyeli tespit edildi.";

        string existingBills = PlayerPrefs.GetString("ScannedBills", "");
        string newBillEntry = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "|" + analysisResult;

        if (!string.IsNullOrEmpty(existingBills))
            existingBills += "###" + newBillEntry;
        else
            existingBills = newBillEntry;

        PlayerPrefs.SetString("ScannedBills", existingBills);
        PlayerPrefs.Save();

        Debug.Log("Fatura analizi kaydedildi: " + newBillEntry);
    }

    public void BackToAppScene()
    {
        if (isScanning)
        {
            StopCamera();
            isScanning = false;
        }

        Debug.Log("AppScene'e geri d�n�l�yor...");
        SceneManager.LoadScene("AppScene");
    }

    void OnDestroy()
    {
        if (scanButton != null) scanButton.onClick.RemoveListener(StartScanning);
        if (captureButton != null) captureButton.onClick.RemoveListener(CapturePhoto);
        if (backToAppButton != null) backToAppButton.onClick.RemoveListener(BackToAppScene);
        if (galleryButton != null) galleryButton.onClick.RemoveListener(SelectFromGallery);

        StopCamera();
    }
}
