using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [Header("Painéis e Menu")]
    [SerializeField] private GameObject pausePanel;

    [Header("Transição")]
    [SerializeField] private string cena = "Menu";

    private bool isPaused;

    private void Start()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            PauseScreen();
        }
    }

    public void PauseScreen()
    {
        if (isPaused) Despausar();
        else Pausar();
    }

    public void Pausar()
    {
        isPaused = true;
        AutoSaveCurrentGame();

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Slider s = pausePanel.GetComponentInChildren<Slider>(true);
            if (s != null && SoundEffectManager.Instance != null)
            {
                SoundEffectManager.Instance.RegisterSlider(s);
            }
        }

        Time.timeScale = 0f;
    }

    public void Despausar()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);

        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.Transition(cena);
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.VoltarAoMenu();
            return;
        }

        SceneManager.LoadScene(cena);
    }

    private void AutoSaveCurrentGame()
    {
        if (SaveManager.Instance == null) return;

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            checkpointSceneName = CheckpointManager.Instance != null ? CheckpointManager.Instance.LastCheckpointScene : string.Empty,
            playerPosition = FindPlayerPosition(),
            checkpointPosition = CheckpointManager.Instance != null ? CheckpointManager.Instance.LastCheckpointPosition : Vector3.zero,
            checkpointReached = CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint,
            levelIndex = 0,
            coins = 0,
            playerHealth = 100
        };

        SaveManager.Instance.CaptureCurrentWorldState(data);
        SaveManager.Instance.SaveCurrentStateToSlot(SaveManager.AutoSaveSlot, data);
    }

    public void SalvarJogo()
    {
        if (SaveManager.Instance == null) return;
        SaveData data = SaveManager.Instance.BuildDefaultSave();
        SaveManager.Instance.SaveCurrentStateToSlot(SaveManager.ManualSaveSlot, data);
    }

    public void CarregarJogo()
    {
        if (SaveManager.Instance == null || !SaveManager.Instance.HasManualSave) return;

        if (SaveRestoreManager.Instance != null)
        {
            SaveRestoreManager.Instance.RequestLoadSlot(SaveManager.ManualSaveSlot);
            return;
        }

        SaveData data = SaveManager.Instance.LoadFromSlot(SaveManager.ManualSaveSlot);
        if (data != null && GameManager.Instance != null && !string.IsNullOrEmpty(data.sceneName))
        {
            GameManager.Instance.ForceSceneChange(data.sceneName);
        }
    }

    private Vector3 FindPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform.position : Vector3.zero;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}