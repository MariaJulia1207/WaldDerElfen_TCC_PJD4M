using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Dados")]
    [SerializeField] private EnemyData enemyData;

    [Header("Detecção")]
    [SerializeField] private EnemyDetector detectionArea;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (detectionArea == null)
            return;

        if (detectionArea.detectedObjs.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Collider2D target = detectionArea.detectedObjs[0];

        if (target == null)
            return;

        Vector2 direcao =
            (target.transform.position - transform.position).normalized;

        rb.linearVelocity = direcao * enemyData.velocidade;
    }
}