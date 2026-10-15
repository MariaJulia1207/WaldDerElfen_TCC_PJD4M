using UnityEngine;

public class BossHitboxBraco : MonoBehaviour
{
    [Header("Configurações de Ataque")]
    [SerializeField] private int dano = 1;
    [SerializeField] private float forcaKnockback = 6f;

    private bool jogadorAtingido;

    /// <summary>
    /// Reseta o estado da hitbox para permitir acerto no novo ataque.
    /// Chamado pelo BossArvore ao iniciar o golpe.
    /// </summary>
    public void ResetarHitbox()
    {
        jogadorAtingido = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Garante que só atinge uma vez por ataque
        if (jogadorAtingido)
            return;

        // Verifica a tag do jogador
        if (!other.CompareTag("Player"))
            return;

        // Aplica Dano usando o HealthSystem existente
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.ReceberDano(dano);
        }

        // Aplica Knockback
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Calcula a direção do impacto (do braço em direção ao jogador)
            Vector2 direcaoKnockback = (other.transform.position - transform.position).normalized;

            // Se estiverem exatamente na mesma posição, empurra para baixo
            if (direcaoKnockback == Vector2.zero)
            {
                direcaoKnockback = Vector2.down;
            }

            rb.AddForce(direcaoKnockback * forcaKnockback, ForceMode2D.Impulse);
        }

        jogadorAtingido = true;
    }
}