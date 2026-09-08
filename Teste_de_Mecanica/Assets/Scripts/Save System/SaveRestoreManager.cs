/*
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveRestoreManager : MonoBehaviour
{
    public static SaveRestoreManager Instance { get; private set; }

    private SaveData pendingSave;
    private int pendingSlot = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
            Debug.LogWarning("SaveManager not found");
            return;
        }

        var data = SaveManager.Instance.LoadFromSlot(slot);
        if (data == null)
        {
            Debug.LogWarning($"No save in slot {slot}");
            return;
        }

        pendingSave = data;
        pendingSlot = slot;

        // If save points to a scene, load it
        if (!string.IsNullOrEmpty(data.sceneName))
        {
            GameManager.Instance.ForceSceneChange(data.sceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingSave == null) return;

        // If pending save scene specified and doesn't match current, wait
        if (!string.IsNullOrEmpty(pendingSave.sceneName) && pendingSave.sceneName != scene.name) return;

        // Apply to player and HUD
        var player = FindObjectOfType<TwoBallController>();
        if (player != null)
        {
            player.transform.position = pendingSave.playerPosition;
        }

        var hud = FindObjectOfType<HUDController>();
        if (hud != null)
        {
            hud.SetCoins(pendingSave.coins);
        }

        Debug.Log($"Applied pending save from slot {pendingSlot} after scene load {scene.name}");

        // clear
        pendingSave = null;
        pendingSlot = -1;
    }
}
*/