using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string firstLevelScene = "Level1";
    [SerializeField] private string saveSlotScene = "Menu";

    [Header("UI")]
    [SerializeField] private GameObject continueButton;

    private void Start()
    {
        UpdateContinueButton();
    }

    private void OnEnable()
    {
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
        Application.Quit();
    }
}