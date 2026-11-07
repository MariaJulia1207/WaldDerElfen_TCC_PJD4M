using UnityEngine;

public class Key : MonoBehaviour, IItem
{
    [SerializeField] private string keyId = "Chave_Porta_1";
    [SerializeField] private string targetDoorId = "Porta_1";

    private void Start()
    {
        if (!string.IsNullOrEmpty(keyId) && SaveManager.Instance != null)
        {
            if (SaveManager.Instance.IsCoinCollected(keyId))
            {
                Destroy(gameObject);
            }
        }
    }

    public void Collect()
    {
        if (!string.IsNullOrEmpty(keyId) && SaveManager.Instance != null)
        {
            SaveManager.Instance.MarkCoinCollected(keyId);
        }

        // Procura a porta correspondente na cena pelo ID
        if (!string.IsNullOrEmpty(targetDoorId))
        {
            Door[] doors = FindObjectsByType<Door>(FindObjectsSortMode.None);
            foreach (Door door in doors)
            {
                if (door.DoorId == targetDoorId)
                {
                    door.OpenDoor();
                    break;
                }
            }
        }

        Destroy(gameObject);
    }
}