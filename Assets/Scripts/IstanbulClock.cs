using UnityEngine;
using TMPro;
using System;
using System.Runtime.InteropServices;

public class IstanbulClock : MonoBehaviour
{
    [Header("Saatin yazýlacaðý TMP_Text")]
    public TMP_Text label;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void StartIstanbulClock(string goName, string method);
    [DllImport("__Internal")] private static extern void StopIstanbulClock();
#endif

    // Editor/Standalone için basit fallback (testte saat görelim)
    TimeZoneInfo _tz;
    float _accum;

    void Awake()
    {
        if (label == null)
            label = GetComponent<TMP_Text>();

        try { _tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
        catch
        {
            try { _tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); } // Windows
            catch { _tz = TimeZoneInfo.Local; }
        }
    }

    void OnEnable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StartIstanbulClock(gameObject.name, nameof(OnTimeFromJS));
#else
        _accum = 0f; // Editor/PC’de C# ile her saniye güncelle
#endif
    }

    void OnDisable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StopIstanbulClock();
#endif
    }

    // WebGL tarafýndaki JS burayý her saniye çaðýrýr
    public void OnTimeFromJS(string timeStr)
    {
        if (label != null)
            label.text = timeStr;
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    void Update()
    {
        // Sadece Editor/Standalone için: her 1 sn’de bir yaz
        _accum += Time.deltaTime;
        if (_accum >= 1f)
        {
            _accum = 0f;
            var nowTr = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _tz);
            if (label != null)
                label.text = nowTr.ToString("HH:mm:ss");
        }
    }
#endif
}
