using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("Dano")]
    [SerializeField] private int dano = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se atingiu o Player
        if (!other.CompareTag("Player"))
            return;

        // Procura o HealthSystem no Player
        HealthSystem healthSystem =
            other.GetComponent<HealthSystem>();

        if (healthSystem != null)
        {
            healthSystem.ReceberDano(dano);
        }
    }
}