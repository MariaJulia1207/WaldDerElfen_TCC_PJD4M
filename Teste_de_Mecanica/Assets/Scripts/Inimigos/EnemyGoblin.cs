using System.Collections;
using UnityEngine;

public class EnemyGoblin : MonoBehaviour
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
    [SerializeField] private float forcaKnockback = 3f;
    [SerializeField] private float duracaoKnockback = 0.15f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform jogador;
    private WaypointMover waypointMover;

    private Vector2 movimento;
    private Vector2 ultimaDirecao = Vector2.down;

    private bool atacando;
    private bool sofrendoKnockback;
    private bool perseguindoJogador;

    private float proximoAtaque;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        waypointMover = GetComponent<WaypointMover>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            jogador = player.transform;
        }

        DesativarTodasHitboxes();
        AtualizarDirecaoAnimacao(ultimaDirecao);
    }

    private void Update()
    {
        if (jogador == null)
            return;

        if (atacando || sofrendoKnockback)
        {
            movimento = Vector2.zero;
            anim.SetBool("IsMoving", false);
            return;
        }

        if (waypointMover != null)
        {
            AtualizarAnimacaoPatrulha();

            float distancia = Vector2.Distance(transform.position, jogador.position);

            if (distancia <= distanciaDeteccao)
            {
                if (!perseguindoJogador)
                {
                    waypointMover.PausePatrol();
                }

                perseguindoJogador = true;
                PerseguirJogador();
                return;
            }

            if (perseguindoJogador)
            {
                perseguindoJogador = false;
                waypointMover.ResumePatrol();
            }

            return;
        }

        float distanciaAtual = Vector2.Distance(transform.position, jogador.position);

        if (distanciaAtual > distanciaDeteccao)
        {
            Parar();
            return;
        }

        if (distanciaAtual <= distanciaAtaque)
        {
            Parar();

            if (Time.time >= proximoAtaque)
            {
                Atacar();
            }

            return;
        }

        PerseguirJogador();
    }

    private void FixedUpdate()
    {
        if (sofrendoKnockback)
            return;

        if (atacando)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (waypointMover != null)
        {
            if (perseguindoJogador)
            {
                rb.linearVelocity = movimento * velocidade;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }

            return;
        }

        rb.linearVelocity = movimento * velocidade;
    }

    private void AtualizarAnimacaoPatrulha()
    {
        if (waypointMover == null)
            return;

        if (waypointMover.IsWaiting)
        {
            anim.SetBool("IsMoving", false);
            anim.SetFloat("MoveX", ultimaDirecao.x);
            anim.SetFloat("MoveY", ultimaDirecao.y);
            anim.SetFloat("LastMoveX", ultimaDirecao.x);
            anim.SetFloat("LastMoveY", ultimaDirecao.y);
            return;
        }

        Vector2 direcao = waypointMover.CurrentMoveDirection;
        ultimaDirecao = direcao;
        AtualizarDirecaoAnimacao(direcao);
        anim.SetBool("IsMoving", true);
    }

    private void PerseguirJogador()
    {
        Vector2 diferenca = jogador.position - transform.position;

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

    private void Parar()
    {
        movimento = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("IsMoving", false);
    }

    private void Atacar()
    {
        atacando = true;

        movimento = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        proximoAtaque = Time.time + tempoEntreAtaques;
        AtualizarDirecaoParaJogador();
        anim.SetBool("IsMoving", false);
        anim.SetTrigger("Attack");
    }

    private void AtualizarDirecaoParaJogador()
    {
        Vector2 diferenca = jogador.position - transform.position;

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

    private void AtualizarDirecaoAnimacao(Vector2 direcao)
    {
        anim.SetFloat("MoveX", direcao.x);
        anim.SetFloat("MoveY", direcao.y);
        anim.SetFloat("LastMoveX", direcao.x);
        anim.SetFloat("LastMoveY", direcao.y);
    }

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

    public void FinalizarAtaque()
    {
        DesativarTodasHitboxes();
        atacando = false;
    }

    public void ReceberKnockback(Vector2 direcao)
    {
        Debug.Log("KNOCKBACK RECEBIDO PELO GOBLIN");

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
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = direcao.normalized * forcaKnockback;

        yield return new WaitForSeconds(duracaoKnockback);

        rb.linearVelocity = Vector2.zero;

        sofrendoKnockback = false;
    }

    private void OnDisable()
    {
        DesativarTodasHitboxes();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
