using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager instance;
    public static DatabaseManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("DatabaseManager");
                instance = go.AddComponent<DatabaseManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("API Settings")]
    public string baseApiUrl = "https://your-api-domain.com/api";
    public string apiKey = "your-api-key";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    #region User Authentication Methods

    public IEnumerator ValidateUserLogin(string username, string password, System.Action<bool, string> callback)
    {
        LoginRequest loginData = new LoginRequest
        {
            username = username,
            password = password
        };

        string jsonData = JsonUtility.ToJson(loginData);
        string url = baseApiUrl + "/auth/login";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    // Token'ý kaydet
                    PlayerPrefs.SetString("AuthToken", response.token);
                    PlayerPrefs.SetString("UserID", response.userId);
                    callback?.Invoke(true, "Login successful");
                }
                else
                {
                    callback?.Invoke(false, response.message);
                }
            }
            else
            {
                callback?.Invoke(false, "Network error: " + request.error);
            }
        }
    }

    public IEnumerator RegisterUser(string username, string password, string email, System.Action<bool, string> callback)
    {
        RegisterRequest registerData = new RegisterRequest
        {
            username = username,
            password = password,
            email = email
        };

        string jsonData = JsonUtility.ToJson(registerData);
        string url = baseApiUrl + "/auth/register";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);
                callback?.Invoke(response.success, response.message);
            }
            else
            {
                callback?.Invoke(false, "Network error: " + request.error);
            }
        }
    }

    #endregion

    #region Information Card Methods

    public IEnumerator GetInformationCard(System.Action<InformationCardData> callback)
    {
        string url = baseApiUrl + "/information/daily";
        string authToken = PlayerPrefs.GetString("AuthToken", "");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", "Bearer " + authToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                InformationCardResponse response = JsonUtility.FromJson<InformationCardResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    callback?.Invoke(response.data);
                }
                else
                {
                    // Fallback data
                    callback?.Invoke(GetFallbackInformationCard());
                }
            }
            else
            {
                Debug.LogError("Information card fetch error: " + request.error);
                callback?.Invoke(GetFallbackInformationCard());
            }
        }
    }

    private InformationCardData GetFallbackInformationCard()
    {
        return new InformationCardData
        {
            title = "EcoLogic",
            message = "Faturalarýnýzý tarayýn, çevre dostu yaþayýn!",
            tip = "Enerji tasarrufu için LED ampul kullanmayý deneyin.",
            imageUrl = ""
        };
    }

    #endregion

    #region Bill Analysis Methods

    public IEnumerator UploadBillForAnalysis(byte[] imageData, System.Action<BillAnalysisResult> callback)
    {
        string url = baseApiUrl + "/bills/analyze";
        string authToken = PlayerPrefs.GetString("AuthToken", "");

        WWWForm form = new WWWForm();
        form.AddBinaryData("bill_image", imageData, "bill.jpg", "image/jpeg");
        form.AddField("user_id", PlayerPrefs.GetString("UserID", ""));

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            request.SetRequestHeader("Authorization", "Bearer " + authToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                BillAnalysisResponse response = JsonUtility.FromJson<BillAnalysisResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    callback?.Invoke(response.analysis);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError("Bill analysis error: " + request.error);
                callback?.Invoke(null);
            }
        }
    }

    public IEnumerator GetPastBills(System.Action<List<PastBillData>> callback)
    {
        string userId = PlayerPrefs.GetString("UserID", "");
        string url = baseApiUrl + "/bills/history?user_id=" + userId;
        string authToken = PlayerPrefs.GetString("AuthToken", "");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", "Bearer " + authToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                PastBillsResponse response = JsonUtility.FromJson<PastBillsResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    callback?.Invoke(new List<PastBillData>(response.bills));
                }
                else
                {
                    callback?.Invoke(new List<PastBillData>());
                }
            }
            else
            {
                Debug.LogError("Past bills fetch error: " + request.error);
                callback?.Invoke(new List<PastBillData>());
            }
        }
    }

    #endregion

    #region Data Classes

    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class LoginResponse
    {
        public bool success;
        public string message;
        public string token;
        public string userId;
    }

    [Serializable]
    public class RegisterRequest
    {
        public string username;
        public string password;
        public string email;
    }

    [Serializable]
    public class RegisterResponse
    {
        public bool success;
        public string message;
    }

    [Serializable]
    public class InformationCardResponse
    {
        public bool success;
        public InformationCardData data;
    }

    [Serializable]
    public class InformationCardData
    {
        public string title;
        public string message;
        public string tip;
        public string imageUrl;
    }

    [Serializable]
    public class BillAnalysisResponse
    {
        public bool success;
        public string message;
        public BillAnalysisResult analysis;
    }

    [Serializable]
    public class BillAnalysisResult
    {
        public string billType;
        public float amount;
        public float consumption;
        public string consumptionUnit;
        public string period;
        public string aiSuggestions;
        public float ecoScore;
        public float savingPotential;
        public string analysisDetails;
    }

    [Serializable]
    public class PastBillsResponse
    {
        public bool success;
        public PastBillData[] bills;
        public int totalCount;
    }

    [Serializable]
    public class PastBillData
    {
        public string id;
        public string billType;
        public string date;
        public float amount;
        public float consumption;
        public string aiSuggestions;
        public float ecoScore;
        public string analysisDetails;
    }

    #endregion
}