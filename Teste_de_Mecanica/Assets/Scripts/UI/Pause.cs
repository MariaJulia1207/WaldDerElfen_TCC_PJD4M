using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    [Header("Painéis e Menu")]
    [SerializeField] private GameObject pausePanel;

    [Header("Transição")]
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private string cena;

    private bool isPaused;

    private void Start()
    {
        // Garante que o jogo comece despausado
        isPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        // ESC abre/fecha o Pause
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            PauseScreen();
        }
    }

    // =========================================================
    // PAUSE
    // =========================================================

    public void PauseScreen()
    {
        if (isPaused)
        {
            Despausar();
        }
        else
        {
            Pausar();
        }
    }

    // =========================================================
    // PAUSAR
    // =========================================================

    public void Pausar()
    {
        isPaused = true;

        pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // =========================================================
    // DESPAUSAR
    // =========================================================

    public void Despausar()
    {
        isPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // =========================================================
    // VOLTAR AO MENU
    // =========================================================

    public void VoltarAoMenu()
    {
        // IMPORTANTE:
        // O LevelLoader usa WaitForSeconds,
        // então precisamos descongelar o tempo primeiro.

        Time.timeScale = 1f;

        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        levelLoader.Transition(cena);
    }
}