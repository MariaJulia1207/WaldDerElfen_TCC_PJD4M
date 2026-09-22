/*
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Dados do Inimigo")]
    [SerializeField] private EnemyData enemyData;

    private Transform player;
    private Rigidbody2D rb;

    private bool perseguindo;
    private bool atacando;

    public Transform Player => player;
    public bool Perseguindo => perseguindo;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (atacando)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!perseguindo || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direcao = ((Vector2)player.position - rb.position).normalized;

        rb.linearVelocity = direcao * enemyData.velocidade;
    }

    public void ComecarPerseguicao(Transform jogador)
    {
        player = jogador;
        perseguindo = true;
    }

    public void PararPerseguicao()
    {
        perseguindo = false;
        player = null;

        rb.linearVelocity = Vector2.zero;
    }

    public void DefinirAtacando(bool estado)
    {
        atacando = estado;
    }
}
*/