using UnityEngine;

public class NPCTrigger : MonoBehaviour
{
    private NPCInteractable npc;

    private void Awake()
    {
        npc = GetComponent<NPCInteractable>();

        if (npc == null)
        {
            Debug.LogError(
                $"NPCInteractable não encontrado em {gameObject.name}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entrou no trigger: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detectado");

            InteractOM.Instance.SetCurrentNPC(npc);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (InteractOM.Instance == null)
            return;

        InteractOM.Instance.ClearCurrentNPC();
    }
    
}