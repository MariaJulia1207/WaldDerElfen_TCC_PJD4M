using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void ReceberCura(int cura)
    {
        if (isDead || cura <= 0)
            return;

        vida += cura;

        if (vida > vidaMaxima)
        {
            vida = vidaMaxima;
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
    // MORTE - ANIMATION EVENT
    // =========================================================

    public void Morrer()
    {
        Time.timeScale = 0f;

        string nomeCenaGameOver = GameManager.Instance != null ? GameManager.Instance.NomeCenaGameOver : "GameOver";

        Scene cenaGameOver = SceneManager.GetSceneByName(nomeCenaGameOver);

        if (!cenaGameOver.isLoaded)
        {
            SceneManager.LoadScene(nomeCenaGameOver, LoadSceneMode.Additive);
        }
    }

    public void RestaurarParaCheckpoint()
    {
        vida = vidaMaxima;
        isDead = false;

        if (player != null)
        {
            player.enabled = true;
            player.IniciarRespawn();
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (sprite != null)
        {
            sprite.color = corOriginal;
        }

        StartCoroutine(EsperarFinalRespawn());
    }

    private IEnumerator EsperarFinalRespawn()
    {
        yield return new WaitForSeconds(3.1f);

        if (player != null)
        {
            player.FinalizarRespawn();
        }
    }
}