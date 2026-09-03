using System.Collections;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Vida")]
    public bool isDead;
    public int vida;
    public int vidaMaxima;

    [Header("Visual do Player")]
    [SerializeField] private SpriteRenderer sprite;

    private PlayerController player;
    private Color corOriginal;

    private Coroutine flashCoroutine;

    private void Start()
    {
        player = GetComponent<PlayerController>();

        if (sprite == null)
        {
            sprite = GetComponent<SpriteRenderer>();
        }

        if (sprite != null)
        {
            corOriginal = sprite.color;
        }
    }

    private void Update()
    {
        DeadState();
    }

    // =========================================================
    // RECEBER DANO
    // =========================================================

    public void ReceberDano(int dano)
    {
        // Se já estiver morto, não recebe mais dano
        if (isDead)
            return;

        vida -= dano;

        // Evita vida negativa
        if (vida < 0)
        {
            vida = 0;
        }

        // Flash vermelho
        if (vida > 0)
        {
            flashCoroutine = StartCoroutine(FlashVermelho());
        }
    }

    // =========================================================
    // FLASH VERMELHO
    // =========================================================

    private IEnumerator FlashVermelho()
    {
        if (sprite == null)
            yield break;

        sprite.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        // Só restaura se o jogador ainda estiver vivo
        if (!isDead)
        {
            sprite.color = corOriginal;
        }
    }

    // =========================================================
    // MORTE
    // =========================================================

    private void DeadState()
    {
        if (vida <= 0 && !isDead)
        {
            isDead = true;

            // Cancela qualquer flash que ainda esteja acontecendo
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            // IMPORTANTE:
            // Remove imediatamente o vermelho antes da animação Death
            if (sprite != null)
            {
                sprite.color = corOriginal;
            }

            // Para o PlayerController
            if (player != null)
            {
                player.enabled = false;
            }

            // Para o Rigidbody
            Rigidbody2D rb = GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Inicia animação de morte
            if (player != null && player.Anim != null)
            {
                player.Anim.SetBool("IsDead", true);
            }
        }
    }

    // =========================================================
    // DESTROY - ANIMATION EVENT
    // =========================================================

    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}