using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        isPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
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
        AutoSaveCurrentGame();

        pausePanel.SetActive(true);

        // Register any SFX slider inside pause panel so manager can sync it
        if (pausePanel != null)
        {
            Slider s = pausePanel.GetComponentInChildren<Slider>(true);
            if (s != null && SoundEffectManager.Instance != null)
            {
                SoundEffectManager.Instance.RegisterSlider(s);
            }
        }

        Time.timeScale = 0f;
    }

    private void AutoSaveCurrentGame()
    {
        if (SaveManager.Instance == null)
        {
            return;
        }

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            playerPosition = FindPlayerPosition(),
            checkpointPosition = CheckpointManager.Instance != null ? CheckpointManager.Instance.LastCheckpointPosition : Vector3.zero,
            checkpointReached = CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint,
            levelIndex = 0,
            coins = 0,
            playerHealth = 100
        };

        SaveManager.Instance.SaveCurrentStateToSlot(SaveManager.AutoSaveSlot, data);
    }

    private Vector3 FindPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform.position : Vector3.zero;
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
        Time.timeScale = 1f;

        isPaused = false;

        pausePanel.SetActive(false);

        levelLoader.Transition(cena);
    }

    // =========================================================
    // GARANTIA
    // =========================================================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}