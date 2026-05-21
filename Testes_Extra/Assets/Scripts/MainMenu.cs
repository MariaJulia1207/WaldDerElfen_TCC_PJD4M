using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Fade")]
    public FadeController fadeController;

    [Header("Cena do Jogo")]
    public string nomeDaCena;

    [Header("Painéis")]
    public GameObject mainPanel;
    public GameObject manualPanel;
    public GameObject creditsPanel;

    private bool clicou = false;

    void Start()
    {
        AbrirMenuPrincipal();
    }

    // =========================
    // INICIAR JOGO
    // =========================

    public void IniciarJogo()
    {
        if (clicou) return;

        clicou = true;

        StartCoroutine(CarregarJogo());
    }

    IEnumerator CarregarJogo()
    {
        yield return StartCoroutine(fadeController.FadeOut());

        SceneManager.LoadScene(nomeDaCena);
    }

    // =========================
    // MANUAL
    // =========================

    public void AbrirManual()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(false);

        manualPanel.SetActive(true);
    }

    // =========================
    // CRÉDITOS
    // =========================

    public void AbrirCreditos()
    {
        mainPanel.SetActive(false);
        manualPanel.SetActive(false);

        creditsPanel.SetActive(true);
    }

    // =========================
    // VOLTAR
    // =========================

    public void AbrirMenuPrincipal()
    {
        mainPanel.SetActive(true);

        manualPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }
}