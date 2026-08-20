using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField]
    private DialogueData dialogueData;

    private bool dialogueCompleted;

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(
            dialogueData,
            this,
            dialogueCompleted);
    }

    public void CompleteDialogue()
    {
        dialogueCompleted = true;
    }
}