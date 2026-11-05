using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private GameObject areaEntranceObject;

    public void OpenDoor()
    {
        if (areaEntranceObject != null)
        {
            areaEntranceObject.SetActive(true);
        }

        Destroy(gameObject);
    }
}