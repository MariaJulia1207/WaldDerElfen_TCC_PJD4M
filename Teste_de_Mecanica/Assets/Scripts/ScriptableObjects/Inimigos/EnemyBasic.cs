using UnityEngine;

public class EnemyBasic : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 2f;

    private Transform player;
    private Rigidbody2D rb;

    private bool perseguindo;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!perseguindo || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direcao = ((Vector2)player.position - rb.position).normalized;

        rb.linearVelocity = direcao * velocidade;
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
}