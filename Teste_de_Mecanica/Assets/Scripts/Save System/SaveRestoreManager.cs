using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveRestoreManager : MonoBehaviour
{
    public static SaveRestoreManager Instance { get; private set; }

    private SaveData pendingSave;
    private int pendingSlot = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void RequestLoadSlot(int slot)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveRestoreManager: SaveManager not found.");
            return;
        }

        SaveData data = SaveManager.Instance.LoadFromSlot(slot);
        if (data == null)
        {
            Debug.LogWarning($"SaveRestoreManager: no save found in slot {slot}.");
            return;
        }

        pendingSave = data;
        pendingSlot = slot;

        string targetScene = !string.IsNullOrEmpty(data.sceneName) ? data.sceneName : SaveManager.Instance.DefaultSceneName;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ForceSceneChange(targetScene);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingSave == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(pendingSave.sceneName) && pendingSave.sceneName != scene.name)
        {
            return;
        }

        ApplyPendingSave();
        pendingSave = null;
        pendingSlot = -1;
    }

    private void ApplyPendingSave()
    {
        if (pendingSave == null)
        {
            return;
        }

        Vector3 targetPosition = pendingSave.checkpointReached && pendingSave.checkpointPosition != Vector3.zero
            ? pendingSave.checkpointPosition
            : pendingSave.playerPosition;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && targetPosition != Vector3.zero)
        {
            player.transform.position = targetPosition;
        }

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.AtualizarCheckpoint(pendingSave.sceneName, targetPosition);
        }

        Debug.Log($"SaveRestoreManager: applied pending save from slot {pendingSlot}.");
    }
}
