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
    public LevelLoader levelLoader;

    void Start()
    {
        // Apenas o menu principal começa ativo
        mainPanel.SetActive(true);

        manualPanel.SetActive(false);
        creditosPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    // =========================
    // BOTÃO JOGAR
    // =========================
    public void Jogar()
    {
        if (levelLoader != null)
        {
            levelLoader.Transition(cutsceneScene);
            return;
        }

        SceneManager.LoadScene(cutsceneScene);
    }

    // =========================
    // MANUAL
    // =========================
    public void AbrirManual()
    {
        mainPanel.SetActive(false);
        manualPanel.SetActive(true);
    }

    public void FecharManual()
    {
        manualPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // =========================
    // CRÉDITOS
    // =========================
    public void AbrirCreditos()
    {
        mainPanel.SetActive(false);
        creditosPanel.SetActive(true);
    }

    public void FecharCreditos()
    {
        creditosPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    
    // =========================
    // CONFIGURAÇÕES
    // =========================
    public void AbrirSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);

        // Register any SFX slider inside the settings panel so it syncs with the manager
        if (settingsPanel != null)
        {
            Slider s = settingsPanel.GetComponentInChildren<Slider>(true);
            if (s != null && SoundEffectManager.Instance != null)
            {
                SoundEffectManager.Instance.RegisterSlider(s);
            }
        }
    }

    public void FecharSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // =========================
    // SAIR
    // =========================
    public void Sair()
    {
        Application.Quit();

        Debug.Log("Jogo fechado");
    }
}