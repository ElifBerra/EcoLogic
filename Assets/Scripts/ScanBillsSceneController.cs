using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

/// <summary>
/// EcoLogicLLM – Fatura tarama / yükleme ve FastAPI ile entegrasyon
/// Backend uçlarý:
///   GET  /ping               -> saðlýk kontrolü
///   POST /parse-invoice      -> multipart/form-data "file" (image/jpeg/png)
///   POST /analyze            -> {"invoice_json": {...}}
/// Dönüþ (parse-invoice):
///   kökte provider, invoice_date, items..., + analysis{...}, llm_response
/// </summary>
public class ScanBillsSceneController : MonoBehaviour
{
    [Header("Scan UI Elements")]
    public Button scanButton;
    public Button captureButton;
    public Button galleryButton;
    public Button backToAppButton;
    public RawImage cameraPreview;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI instructionText;

    [Header("Processing UI")]
    public GameObject processingPanel;
    public Slider progressSlider;
    public TextMeshProUGUI progressText;

    [Header("Server (Tek taban URL)")]
    [Tooltip("Editor/Windows için (ayný makine)")]
    public string baseUrlEditor = "http://127.0.0.1:8080";
    [Tooltip("WebGL/Telefon için PC'nin LAN IP'si: ipconfig -> IPv4 Address")]
    public string baseUrlLAN = "http://192.168.1.34:8080"; // <- KENDÝ PC IP'ni yaz
    [Tooltip("Geliþtirmede self-signed HTTPS'e izin ver (yalnýzca dev!)")]
    public bool allowSelfSignedHttpsInDev = true;
    [Tooltip("FastAPI'de header bekliyorsan (örn. X-API-KEY)")]
    public string apiKey = ""; // ör: X-API-KEY deðeri

    [Header("Capture")]
    [Range(256, 4096)] public int maxUploadSide = 1600; // görsel en/boy max
    [Range(10, 100)] public int jpgQuality = 80;

    private bool isScanning = false;
    private WebCamTexture webCamTexture;
    private bool serverReachable = false;

    // ---- URL hesaplayýcýlar (tek yerden kontrol) ----
    private string BaseUrl =>
#if UNITY_WEBGL && !UNITY_EDITOR
        baseUrlLAN;            // WebGL build tarayýcýda -> PC'nin LAN IP'si
#elif UNITY_ANDROID || UNITY_IOS
        baseUrlLAN;            // Mobil cihaz -> PC'nin LAN IP'si
#else
        baseUrlEditor;         // Editor/Windows Standalone -> localhost
#endif

    private string ParseUrl => $"{BaseUrl}/parse-invoice";
    private string AnalyzeUrl => $"{BaseUrl}/analyze";
    private string PingUrl => $"{BaseUrl}/ping";

