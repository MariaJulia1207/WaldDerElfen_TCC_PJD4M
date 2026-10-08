using UnityEngine;

public class BossRaiz : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D hitbox;

    [Header("Dano")]
    [SerializeField] private int dano = 1;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 4f;

    private BossRaizPool pool;

    private bool ativa;
    private bool jogadorAtingido;

    private const string ESTADO_ATACAR = "Atacar";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (hitbox == null)
            hitbox = GetComponent<Collider2D>();

        DesativarHitbox();
    }

    // =========================================================
    // POOL
    // =========================================================

    public void DefinirPool(BossRaizPool novoPool)
    {
        pool = novoPool;
    }

    // =========================================================
    // ATIVAR
    // =========================================================

    public void Ativar(Vector3 posicao)
    {
        transform.position = posicao;

        ativa = true;
        jogadorAtingido = false;

        gameObject.SetActive(true);

        DesativarHitbox();

        if (animator != null)
        {
            animator.Play(
                ESTADO_ATACAR,
                0,
                0f
            );
        }
        else
        {
            Debug.LogError(
                "BossRaiz: Animator não encontrado.",
                this
            );
        }
    }

    // =========================================================
    // ANIMATION EVENT
    // =========================================================

    public void AtivarHitbox()
    {
        if (!ativa)
            return;

        jogadorAtingido = false;

        if (hitbox != null)
            hitbox.enabled = true;
    }

    // =========================================================
    // ANIMATION EVENT
    // =========================================================

    public void DesativarHitbox()
    {
        if (hitbox != null)
            hitbox.enabled = false;
    }

    // =========================================================
    // DANO
    // =========================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!ativa)
            return;

        if (jogadorAtingido)
            return;

        if (!other.CompareTag("Player"))
            return;

        HealthSystem health =
            other.GetComponent<HealthSystem>();

        if (health != null)
        {
            health.ReceberDano(dano);
        }

        Rigidbody2D rb =
            other.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 direcao =
                (
                    other.transform.position -
                    transform.position
                ).normalized;

            rb.AddForce(
                direcao * forcaKnockback,
                ForceMode2D.Impulse
            );
        }

        jogadorAtingido = true;
    }

    // =========================================================
    // FINAL DA ANIMAÇÃO
    // =========================================================

    public void FinalizarRaiz()
    {
        if (!ativa)
            return;

        DesativarHitbox();

        ativa = false;

        if (pool != null)
        {
            pool.DevolverRaiz(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public bool EstaAtiva()
    {
        return ativa;
    }
}