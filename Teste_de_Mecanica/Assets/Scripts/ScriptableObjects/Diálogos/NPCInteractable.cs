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
        if (dialogueData == null)
        {
            Debug.LogError($"[NPCInteractable] ERRO CRÍTICO: DialogueData NÃO está atribuído no Inspector de '{gameObject.name}'!");
            return;
        }

        Debug.Log($"[NPCInteractable] Chamando DialogueManager para o NPC '{gameObject.name}'");

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                dialogueData,
                this,
                dialogueCompleted);
        }
        else
        {
            Debug.LogError("[NPCInteractable] DialogueManager.Instance NÃO foi encontrado na cena!");
        }
    }

    public void CompleteDialogue()
    {
        dialogueCompleted = true;
        Debug.Log($"[NPCInteractable] Diálogo marcado como concluído para '{gameObject.name}'");
        MoverNpcSeNecessario();
    }

    public void ApplySaveState(bool completed)
    {
        dialogueCompleted = completed;

        if (dialogueCompleted)
        {
            MoverNpcSeNecessario();
        }
    }

    private void MoverNpcSeNecessario()
    {
        if (dialogueData != null && dialogueData.moverNpcAposDialogo)
        {
            transform.position = dialogueData.posicaoDestinoNpc;
        }
    }
}