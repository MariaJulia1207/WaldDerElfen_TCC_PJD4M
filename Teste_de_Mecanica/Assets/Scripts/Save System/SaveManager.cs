/*
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    string GetPath(int slot) => Path.Combine(Application.persistentDataPath, $"save{slot}.json");

    public void SaveToSlot(int slot, SaveData data) {
        var json = JsonUtility.ToJson(data);
        File.WriteAllText(GetPath(slot), json);
        Debug.Log($"Saved slot {slot} -> {GetPath(slot)}");
    }

    public SaveData LoadFromSlot(int slot) {
        var path = GetPath(slot);
        if (!File.Exists(path)) return null;
        var json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public bool SlotExists(int slot) => File.Exists(GetPath(slot));
}
*/