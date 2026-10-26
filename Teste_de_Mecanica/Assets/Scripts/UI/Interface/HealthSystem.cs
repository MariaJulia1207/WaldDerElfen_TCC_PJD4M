using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    // Armazena os valores globalmente entre trocas de cena
    private static int vidaSalva = -1;
    private static int vidaMaximaSalva = -1;

    [Header("Vida")]
    public bool isDead;
    public int vida;
    public int vidaMaxima;

    [Header("Visual do Player")]
    [SerializeField] private SpriteRenderer sprite;

    private PlayerController player;
    private Color corOriginal;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // Se existir vida salva de uma cena anterior, restaura
        if (vidaSalva > 0)
        {
            vida = vidaSalva;
            vidaMaxima = vidaMaximaSalva > 0 ? vidaMaximaSalva : vidaMaxima;
        }
        else
        {
            // Primeira inicialização do jogo
            vidaSalva = vida;
            vidaMaximaSalva = vidaMaxima;
        }
    }

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

    /// <summary>
    /// Reseta os dados salvos globalmente para um novo jogo do zero.
    /// </summary>
    public static void ResetHealthData()
    {
        vidaSalva = -1;
        vidaMaximaSalva = -1;
    }

    private const float CircleAlpha = 75f / 255f;

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
            if (renderer == null) continue;

            renderer.enabled = true;

            if (string.Equals(renderer.name, "Circle", System.StringComparison.OrdinalIgnoreCase))
            {
                Color c = renderer.color;
                c.a = CircleAlpha;
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
        vidaSalva = vida;
        vidaMaximaSalva = vidaMaxima;
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
        if (isDead || dano <= 0)
            return;

        vida -= dano;

        if (vida < 0)
        {
            vida = 0;
        }

        // Atualiza a vida salva
        vidaSalva = vida;

        SoundEffectManager.Play("Damage");

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

        // Atualiza a vida salva
        vidaSalva = vida;
    }

    public void AumentarVidaMaxima(int quantidade)
    {
        vidaMaxima += quantidade;
        vida += quantidade;

        vidaMaximaSalva = vidaMaxima;
        vidaSalva = vida;
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

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            if (sprite != null)
            {
                sprite.color = corOriginal;
            }

            if (player != null)
            {
                player.enabled = false;
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

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

        Debug.LogWarning("HealthSystem: GameOverCanvas local não foi encontrado na cena.");
    }

    public void RestaurarParaCheckpoint()
    {
        vida = vidaMaxima;
        vidaSalva = vida;
        vidaMaximaSalva = vidaMaxima;
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
            if (renderer == null) continue;

            if (string.Equals(renderer.name, "Circle", System.StringComparison.OrdinalIgnoreCase))
            {
                Color c = renderer.color;
                c.a = CircleAlpha;
                renderer.color = c;
            }
        }
    }
}