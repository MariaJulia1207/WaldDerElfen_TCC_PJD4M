using UnityEngine;

public class Key : MonoBehaviour, IItem
{
    [SerializeField] private Door targetDoor;

    public void Collect()
    {
        if (targetDoor != null)
        {
            targetDoor.OpenDoor();
        }

        Destroy(gameObject);
    }
}