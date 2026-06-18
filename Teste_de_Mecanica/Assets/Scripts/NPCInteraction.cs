using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public DialogueData dialogue;

    private bool playerInside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        ObserverManager.Instance.SetCurrentNPC(this);

        InteractButtonController.Instance.ShowButton(
            transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        InteractButtonController.Instance.HideButton();

        if (ObserverManager.Instance.GetCurrentNPC() == this)
        {
            ObserverManager.Instance.EndInteraction();
        }
    }

    public void Interact()
    {
        if (!playerInside)
            return;

        bool jaConversou =
            ObserverManager.Instance.HasTalked(
                dialogue.npcID);

        DialogueManager.Instance.StartDialogue(
            dialogue,
            jaConversou);
    }
}