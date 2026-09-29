using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

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

    public void SetPlayerLevel(int level, int slot = 0)
    {
        if (SaveManager.Instance == null)
        {
            return;
        }

        SaveData data = SaveManager.Instance.LoadFromSlot(slot) ?? SaveManager.Instance.BuildDefaultSave();
        data.levelIndex = level;
        SaveManager.Instance.SaveCurrentStateToSlot(slot, data);
    }

    public void SetPlayerName(string playerName, int slot = 0)
    {
        _ = playerName;
        _ = slot;
    }

    public void SaveDataInFile(int slot = 0, SaveData data = null)
    {
        if (SaveManager.Instance == null)
        {
            return;
        }

        SaveManager.Instance.SaveCurrentStateToSlot(slot, data ?? SaveManager.Instance.BuildDefaultSave());
    }

    public bool LoadDataInFile(int slot = 0)
    {
        if (SaveManager.Instance == null)
        {
            return false;
        }

        return SaveManager.Instance.LoadFromSlot(slot) != null;
    }
}