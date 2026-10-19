using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public string LastCheckpointScene { get; private set; }
    public Vector3 LastCheckpointPosition { get; private set; }
    public bool HasCheckpoint => !string.IsNullOrEmpty(LastCheckpointScene);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void AtualizarCheckpoint(string sceneName, Vector3 position)
    {
        LastCheckpointScene = sceneName;
        LastCheckpointPosition = position;
        PersistCheckpointState();
    }

    public SaveData BuildCheckpointSave()
    {
        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            checkpointSceneName = LastCheckpointScene,
            checkpointPosition = LastCheckpointPosition,
            checkpointReached = true,
            playerPosition = Vector3.zero,
            playerHealth = 100
        };

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition = player.transform.position;

            HealthSystem health = player.GetComponent<HealthSystem>();
            if (health != null)
            {
                data.playerHealth = Mathf.Clamp(health.vida, 0, Mathf.Max(1, health.vidaMaxima));
            }
        }

        if (!string.IsNullOrEmpty(LastCheckpointScene))
        {
            data.checkpointReached = true;
            data.checkpointSceneName = LastCheckpointScene;
            data.checkpointPosition = LastCheckpointPosition;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CaptureCurrentWorldState(data);
        }

        data.checkpointReached = true;
        data.checkpointSceneName = LastCheckpointScene;
        data.checkpointPosition = LastCheckpointPosition;

        return data;
    }

    public void PersistCheckpointState()
    {
        if (SaveManager.Instance == null)
        {
            return;
        }

        SaveData data = BuildCheckpointSave();
        data.checkpointReached = true;
        data.checkpointSceneName = LastCheckpointScene;
        data.checkpointPosition = LastCheckpointPosition;

        if (string.IsNullOrEmpty(data.checkpointSceneName))
        {
            return;
        }

        SaveManager.Instance.SaveCurrentStateToSlot(SaveManager.AutoSaveSlot, data);
    }

    public bool RestoreCheckpointFromAutoSave(bool unloadGameOverScene = true)
    {
        if (unloadGameOverScene && GameManager.Instance != null)
        {
            string nomeCenaGameOver = GameManager.Instance.NomeCenaGameOver;
            Scene cenaGameOver = SceneManager.GetSceneByName(nomeCenaGameOver);

            if (cenaGameOver.isLoaded)
            {
                SceneManager.UnloadSceneAsync(nomeCenaGameOver);
            }
        }

        if (SaveManager.Instance == null)
        {
            return false;
        }

        SaveData checkpointSave = SaveManager.Instance.LoadFromSlot(SaveManager.AutoSaveSlot);
        if (checkpointSave == null || !checkpointSave.checkpointReached || string.IsNullOrEmpty(checkpointSave.checkpointSceneName))
        {
            return false;
        }

        LastCheckpointScene = checkpointSave.checkpointSceneName;
        LastCheckpointPosition = checkpointSave.checkpointPosition;

        if (!string.Equals(SceneManager.GetActiveScene().name, LastCheckpointScene, System.StringComparison.Ordinal))
        {
            SceneManager.LoadScene(LastCheckpointScene);
            return true;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = checkpointSave.checkpointPosition;

            HealthSystem health = player.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.vida = Mathf.Clamp(checkpointSave.playerHealth, 0, Mathf.Max(1, health.vidaMaxima));
                health.isDead = false;
                health.RestaurarParaCheckpoint();
            }
        }

        if (SaveRestoreManager.Instance != null)
        {
            SaveRestoreManager.Instance.ApplySaveData(checkpointSave);
        }

        return true;
    }

    public void RestaurarPlayerNoCheckpoint(bool unloadGameOverScene = true)
    {
        if (unloadGameOverScene && GameManager.Instance != null)
        {
            string nomeCenaGameOver = GameManager.Instance.NomeCenaGameOver;
            Scene cenaGameOver = SceneManager.GetSceneByName(nomeCenaGameOver);

            if (cenaGameOver.isLoaded)
            {
                SceneManager.UnloadSceneAsync(nomeCenaGameOver);
            }
        }

        if (!HasCheckpoint)
        {
            return;
        }

        string cenaAtual = SceneManager.GetActiveScene().name;
        if (!string.Equals(cenaAtual, LastCheckpointScene, System.StringComparison.Ordinal))
        {
            if (!string.IsNullOrEmpty(LastCheckpointScene))
            {
                SceneManager.LoadScene(LastCheckpointScene);
            }
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        CameraFollowPlayer cameraFollow = FindAnyObjectByType<CameraFollowPlayer>();

        if (cameraFollow != null)
        {
            cameraFollow.DesativarSeguido();
        }

        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.DisableControls();
            }

            player.transform.position = LastCheckpointPosition;

            if (cameraFollow != null)
            {
                cameraFollow.SnapToTarget(player.transform);
                cameraFollow.ReativarSeguido(player.transform);
            }

            HealthSystem health = player.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.RestaurarParaCheckpoint();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!HasCheckpoint || scene.name != LastCheckpointScene)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = LastCheckpointPosition;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
