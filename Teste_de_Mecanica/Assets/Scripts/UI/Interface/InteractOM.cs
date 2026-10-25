using UnityEngine;
using UnityEngine.InputSystem;

public class InteractOM : MonoBehaviour
{
    public static InteractOM Instance { get; private set; }

    private NPCInteractable currentNPC;

    public NPCInteractable CurrentNPC => currentNPC;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Ignora se o diálogo estiver aberto OU se acabou de fechar no frame atual
            if (DialogueManager.Instance != null &&
               (DialogueManager.Instance.IsDialogueOpen || DialogueManager.Instance.JustClosedThisFrame))
            {
                return;
            }

            Interact();
        }
    }

    public void SetCurrentNPC(NPCInteractable npc)
    {
        currentNPC = npc;
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