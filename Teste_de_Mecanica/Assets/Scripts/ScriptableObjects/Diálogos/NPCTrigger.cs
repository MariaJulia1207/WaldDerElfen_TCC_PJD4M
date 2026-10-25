using UnityEngine;

public class NPCTrigger : MonoBehaviour
{
    private NPCInteractable npc;
    private int playerColliderCount = 0;

    private void Awake()
    {
        npc = GetComponent<NPCInteractable>();

        if (npc == null)
        {
            Debug.LogError($"[NPCTrigger] NPCInteractable NÃO ENCONTRADO no objeto: {gameObject.name}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerColliderCount++;
            Debug.Log($"[NPCTrigger] Enter por '{other.gameObject.name}'. Total de colisores do Player no trigger: {playerColliderCount}");

            if (playerColliderCount == 1)
            {
                if (InteractOM.Instance != null)
                {
                    Debug.Log($"[NPCTrigger] Definindo NPC atual: {npc.gameObject.name}");
                    InteractOM.Instance.SetCurrentNPC(npc);
                }
                else
                {
                    Debug.LogError("[NPCTrigger] InteractOM.Instance está NULO!");
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerColliderCount--;
            if (playerColliderCount < 0) playerColliderCount = 0;

            Debug.Log($"[NPCTrigger] Exit por '{other.gameObject.name}'. Total de colisores do Player no trigger: {playerColliderCount}");

            if (playerColliderCount == 0)
            {
                if (InteractOM.Instance != null)
                {
                    Debug.Log($"[NPCTrigger] Limpando NPC atual: {npc.gameObject.name}");
                    InteractOM.Instance.ClearCurrentNPC();
                }
            }
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null) return false;

        // Ignora colisores de gatilho nos filhos (ex: Collector, AttackHitboxes)
        if (other.gameObject != other.transform.root.gameObject && other.isTrigger)
        {
            return false;
        }

        if (other.CompareTag("Player"))
            return true;

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
            return true;

        if (other.transform.root != null && other.transform.root.CompareTag("Player"))
            return true;

        return false;
    }

    private void OnDisable()
    {
        playerColliderCount = 0;
    }
}