using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private string saveId = "Porta_1";
    [SerializeField] private GameObject areaEntranceObject;

    public string DoorId => saveId;

    private void Start()
    {
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