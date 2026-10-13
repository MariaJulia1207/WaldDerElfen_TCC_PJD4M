using UnityEngine;

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

        bool hasAutoSave = SaveManager.Instance != null && SaveManager.Instance.HasAutosave;
        continueButton.SetActive(hasAutoSave);
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
        if (SaveRestoreManager.Instance != null)
        {
            SaveRestoreManager.Instance.RequestLoadSlot(SaveManager.AutoSaveSlot);
            return;
        }

        if (SaveManager.Instance != null && SaveManager.Instance.SlotExists(SaveManager.AutoSaveSlot))
        {
            SaveData data = SaveManager.Instance.LoadFromSlot(SaveManager.AutoSaveSlot);
            if (data != null && GameManager.Instance != null && !string.IsNullOrEmpty(data.sceneName))
            {
                if (TryUseLevelLoader(data.sceneName))
                {
                    return;
                }

                GameManager.Instance.ForceSceneChange(data.sceneName);
            }
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
        LevelLoader loader = ResolveLevelLoader();
        if (loader == null)
        {
            return false;
        }

        loader.Transition(sceneName);
        return true;
    }
}