using UnityEngine;

public class EnemyTriggerDamage : MonoBehaviour
{
    [SerializeField] private int dano = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HealthSystem health =
                collision.gameObject.GetComponent<HealthSystem>();

            if (health != null)
            {
                health.ReceberDano(dano);
            }
        }
    }
}