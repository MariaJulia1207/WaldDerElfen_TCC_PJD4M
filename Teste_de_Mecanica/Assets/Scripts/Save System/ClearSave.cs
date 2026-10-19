using System.IO;
using UnityEngine;

public class ClearSave : MonoBehaviour
{
    [ContextMenu("Clear All Saves")]
    public void ClearAllSaves()
    {
        int firstSlot = SaveManager.AutoSaveSlot;
        int lastSlot = SaveManager.LastManualSlot;

        for (int slot = firstSlot; slot <= lastSlot; slot++)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.ClearSlot(slot);
            }

            string[] possiblePaths =
            {
                Path.Combine(Application.persistentDataPath, "platform_save_slot_" + slot + ".sav"),
                Path.Combine(Application.persistentDataPath, "save_slot_" + slot + ".sav"),
                SaveManager.Instance != null ? SaveManager.Instance.GetPath(slot) : string.Empty
            };

            foreach (string path in possiblePaths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        Debug.Log("Todos os saves foram limpos. Caminho: " + Application.persistentDataPath);
    }
}
