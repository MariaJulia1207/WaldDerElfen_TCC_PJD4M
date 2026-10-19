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
            sprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (sprite != null)
        {
            corOriginal = sprite.color;
        }
    }

    private void EnsurePlayerVisible()
    {
        if (sprite == null)
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (sprite != null)
        {
            sprite.enabled = true;
            Color visibleColor = sprite.color;
            visibleColor.a = 1f;
            sprite.color = visibleColor;
            corOriginal = visibleColor;
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled = true;

            if (string.Equals(renderer.name, "Circle", System.StringComparison.OrdinalIgnoreCase))
            {
                Color c = renderer.color;
                c.a = 0.75f;
                renderer.color = c;
                continue;
            }

            Color mainColor = renderer.color;
            mainColor.a = 1f;
            renderer.color = mainColor;
        }
    }

    public void ResetToFullHealth()
    {
        vida = vidaMaxima;
        isDead = false;

        EnsurePlayerVisible();

        if (player != null)
        {
            player.EnableControls();
            player.IniciarRespawn();
            player.FinalizarRespawn();
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (sprite != null)
        {
            sprite.color = corOriginal;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
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
        if (isDead || dano <= 0)
            return;

        vida -= dano;

        // Evita vida negativa
        if (vida < 0)
        {
            vida = 0;
        }

        SoundEffectManager.Play("Damage");

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

        if (GameOverMenuController.Instance != null)
        {
            GameOverMenuController.Instance.ShowGameOver();
            return;
        }

        GameOverMenuController localInstance = FindAnyObjectByType<GameOverMenuController>(FindObjectsInactive.Include);
        if (localInstance != null)
        {
            localInstance.ShowGameOver();
            return;
        }

        Debug.LogWarning("HealthSystem: GameOverCanvas local não foi encontrado na cena. Verifique o LevelBootstrap e o prefab do canvas.");
    }

    public void RestaurarParaCheckpoint()
    {
        vida = vidaMaxima;
        isDead = false;

        EnsurePlayerVisible();

        if (player != null)
        {
            player.enabled = true;
            player.IniciarRespawn();
            player.FinalizarRespawn();

            if (player.Anim != null)
            {
                player.Anim.SetBool("IsDead", false);
            }
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

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            if (string.Equals(renderer.name, "Circle", System.StringComparison.OrdinalIgnoreCase))
            {
                Color c = renderer.color;
                c.a = 0.75f;
                renderer.color = c;
            }
        }
    }
}