using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameManager[] gameManagers = Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None);

                if (gameManagers.Length == 0)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
                else
                {
                    instance = gameManagers[0];
                }
            }
            return instance;
        }
    }

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

    void Start()
    {
        CheckLoginStatus();
    }

    private void CheckLoginStatus()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Login scene'de de�ilsek ve kullan�c� giri� yapmam��sa login'e y�nlendir
        if (currentScene != "LogInScene")
        {
            if (!IsUserLoggedIn())
            {
                Debug.Log("Kullan�c� giri� yapmam��, login ekran�na y�nlendiriliyor...");
                SceneManager.LoadScene("LogInScene");
                return;
            }
        }
    }

    public static string GetUsername()
    {
        return PlayerPrefs.GetString("Username", "Kullan�c�");
    }

    public static void Logout()
    {
        // Kullan�c� bilgilerini temizle
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("IsLoggedIn");

        // E�er "Beni Hat�rla" se�ilmemi�se, kay�tl� bilgileri de sil
        bool rememberMe = PlayerPrefs.GetInt("RememberMe", 0) == 1;
        if (!rememberMe)
        {
            PlayerPrefs.DeleteKey("SavedUsername");
            PlayerPrefs.DeleteKey("SavedPassword");
        }

        PlayerPrefs.Save();

        Debug.Log("Kullan�c� ��k�� yapt�");
        SceneManager.LoadScene("LogInScene");
    }

    public static void ForgetUser()
    {
        // T�m kullan�c� bilgilerini sil
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("IsLoggedIn");
        PlayerPrefs.DeleteKey("SavedUsername");
        PlayerPrefs.DeleteKey("SavedPassword");
        PlayerPrefs.DeleteKey("RememberMe");
        PlayerPrefs.Save();

        Debug.Log("Kullan�c� bilgileri tamamen silindi");
        SceneManager.LoadScene("LogInScene");
    }

    public static bool IsUserLoggedIn()
    {
        int isLoggedIn = PlayerPrefs.GetInt("IsLoggedIn", 0);
        string username = PlayerPrefs.GetString("Username", "");

        return isLoggedIn == 1 && !string.IsNullOrEmpty(username);
    }

    public static bool IsUserRemembered()
    {
        return PlayerPrefs.GetInt("RememberMe", 0) == 1;
    }

    // Scene de�i�im event'� dinleme
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Her scene y�klendi�inde login durumunu kontrol et
        if (scene.name != "LogInScene" && !IsUserLoggedIn())
        {
            SceneManager.LoadScene("LogInScene");
        }
    }

    // Database entegrasyonu i�in haz�r methodlar
    public static void SaveUserSession(string username)
    {
        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.SetInt("IsLoggedIn", 1);
        PlayerPrefs.SetString("SessionStartTime", System.DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    public static void ClearUserSession()
    {
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("IsLoggedIn");
        PlayerPrefs.DeleteKey("SessionStartTime");
        PlayerPrefs.Save();
    }
}