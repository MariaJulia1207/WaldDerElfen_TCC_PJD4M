using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Dados")]
    [SerializeField] private EnemyData enemyData;

    [Header("Hitboxes")]
    [SerializeField] private GameObject attackUp;
    [SerializeField] private GameObject attackDown;
    [SerializeField] private GameObject attackLeft;
    [SerializeField] private GameObject attackRight;

    [Header("Referências")]
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private EnemyAnimator enemyAnimator;

    private float proximoAtaque;

    private void Start()
    {
        DesativarTodasHitboxes();
    }

    private void Update()
    {
        if (enemyAI.Player == null)
            return;

        float distancia = Vector2.Distance(
            transform.position,
            enemyAI.Player.position
        );

        if (distancia <= enemyData.distanciaAtaque &&
            Time.time >= proximoAtaque)
        {
            Atacar();
        }
    }

    private void Atacar()
    {
        proximoAtaque = Time.time + enemyData.tempoEntreAtaques;

        Vector2 direcao = (
            enemyAI.Player.position - transform.position
        ).normalized;

        enemyAI.DefinirAtacando(true);
        enemyAnimator.DefinirAtacando(true);

        AtivarHitbox(direcao);
    }

    private void AtivarHitbox(Vector2 direcao)
    {
        DesativarTodasHitboxes();

        if (Mathf.Abs(direcao.x) > Mathf.Abs(direcao.y))
        {
            if (direcao.x > 0)
                attackRight.SetActive(true);
            else
                attackLeft.SetActive(true);
        }
        else
        {
            if (direcao.y > 0)
                attackUp.SetActive(true);
            else
                attackDown.SetActive(true);
        }
    }

    public void FinalizarAtaque()
    {
        DesativarTodasHitboxes();

        enemyAI.DefinirAtacando(false);
        enemyAnimator.DefinirAtacando(false);
    }

    private void DesativarTodasHitboxes()
    {
        attackUp.SetActive(false);
        attackDown.SetActive(false);
        attackLeft.SetActive(false);
        attackRight.SetActive(false);
    }
}