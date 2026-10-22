using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject manualPanel;
    public GameObject creditosPanel;
    public GameObject settingsPanel;

    [Header("Nome das Cenas")]
    public string cutsceneScene = "Cutscene_EraUmaVez";
    public string gameplayScene = "Level1";

    void Start()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (manualPanel != null) manualPanel.SetActive(false);
        if (creditosPanel != null) creditosPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void Jogar()
    {
        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.Transition(cutsceneScene);
            return;
        }

        SceneManager.LoadScene(cutsceneScene);
    }

    public void AbrirManual()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (manualPanel != null) manualPanel.SetActive(true);
    }

    public void FecharManual()
    {
        if (manualPanel != null) manualPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void AbrirCreditos()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (creditosPanel != null) creditosPanel.SetActive(true);
    }

    public void FecharCreditos()
    {
        if (creditosPanel != null) creditosPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void AbrirSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Slider s = settingsPanel.GetComponentInChildren<Slider>(true);
            if (s != null && SoundEffectManager.Instance != null)
            {
                SoundEffectManager.Instance.RegisterSlider(s);
            }
        }
    }

    public void FecharSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void Sair()
    {
        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.TransitionQuit();
            return;
        }

        Application.Quit();
    }
}