using UnityEngine;

public class BossRaiz : MonoBehaviour
{
    [Header("Animação")]
    [SerializeField] private Animator animator;

    [Header("Hitbox")]
    [SerializeField] private GameObject hitbox;

    private BossRaizPool pool;

    private bool ativa = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (hitbox != null)
            hitbox.SetActive(false);
    }

    // =========================================================
    // POOL
    // =========================================================

    public void DefinirPool(BossRaizPool novaPool)
    {
        pool = novaPool;
    }

    // =========================================================
    // ATIVAR
    // =========================================================

    public void Ativar(Vector3 posicao)
    {
        transform.position = posicao;

        ativa = true;

        gameObject.SetActive(true);

        if (hitbox != null)
            hitbox.SetActive(false);

        if (animator != null)
        {
            animator.Play(
                "Invocacao",
                0,
                0f
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

        if (hitbox != null)
            hitbox.SetActive(true);
    }

    // =========================================================
    // FINAL DA ANIMAÇÃO
    // =========================================================

    public void FinalizarRaiz()
    {
        if (hitbox != null)
            hitbox.SetActive(false);

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

    // =========================================================
    // ESTADO
    // =========================================================

    public bool EstaAtiva()
    {
        return ativa;
    }
}