using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject manualPanel;
    public GameObject creditosPanel;

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
    // SAIR
    // =========================
    public void Sair()
    {
        Application.Quit();

        Debug.Log("Jogo fechado");
    }
}