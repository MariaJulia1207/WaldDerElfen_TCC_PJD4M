using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private string saveId;
    [SerializeField] private GameObject areaEntranceObject;

    private void Start()
    {
        // Se a porta já foi aberta neste save, ativa a entrada da área e remove a porta fechada
        if (!string.IsNullOrEmpty(saveId) && SaveManager.Instance != null)
        {
            if (SaveManager.Instance.IsCoinCollected(saveId))
            {
                OpenDoorInternal();
            }
        }
    }

    public void OpenDoor()
    {
        // Marca a porta como aberta no SaveManager
        if (!string.IsNullOrEmpty(saveId) && SaveManager.Instance != null)
        {
            SaveManager.Instance.MarkCoinCollected(saveId);
        }

        OpenDoorInternal();
    }

    private void OpenDoorInternal()
    {
        if (areaEntranceObject != null)
        {
            areaEntranceObject.SetActive(true);
        }

        Destroy(gameObject);
    }
}