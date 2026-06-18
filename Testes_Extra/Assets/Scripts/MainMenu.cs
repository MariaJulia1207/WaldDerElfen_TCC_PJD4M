using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject manualPanel;
    public GameObject creditosPanel;

    [Header("Nome da Cena")]
    public string gameplayScene = "Level1";

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
        SceneManager.LoadScene(gameplayScene);
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