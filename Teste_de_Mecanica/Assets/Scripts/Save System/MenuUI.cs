/*
using UnityEngine;

/// <summary>
/// Simple UI controller for the main menu. Attach to a GameObject in the MenuPrincipal scene
/// and wire the OnStartClicked and OnQuitClicked methods to the respective UI buttons.
/// </summary>
public class MenuUiController : MonoBehaviour
{
    [Header("Scenes")]
    public string firstLevelScene = "SampleScene";
    public string saveSlotScene = "SavingSlotSelection";

    [Header("UI")]
    public GameObject continueButton;

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
        if (continueButton == null) return;
        bool hasAuto = SaveManager.Instance != null && SaveManager.Instance.SlotExists(0);
        continueButton.SetActive(hasAuto);
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
        // load from autosave slot 0
        if (SaveRestoreManager.Instance != null)
        {
            SaveRestoreManager.Instance.RequestLoadSlot(0);
        }
        else if (SaveManager.Instance != null && SaveManager.Instance.SlotExists(0))
        {
            var data = SaveManager.Instance.LoadFromSlot(0);
            if (data != null && GameManager.Instance != null)
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
        Debug.Log("MenuUiController: Quit requested.");
        Application.Quit();
    }
}
*/