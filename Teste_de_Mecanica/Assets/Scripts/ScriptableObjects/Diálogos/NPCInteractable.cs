using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField]
    private DialogueData dialogueData;
    [SerializeField]
    private string saveId;

    private bool dialogueCompleted;

    public string SaveId => string.IsNullOrEmpty(saveId) ? gameObject.name : saveId;
    public bool IsDialogueCompleted => dialogueCompleted;

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

    public void ApplySaveState(bool completed)
    {
        dialogueCompleted = completed;
    }
}