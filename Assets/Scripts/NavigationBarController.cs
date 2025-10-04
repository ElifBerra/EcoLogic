using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Reusable controller for the bottom/top navigation bar.
/// Drop this on your NavigationBar GameObject and assign the buttons in the Inspector.
/// Works in every scene and simply loads the target scenes using SceneNames constants.
/// </summary>
public class NavigationBarController : MonoBehaviour
{
    [Header("Nav Buttons")]
    [SerializeField] private Button homeButton;       // -> Starting/App hub
    [SerializeField] private Button pastBillsButton;  // -> PastBillsScene
    [SerializeField] private Button scanButton;       // -> ScanBillsScene
    [SerializeField] private Button logoutButton;     // -> LogInScene (via GameManager.Logout())
    [SerializeField] private Button solarPanelButton;



    [Header("Optional UX")]
    [SerializeField] private AudioSource clickSfx;

    private void Awake()
    {
        // In case the prefab is copied into multiple canvases, avoid duplicate listeners.
        RemoveAll();
        Wire();
    }

    private void OnDestroy()
    {
        RemoveAll();
    }

    private void Wire()
    {
        if (homeButton)      homeButton.onClick.AddListener(GoHome);
        if (pastBillsButton) pastBillsButton.onClick.AddListener(GoPastBills);
        if (scanButton)      scanButton.onClick.AddListener(GoScan);
        if (logoutButton)    logoutButton.onClick.AddListener(DoLogout);
        if (solarPanelButton) solarPanelButton.onClick.AddListener(GoSolarPanel);

    }

    private void RemoveAll()
    {
        if (homeButton)      homeButton.onClick.RemoveAllListeners();
        if (pastBillsButton) pastBillsButton.onClick.RemoveAllListeners();
        if (scanButton)      scanButton.onClick.RemoveAllListeners();
        if (logoutButton)    logoutButton.onClick.RemoveAllListeners();
    }

    private void PlayClick()
    {
        if (clickSfx) clickSfx.Play();
    }

    // --- Handlers ---
    public void GoHome()
    {
        PlayClick();
        // Prefer App hub if present; fallback to Starting
        // Change the target to SceneNames.App if your main hub is AppScene.
        if (HasSceneInBuild(SceneNames.App)) SceneManager.LoadScene(SceneNames.App);
        else                                  SceneManager.LoadScene(SceneNames.Starting);
    }

    public void GoSolarPanel()
    {
        PlayClick();
        SceneManager.LoadScene(SceneNames.SolarPanel);
    }


    public void GoPastBills()
    {
        PlayClick();
        SceneManager.LoadScene(SceneNames.PastBills);
    }

    public void GoScan()
    {
        PlayClick();
        SceneManager.LoadScene(SceneNames.ScanBills);
    }

    public void DoLogout()
    {
        PlayClick();
        // Centralized logout; will route to LogInScene
        GameManager.Logout();
    }

    // Helper: check build settings
    private bool HasSceneInBuild(string name)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (sceneName == name) return true;
        }
        return false;
    }
}
