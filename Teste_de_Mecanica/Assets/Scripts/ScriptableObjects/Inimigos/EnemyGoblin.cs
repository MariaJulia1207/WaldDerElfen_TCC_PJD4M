using System.Collections;
using UnityEngine;

public class EnemyMoblin : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 2f;
    [SerializeField] private float distanciaDeteccao = 5f;
    [SerializeField] private float distanciaAtaque = 1.2f;

    [Header("Ataque")]
    [SerializeField] private float tempoEntreAtaques = 1.5f;

    [Header("Hitboxes da Espada")]
    [SerializeField] private GameObject attackUp;
    [SerializeField] private GameObject attackDown;
    [SerializeField] private GameObject attackLeft;
    [SerializeField] private GameObject attackRight;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 2.5f;
    [SerializeField] private float duracaoKnockback = 0.12f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform jogador;

    private Vector2 movimento;
    private Vector2 ultimaDirecao = Vector2.down;

    private bool atacando;
    private bool sofrendoKnockback;

    private float proximoAtaque;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            jogador = player.transform;
        }

        DesativarTodasHitboxes();

        AtualizarDirecaoAnimacao(ultimaDirecao);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (jogador == null)
            return;

        // Durante ataque ou knockback o Moblin não recebe
        // comandos de movimento.
        if (atacando || sofrendoKnockback)
        {
            movimento = Vector2.zero;

            anim.SetBool("IsMoving", false);

            return;
        }

        float distancia =
            Vector2.Distance(transform.position, jogador.position);

        // -----------------------------------------------------
        // FORA DO ALCANCE DE DETECÇÃO
        // -----------------------------------------------------

        if (distancia > distanciaDeteccao)
        {
            Parar();
            return;
        }

        // -----------------------------------------------------
        // DENTRO DO ALCANCE DE ATAQUE
        // -----------------------------------------------------

        if (distancia <= distanciaAtaque)
        {
            Parar();

            if (Time.time >= proximoAtaque)
            {
                Atacar();
            }

            return;
        }

        // -----------------------------------------------------
        // PERSEGUIR
        // -----------------------------------------------------

        PerseguirJogador();
    }

    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        if (atacando || sofrendoKnockback)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = movimento * velocidade;
    }

    // =========================================================
    // PERSEGUIR JOGADOR
    // =========================================================

    private void PerseguirJogador()
    {
        Vector2 diferenca =
            jogador.position - transform.position;

        // -----------------------------------------------------
        // MOVIMENTO CARDINAL
        // -----------------------------------------------------
        // O Moblin só pode andar para:
        // cima, baixo, esquerda ou direita.
        //
        // A maior diferença determina a direção.
        // -----------------------------------------------------

        if (Mathf.Abs(diferenca.x) > Mathf.Abs(diferenca.y))
        {
            if (diferenca.x > 0)
                movimento = Vector2.right;
            else
                movimento = Vector2.left;
        }
        else
        {
            if (diferenca.y > 0)
                movimento = Vector2.up;
            else
                movimento = Vector2.down;
        }

        ultimaDirecao = movimento;

        AtualizarDirecaoAnimacao(ultimaDirecao);

        anim.SetBool("IsMoving", true);
    }

    // =========================================================
    // PARAR
    // =========================================================

    private void Parar()
    {
        movimento = Vector2.zero;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);
    }

    // =========================================================
    // ATAQUE
    // =========================================================

    private void Atacar()
    {
        atacando = true;

        movimento = Vector2.zero;

        rb.linearVelocity = Vector2.zero;

        proximoAtaque =
            Time.time + tempoEntreAtaques;

        // O Moblin olha para o jogador antes de atacar.
        AtualizarDirecaoParaJogador();

        anim.SetBool("IsMoving", false);

        anim.SetTrigger("Attack");
    }

    // =========================================================
    // DIREÇÃO PARA O JOGADOR
    // =========================================================

    private void AtualizarDirecaoParaJogador()
    {
        Vector2 diferenca =
            jogador.position - transform.position;

        // Movimento cardinal.
        if (Mathf.Abs(diferenca.x) > Mathf.Abs(diferenca.y))
        {
            if (diferenca.x > 0)
                ultimaDirecao = Vector2.right;
            else
                ultimaDirecao = Vector2.left;
        }
        else
        {
            if (diferenca.y > 0)
                ultimaDirecao = Vector2.up;
            else
                ultimaDirecao = Vector2.down;
        }

        AtualizarDirecaoAnimacao(ultimaDirecao);
    }

    // =========================================================
    // ANIMAÇÃO
    // =========================================================

    private void AtualizarDirecaoAnimacao(Vector2 direcao)
    {
        anim.SetFloat("MoveX", direcao.x);
        anim.SetFloat("MoveY", direcao.y);

        anim.SetFloat("LastMoveX", direcao.x);
        anim.SetFloat("LastMoveY", direcao.y);
    }

    // =========================================================
    // HITBOX
    // =========================================================
    // Esses métodos serão chamados pelos Animation Events
    // da animação de ataque.
    // =========================================================

    public void AtivarHitbox()
    {
        DesativarTodasHitboxes();

        if (ultimaDirecao == Vector2.up)
        {
            if (attackUp != null)
                attackUp.SetActive(true);
        }
        else if (ultimaDirecao == Vector2.down)
        {
            if (attackDown != null)
                attackDown.SetActive(true);
        }
        else if (ultimaDirecao == Vector2.left)
        {
            if (attackLeft != null)
                attackLeft.SetActive(true);
        }
        else if (ultimaDirecao == Vector2.right)
        {
            if (attackRight != null)
                attackRight.SetActive(true);
        }
    }

    public void DesativarHitbox()
    {
        DesativarTodasHitboxes();
    }

    private void DesativarTodasHitboxes()
    {
        if (attackUp != null)
            attackUp.SetActive(false);

        if (attackDown != null)
            attackDown.SetActive(false);

        if (attackLeft != null)
            attackLeft.SetActive(false);

        if (attackRight != null)
            attackRight.SetActive(false);
    }

    // =========================================================
    // FINALIZAR ATAQUE
    // =========================================================
    // Animation Event no último frame do ataque.
    // =========================================================

    public void FinalizarAtaque()
    {
        DesativarTodasHitboxes();

        atacando = false;
    }

    // =========================================================
    // KNOCKBACK
    // =========================================================

    public void ReceberKnockback(Vector2 direcao)
    {
        if (sofrendoKnockback)
            return;

        StopCoroutine(nameof(AplicarKnockback));
        StartCoroutine(AplicarKnockback(direcao));
    }

    private IEnumerator AplicarKnockback(Vector2 direcao)
    {
        sofrendoKnockback = true;

        atacando = false;

        DesativarTodasHitboxes();

        movimento = Vector2.zero;

        anim.SetBool("IsMoving", false);

        rb.linearVelocity =
            direcao.normalized * forcaKnockback;

        yield return new WaitForSeconds(duracaoKnockback);

        rb.linearVelocity = Vector2.zero;

        sofrendoKnockback = false;
    }

    // =========================================================
    // SEGURANÇA
    // =========================================================

    private void OnDisable()
    {
        DesativarTodasHitboxes();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}