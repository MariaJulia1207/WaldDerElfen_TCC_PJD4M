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
        if (isDead)
            return;

        vida -= dano;

        if (vida < 0)
        {
            vida = 0;
        }

        StartCoroutine(FlashVermelho());
    }

    // =========================================================
    // FLASH
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

            StartCoroutine(EsperarMorte());
        }
    }

    private IEnumerator EsperarMorte()
    {
        yield return null;

        Animator animator = player.Anim;

        while (true)
        {
            AnimatorStateInfo estado =
                animator.GetCurrentAnimatorStateInfo(0);

            if (estado.IsName("Death") &&
                estado.normalizedTime >= 1f)
            {
                break;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}