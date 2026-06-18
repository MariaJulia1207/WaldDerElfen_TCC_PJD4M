using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 5f;

    private Rigidbody2D rb;
    private Vector2 movimento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movimento = Vector2.zero;

        // Esquerda
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            movimento.x = -1;
        }

        // Direita
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            movimento.x = 1;
        }

        // Cima
        if (Keyboard.current.upArrowKey.isPressed)
        {
            movimento.y = 1;
        }

        // Baixo
        if (Keyboard.current.downArrowKey.isPressed)
        {
            movimento.y = -1;
        }

        // Corrige velocidade diagonal
        movimento = movimento.normalized;
        
        // Diálogo
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (DialogueManager.Instance.IsDialogueActive())
            {
                DialogueManager.Instance.NextLine();
            }
            else
            {
                NPCInteraction npc =
                    ObserverManager.Instance.GetCurrentNPC();

                if (npc != null)
                {
                    npc.Interact();
                }
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movimento * velocidade;
    }
}