    // ---- HTTPS dev sertifika handler (opsiyonel) ----
    class DevBypassCert : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }

    void Start()
    {
        Application.runInBackground = true; // WebGL'de faydalý
        InitUI();
        WireUI();
        // Baþta baðlantýyý test et
        StartCoroutine(PingServer());
    }

    #region UI lifecycle
    void InitUI()
    {
        if (statusText) statusText.text = "Fatura taramaya hazýr";
        if (instructionText) instructionText.text = "Faturanýzý çekin veya galeriden seçin. Analiz sonucu ekranda görünecek.";
        if (processingPanel) processingPanel.SetActive(false);
        if (progressSlider) progressSlider.value = 0f;
        if (cameraPreview) cameraPreview.gameObject.SetActive(false);
    }

    void WireUI()
    {
        if (scanButton) scanButton.onClick.AddListener(StartScanning);
        if (captureButton) captureButton.onClick.AddListener(CapturePhoto);
        if (galleryButton) galleryButton.onClick.AddListener(SelectFromGallery);
        if (backToAppButton) backToAppButton.onClick.AddListener(BackToAppScene);
    }
    #endregion

    #region Camera
    public void StartScanning()
    {
        if (isScanning) return;
        if (statusText) statusText.text = "Kamera açýlýyor...";
        if (cameraPreview) cameraPreview.gameObject.SetActive(true);
        StartCamera();
    }

    void StartCamera()
    {
        var devices = WebCamTexture.devices;
        if (devices == null || devices.Length == 0)
        {
            if (statusText) statusText.text = "Kameraya eriþilemiyor. Ýzinleri kontrol edin.";
            return;
        }
        // Arka kamerayý tercih et
        string deviceName = devices[0].name;
        foreach (var d in devices) if (!d.isFrontFacing) { deviceName = d.name; break; }

        webCamTexture = new WebCamTexture(deviceName, 1280, 720, 30);
        if (cameraPreview)
        {
            cameraPreview.texture = webCamTexture;
            if (cameraPreview.material) cameraPreview.material.mainTexture = webCamTexture;
        }
        webCamTexture.Play();
        StartCoroutine(CheckCameraStart());
    }

    IEnumerator CheckCameraStart()
    {
        float t = 5f;
        while (!webCamTexture.isPlaying && t > 0f) { t -= Time.deltaTime; yield return null; }
        if (!webCamTexture.isPlaying) { if (statusText) statusText.text = "Kamera baþlatýlamadý."; }
        else { isScanning = true; if (statusText) statusText.text = "Kamera hazýr"; }
    }

    void StopCamera()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            Destroy(webCamTexture);
            webCamTexture = null;
        }
        if (cameraPreview) cameraPreview.texture = null;
        isScanning = false;
    }

    public void CapturePhoto()
    {
        if (!webCamTexture || !webCamTexture.isPlaying)
        {
            Debug.LogWarning("Kamera çalýþmýyor.");
            return;
        }

        var photo = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        // Downscale
        Texture2D sendTex = photo;
        int longest = Mathf.Max(photo.width, photo.height);
        if (longest > maxUploadSide)
        {
            float s = maxUploadSide / (float)longest;
            int w = Mathf.RoundToInt(photo.width * s);
            int h = Mathf.RoundToInt(photo.height * s);
            sendTex = ScaleTexture(photo, w, h);
            Destroy(photo);
        }

        byte[] jpg = sendTex.EncodeToJPG(Mathf.Clamp(jpgQuality, 10, 100));
        Destroy(sendTex);

        if (!serverReachable)
        {
            if (statusText) statusText.text = "Sunucuya ulaþýlamýyor. Baðlantý test ediliyor...";
            StartCoroutine(PingThenUpload(jpg, "capture.jpg"));
        }
        else
        {
            if (statusText) statusText.text = "Fotoðraf alýndý, gönderiliyor...";
            StartCoroutine(UploadInvoiceImage(jpg, "capture.jpg"));
        }
    }
    #endregion

    #region Gallery
    public void SelectFromGallery()
    {
#if UNITY_EDITOR
        string file = UnityEditor.EditorUtility.OpenFilePanel("Fatura seç", "", "jpg,jpeg,png");
        if (string.IsNullOrEmpty(file)) { if (statusText) statusText.text = "Seçim iptal."; return; }
        byte[] bytes = File.ReadAllBytes(file);
        if (statusText) statusText.text = "Gönderiliyor...";
        StartCoroutine(UploadInvoiceImage(bytes, Path.GetFileName(file)));
#else
        if (statusText) statusText.text = "Bu platformda basit picker yok. (NativeGallery önerilir)";
#endif
    }
    #endregion

    #region Networking
    IEnumerator PingServer()
    {
        using (var uwr = UnityWebRequest.Get(PingUrl))
        {
            uwr.timeout = 8;
            if (BaseUrl.StartsWith("https", System.StringComparison.OrdinalIgnoreCase) && allowSelfSignedHttpsInDev)
                uwr.certificateHandler = new DevBypassCert();

            yield return uwr.SendWebRequest();

            serverReachable = (uwr.result == UnityWebRequest.Result.Success && uwr.responseCode == 200);
            Debug.Log($"PING {PingUrl} -> {uwr.result} {uwr.responseCode} {uwr.error}");
            if (statusText) statusText.text = serverReachable ? $"Baðlantý OK: {BaseUrl}" : $"Sunucuya ulaþýlamýyor: {BaseUrl}";
        }
    }

    IEnumerator PingThenUpload(byte[] jpg, string fileName)
    {
        yield return PingServer();
        if (!serverReachable)
        {
            if (statusText) statusText.text = "Sunucuya ulaþýlamadý. IP/port/CORS kontrol et.";
            yield break;
        }
        StartCoroutine(UploadInvoiceImage(jpg, fileName));
    }

    IEnumerator UploadInvoiceImage(byte[] bytes, string fileName)
    {
        if (processingPanel) processingPanel.SetActive(true);
        if (progressSlider) progressSlider.value = 0.05f;
        if (progressText) progressText.text = "Yükleniyor...";

        // Ýçerik tipini dosya adýna göre belirle
        string ext = string.IsNullOrEmpty(fileName) ? ".jpg" : Path.GetExtension(fileName).ToLower();
        string mime = (ext == ".png") ? "image/png" : "image/jpeg";
        string safeName = string.IsNullOrEmpty(fileName) ? (mime == "image/png" ? "invoice.png" : "invoice.jpg") : fileName;

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", bytes, safeName, mime);

        using (var uwr = UnityWebRequest.Post(ParseUrl, form))
        {
            uwr.timeout = 90;
            uwr.SetRequestHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(apiKey)) uwr.SetRequestHeader("X-API-KEY", apiKey);
            if (ParseUrl.StartsWith("https", System.StringComparison.OrdinalIgnoreCase) && allowSelfSignedHttpsInDev)
                uwr.certificateHandler = new DevBypassCert();

            var op = uwr.SendWebRequest();
            while (!op.isDone)
            {
                if (progressSlider) progressSlider.value = Mathf.Lerp(progressSlider.value, 0.7f + 0.25f * uwr.uploadProgress, 0.15f);
                yield return null;
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Upload fail: {uwr.responseCode} - {uwr.error} - {uwr.downloadHandler.text}");
                if (statusText) statusText.text = "Yükleme hatasý (konsola bak)";
                if (processingPanel) processingPanel.SetActive(false);
                yield break;
            }

            ParseAndShowResult(uwr.downloadHandler.text);
        }
    }

    /// <summary>
    /// (Opsiyonel) sample JSON’u /analyze’a gönderip test için
    /// </summary>
    public void SendSampleJsonToAnalyze(TextAsset sampleJson)
    {
        if (sampleJson == null) { Debug.LogWarning("Sample JSON yok."); return; }
        StartCoroutine(PostAnalyze(sampleJson.text));
    }

    IEnumerator PostAnalyze(string rawInvoiceJson)
    {
        if (processingPanel) processingPanel.SetActive(true);
        if (progressText) progressText.text = "Analiz isteði gönderiliyor...";

        // Body: {"invoice_json": {...}}
        var wrapped = $"{{\"invoice_json\": {rawInvoiceJson} }}";
        byte[] body = Encoding.UTF8.GetBytes(wrapped);

        using (var uwr = new UnityWebRequest(AnalyzeUrl, "POST"))
        {
            uwr.uploadHandler = new UploadHandlerRaw(body);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(apiKey)) uwr.SetRequestHeader("X-API-KEY", apiKey);
            uwr.timeout = 90;
            if (AnalyzeUrl.StartsWith("https", System.StringComparison.OrdinalIgnoreCase) && allowSelfSignedHttpsInDev)
                uwr.certificateHandler = new DevBypassCert();

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Analyze error: {uwr.responseCode} - {uwr.error} - {uwr.downloadHandler.text}");
                if (statusText) statusText.text = "Sunucu hatasý (konsola bak)";
            }
            else
            {
                ParseAndShowResult(uwr.downloadHandler.text);
            }
        }
    }
    #endregion

    #region JSON parse & UI
    void ParseAndShowResult(string json)
    {
        try
        {
            if (progressSlider) progressSlider.value = 1f;
            if (progressText) progressText.text = "Yanýt alýndý";

            var jo = JObject.Parse(json);

            // Kökteki alanlar (parse-invoice dönüþü için)
            string provider = (string)jo["provider"];
            string invDate = (string)jo["invoice_date"];
            string dueDate = (string)jo["due_date"];

            float? totalTry = (float?)jo["total_try"];
            float? geceFark = (float?)jo.SelectToken("items.gece.index.fark");
            float? gunduzFark = (float?)jo.SelectToken("items.gunduz.index.fark");
            float? puantFark = (float?)jo.SelectToken("items.puant.index.fark");

            // analysis ve LLM
            float? totalCons = (float?)jo.SelectToken("analysis.total_consumption");
            float? avgCost = (float?)jo.SelectToken("analysis.avg_cost");
            string llm = (string)jo["llm_response"];

            var sb = new StringBuilder();
            sb.AppendLine($"Saðlayýcý: {provider ?? "-"}");
            sb.AppendLine($"Fatura Tarihi: {invDate ?? "-"}");
            sb.AppendLine($"Son Ödeme: {dueDate ?? "-"}");
            sb.AppendLine($"Toplam Tutar (TL): {(totalTry.HasValue ? totalTry.Value.ToString("0.##") : "-")}");
            sb.AppendLine($"Tüketim (kWh): {(totalCons.HasValue ? totalCons.Value.ToString("0.##") : "-")} | Ortalama Maliyet: {(avgCost.HasValue ? avgCost.Value.ToString("0.###") : "-")}");
            sb.AppendLine($"Gündüz fark: {gunduzFark?.ToString("0.###") ?? "-"} | Puant fark: {puantFark?.ToString("0.###") ?? "-"} | Gece fark: {geceFark?.ToString("0.###") ?? "-"}");
            sb.AppendLine("\n— LLM Önerileri —\n");
            sb.AppendLine(string.IsNullOrWhiteSpace(llm) ? "(LLM yanýtý boþ döndü)" : llm);

            if (statusText) statusText.text = sb.ToString();
            if (instructionText) instructionText.text = "Analiz tamamlandý.";

            PlayerPrefs.SetString("EcoLogic_LastInvoiceJson", json);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON parse hatasý: " + e + "\nPayload: " + json);
            if (statusText) statusText.text = "JSON parse hatasý (konsola bak)";
        }
        finally
        {
            if (processingPanel) processingPanel.SetActive(false);
        }
    }
    #endregion

    #region Helpers
    Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        var rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        Graphics.Blit(source, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;

        var result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }
    #endregion

    #region Navigation / Cleanup
    public void BackToAppScene()
    {
        if (isScanning) StopCamera();
        SceneManager.LoadScene("AppScene"); // sahne adýný düzenle
    }

    void OnDestroy()
    {
        if (scanButton) scanButton.onClick.RemoveListener(StartScanning);
        if (captureButton) captureButton.onClick.RemoveListener(CapturePhoto);
        if (galleryButton) galleryButton.onClick.RemoveListener(SelectFromGallery);
        if (backToAppButton) backToAppButton.onClick.RemoveListener(BackToAppScene);
        StopCamera();
    }
    #endregion
}
