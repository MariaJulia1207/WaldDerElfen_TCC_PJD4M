using UnityEngine;

public class RaizHitbox : MonoBehaviour
{
    [Header("Dano")]
    [SerializeField] private int dano = 1;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 4f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        HealthSystem health = other.GetComponent<HealthSystem>();

        if (health != null)
        {
            health.ReceberDano(dano);
        }

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 direcao =
                (other.transform.position - transform.position).normalized;

            rb.AddForce(
                direcao * forcaKnockback,
                ForceMode2D.Impulse
            );
        }
    }
}