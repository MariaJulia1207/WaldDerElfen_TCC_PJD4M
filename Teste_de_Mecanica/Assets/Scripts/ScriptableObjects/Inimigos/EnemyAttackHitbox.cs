using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("Dados do Inimigo")]
    [SerializeField] private EnemyData enemyData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se atingiu o Player
        if (!other.CompareTag("Player"))
            return;

        // Procura o HealthSystem no Player
        HealthSystem healthSystem = other.GetComponent<HealthSystem>();

        if (healthSystem != null)
        {
            healthSystem.ReceberDano(enemyData.dano);
        }
    }
}