using UnityEngine;

public class InteractOM : MonoBehaviour
{
    public static InteractOM Instance;

    private NPCInteractable currentNPC;

    private void Awake()
    {
        Instance = this;
    }

    public void SetCurrentNPC(NPCInteractable npc)
    {
        currentNPC = npc;

        Debug.Log("Enviando evento ShowInteractButton");

        ObserverManager.Notify("ShowInteractButton");
    }

    public void ClearCurrentNPC()
    {
        currentNPC = null;

        ObserverManager.Notify("HideInteractButton");
    }

    public void Interact()
    {
        if (currentNPC != null)
        {
            currentNPC.Interact();
        }
    }
}