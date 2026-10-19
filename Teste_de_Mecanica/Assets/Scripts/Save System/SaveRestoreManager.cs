using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveRestoreManager : MonoBehaviour
{
    public static SaveRestoreManager Instance { get; private set; }

    private SaveData pendingSave;
    private int pendingSlot = -1;
    private bool waitingForScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSavedSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSavedSceneLoaded;
        }
    }

    public void RequestLoadSlot(int slot)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveRestoreManager: SaveManager não encontrado.");
            return;
        }

        SaveData data = SaveManager.Instance.LoadFromSlot(slot);
        if (data == null)
        {
            Debug.LogWarning("SaveRestoreManager: não existe save no slot " + slot);
            return;
        }

        if (string.IsNullOrEmpty(data.sceneName))
        {
            Debug.LogWarning("SaveRestoreManager: save não possui cena.");
            return;
        }

        pendingSave = data.Clone();
        pendingSlot = slot;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetCurrentCollectedCoins(data.collectedImportantItems);
        }

        Time.timeScale = 1f;
        ForceReloadSavedScene();
    }

    private void ForceReloadSavedScene()
    {
        if (pendingSave == null || waitingForScene)
        {
            return;
        }

        waitingForScene = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ForceSceneChange(pendingSave.sceneName);
        }
        else
        {
            SceneManager.LoadScene(pendingSave.sceneName, LoadSceneMode.Single);
        }
    }

    private void OnSavedSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingSave == null)
        {
            return;
        }

        if (scene.name != pendingSave.sceneName)
        {
            return;
        }

        waitingForScene = false;
        StartCoroutine(ApplySaveNextFrame());
    }

    private IEnumerator ApplySaveNextFrame()
    {
        yield return null;

        SaveData data = pendingSave;
        int loadedSlot = pendingSlot;
        ApplySaveData(data);

        if (SaveManager.Instance != null && loadedSlot != SaveManager.AutoSaveSlot)
        {
            SaveManager.Instance.CopySlotToAutosave(loadedSlot);
        }

        pendingSave = null;
        pendingSlot = -1;
    }

    public void ApplySaveData(SaveData data)
    {
        if (data == null)
        {
            return;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetCurrentCollectedCoins(data.collectedImportantItems);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 respawnPosition = data.checkpointReached && !string.IsNullOrEmpty(data.checkpointSceneName)
                ? data.checkpointPosition
                : data.playerPosition;

            player.transform.position = respawnPosition;

            HealthSystem health = player.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.vida = Mathf.Clamp(data.playerHealth, 0, Mathf.Max(1, health.vidaMaxima));
                health.isDead = false;
            }
        }

        if (!string.IsNullOrEmpty(data.checkpointSceneName) && CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.AtualizarCheckpoint(data.checkpointSceneName, data.checkpointPosition);
        }

        NPCInteractable[] npcs = FindObjectsByType<NPCInteractable>(FindObjectsSortMode.None);
        foreach (NPCInteractable npc in npcs)
        {
            if (npc == null)
            {
                continue;
            }

            bool completed = data.interactedNPCs != null && data.interactedNPCs.Contains(npc.SaveId);
            npc.ApplySaveState(completed);
        }

        BossArena[] arenas = FindObjectsByType<BossArena>(FindObjectsSortMode.None);
        foreach (BossArena arena in arenas)
        {
            if (arena == null)
            {
                continue;
            }

            bool defeated = data.defeatedBosses != null && data.defeatedBosses.Contains(arena.SaveId);
            arena.ApplySaveState(defeated);
        }

        Debug.Log("SaveRestoreManager: save aplicado.");
    }
}
