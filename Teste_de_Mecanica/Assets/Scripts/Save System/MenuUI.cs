using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string firstLevelScene = "Level1";
    [SerializeField] private string saveSlotScene = "Menu";

    [Header("UI")]
    [SerializeField] private GameObject continueButton;
    [SerializeField] private LevelLoader levelLoader;

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }

        UpdateContinueButton();
    }

    private void OnEnable()
    {
        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }

        UpdateContinueButton();
    }

    public void UpdateContinueButton()
    {
        if (continueButton == null)
        {
            return;
        }

        bool hasSave = SaveManager.Instance != null && (SaveManager.Instance.HasAutosave || SaveManager.Instance.HasManualSave);
        continueButton.SetActive(hasSave);
    }

    public void OnNewGameClicked()
    {
        if (TryUseLevelLoader(firstLevelScene))
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ForceSceneChange(firstLevelScene);
        }
    }

    public void OnContinueClicked()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasAutosave)
        {
            LoadSlot(SaveManager.AutoSaveSlot);
            return;
        }

        if (SaveManager.Instance != null && SaveManager.Instance.HasManualSave)
        {
            LoadSlot(SaveManager.ManualSaveSlot);
            return;
        }
    }

    public void OnSaveGameClicked()
    {
        if (SaveManager.Instance == null)
        {
            return;
        }

        SaveData save = SaveManager.Instance.BuildDefaultSave();
        SaveManager.Instance.SaveCurrentStateToSlot(SaveManager.ManualSaveSlot, save);
    }

    public void OnLoadManualSlotClicked()
    {
        LoadSlot(SaveManager.ManualSaveSlot);
    }

    private void LoadSlot(int slot)
    {
        if (SaveRestoreManager.Instance != null)
        {
            SaveRestoreManager.Instance.RequestLoadSlot(slot);
            return;
        }

        if (SaveManager.Instance == null || !SaveManager.Instance.SlotExists(slot))
        {
            return;
        }

        SaveData data = SaveManager.Instance.LoadFromSlot(slot);
        if (data != null && GameManager.Instance != null && !string.IsNullOrEmpty(data.sceneName))
        {
            if (TryUseLevelLoader(data.sceneName))
            {
                return;
            }

            GameManager.Instance.ForceSceneChange(data.sceneName);
        }
    }

    public void OnLoadGameClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RequestSceneChange(saveSlotScene);
        }
    }

    public void OnQuitClicked()
    {
        Debug.Log("MenuUI: Quit requested.");

        LevelLoader loader = ResolveLevelLoader();
        if (loader != null)
        {
            loader.TransitionQuit();
            return;
        }

        Application.Quit();
    }

    private LevelLoader ResolveLevelLoader()
    {
        if (levelLoader != null)
        {
            return levelLoader;
        }

        levelLoader = FindAnyObjectByType<LevelLoader>(FindObjectsInactive.Include);
        return levelLoader;
    }

    private bool TryUseLevelLoader(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (string.Equals(currentScene, sceneName, System.StringComparison.Ordinal))
        {
            return false;
        }

        if (sceneName == firstLevelScene || sceneName == saveSlotScene)
        {
            LevelLoader loader = ResolveLevelLoader();
            if (loader == null)
            {
                return false;
            }

            loader.Transition(sceneName);
            return true;
        }

        return false;
    }
}