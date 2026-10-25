using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string firstLevelScene = "Level1";
    [SerializeField] private string saveSlotScene = "Menu";

    [Header("UI")]
    [SerializeField] private GameObject continueButton;

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

    // =========================================================
    // NOVO JOGO
    // =========================================================

    public void OnNewGameClicked()
    {
        // Reseta os dados de vida salvos para começar com a vida padrão no novo jogo
        HealthSystem.ResetHealthData();

        if (TryUseLevelLoader(firstLevelScene))
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ForceSceneChange(firstLevelScene);
            return;
        }

        SceneManager.LoadScene(firstLevelScene);
    }

    // =========================================================
    // CONTINUAR / CARREGAR JOGO
    // =========================================================

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
        if (data != null && !string.IsNullOrEmpty(data.sceneName))
        {
            if (TryUseLevelLoader(data.sceneName))
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ForceSceneChange(data.sceneName);
            }
        }
    }

    public void OnLoadGameClicked()
    {
        if (TryUseLevelLoader(saveSlotScene))
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RequestSceneChange(saveSlotScene);
        }
    }

    // =========================================================
    // SAIR
    // =========================================================

    public void OnQuitClicked()
    {
        Debug.Log("MenuUI: Quit requested.");

        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.TransitionQuit();
            return;
        }

        Application.Quit();
    }

    // =========================================================
    // TRANSIÇÃO VIA LEVEL LOADER
    // =========================================================

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

        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.Transition(sceneName);
            return true;
        }

        return false;
    }
}