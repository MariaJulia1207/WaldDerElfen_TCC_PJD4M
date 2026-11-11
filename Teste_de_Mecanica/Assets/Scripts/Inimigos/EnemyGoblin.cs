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

        float distancia = Vector2.Distance(transform.position, jogador.position);

        // 1. Prioridade: Ataque
        if (distancia <= distanciaAtaque)
        {
            if (perseguindoJogador)
            {
                perseguindoJogador = false;
            }

            if (waypointMover != null)
            {
                waypointMover.PausePatrol();
            }

            Parar();

            if (Time.time >= proximoAtaque)
            {
                Atacar();
            }
            return;
        }

        // 2. Perseguição ao Jogador
        if (distancia <= distanciaDeteccao)
        {
            if (waypointMover != null && !perseguindoJogador)
            {
                waypointMover.PausePatrol();
            }

            perseguindoJogador = true;
            PerseguirJogador();
            return;
        }

        // 3. Se perdeu o jogador de vista, retoma a patrulha
        if (perseguindoJogador)
        {
            perseguindoJogador = false;
            if (waypointMover != null)
            {
                waypointMover.ResumePatrol();
            }
        }

        // 4. Patrulha via Waypoint ou Parado
        if (waypointMover != null && waypointMover.HasWaypoints)
        {
            AtualizarAnimacaoPatrulha();
        }
        else
        {
            Parar();
        }
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

        if (waypointMover != null && waypointMover.HasWaypoints)
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
        if (waypointMover == null || !waypointMover.HasWaypoints)
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
            movimento = diferenca.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            movimento = diferenca.y > 0 ? Vector2.up : Vector2.down;
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

        SoundEffectManager.Play("EnemySword");
    }

    private void AtualizarDirecaoParaJogador()
    {
        Vector2 diferenca = jogador.position - transform.position;

        if (Mathf.Abs(diferenca.x) > Mathf.Abs(diferenca.y))
        {
            ultimaDirecao = diferenca.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            ultimaDirecao = diferenca.y > 0 ? Vector2.up : Vector2.down;
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

        if (ultimaDirecao == Vector2.up && attackUp != null) attackUp.SetActive(true);
        else if (ultimaDirecao == Vector2.down && attackDown != null) attackDown.SetActive(true);
        else if (ultimaDirecao == Vector2.left && attackLeft != null) attackLeft.SetActive(true);
        else if (ultimaDirecao == Vector2.right && attackRight != null) attackRight.SetActive(true);
    }

    public void DesativarHitbox()
    {
        DesativarTodasHitboxes();
    }

    private void DesativarTodasHitboxes()
    {
        if (attackUp != null) attackUp.SetActive(false);
        if (attackDown != null) attackDown.SetActive(false);
        if (attackLeft != null) attackLeft.SetActive(false);
        if (attackRight != null) attackRight.SetActive(false);
    }

    public void FinalizarAtaque()
    {
        DesativarTodasHitboxes();
        atacando = false;
    }

    public void ReceberKnockback(Vector2 direcao)
    {
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