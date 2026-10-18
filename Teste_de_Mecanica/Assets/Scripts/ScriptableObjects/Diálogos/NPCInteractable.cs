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

        // Move o NPC imediatamente se o diálogo recém-concluído tiver a opção ativada
        MoverNpcSeNecessario();
    }

    public void ApplySaveState(bool completed)
    {
        dialogueCompleted = completed;

        // Reposiciona o NPC ao carregar um jogo onde o diálogo já foi concluído
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