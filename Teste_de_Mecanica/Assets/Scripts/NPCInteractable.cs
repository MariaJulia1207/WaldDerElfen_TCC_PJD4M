
using UnityEngine;
using UnityEngine.InputSystem;


public class NPCInteractable : MonoBehaviour
{
    [SerializeField]
    private DialogueData dialogue;

    public DialogueData GetDialogue()
    {
        return dialogue;
    }

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }
}