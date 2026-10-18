using System.Collections;
using UnityEngine;

public class EnemyGriffin : MonoBehaviour
{
    [Header("Movimento e Detecção")]
    [SerializeField] private float distanciaAtaque = 6f;

    [Header("Ataque e Projétil")]
    [SerializeField] private ProjetilPool poolProjetil;
    [SerializeField] private Transform pontoDisparo;
    [SerializeField] private float velocidadeProjetil = 5f;
    [SerializeField] private float tempoEntreAtaques = 2f;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 3f;
    [SerializeField] private float duracaoKnockback = 0.15f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform jogador;
    private WaypointMover waypointMover;

    private Vector2 ultimaDirecao = Vector2.right;
    private bool atacando;
    private bool sofrendoKnockback;
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

        if (poolProjetil == null)
        {
            poolProjetil = FindFirstObjectByType<ProjetilPool>();
        }

        AtualizarDirecaoAnimacao(ultimaDirecao);
    }

    private void Update()
    {
        if (jogador == null)
            return;

        if (atacando || sofrendoKnockback)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("IsMoving", false);
            return;
        }

        // Movimento por Waypoints
        AtualizarAnimacaoPatrulha();

        // Checar Ataque à distância
        float distancia = Vector2.Distance(transform.position, jogador.position);
        if (distancia <= distanciaAtaque && Time.time >= proximoAtaque)
        {
            Atacar();
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

        rb.linearVelocity = Vector2.zero;
    }

    private void AtualizarAnimacaoPatrulha()
    {
        if (waypointMover == null)
            return;

        if (waypointMover.IsWaiting)
        {
            anim.SetBool("IsMoving", false);
            return;
        }

        Vector2 direcao = waypointMover.CurrentMoveDirection;

        // Atualiza a direção com base apenas na movimentação horizontal (Esquerda / Direita)
        if (Mathf.Abs(direcao.x) > 0.01f)
        {
            ultimaDirecao = direcao.x > 0 ? Vector2.right : Vector2.left;
        }

        AtualizarDirecaoAnimacao(ultimaDirecao);
        anim.SetBool("IsMoving", true);
    }

    private void Atacar()
    {
        atacando = true;
        proximoAtaque = Time.time + tempoEntreAtaques;

        if (waypointMover != null)
        {
            waypointMover.PausePatrol();
        }

        rb.linearVelocity = Vector2.zero;
        anim.SetBool("IsMoving", false);

        // Virar para a direção do jogador no eixo horizontal
        Vector2 diferenca = jogador.position - transform.position;
        if (diferenca.x != 0)
        {
            ultimaDirecao = diferenca.x > 0 ? Vector2.right : Vector2.left;
            AtualizarDirecaoAnimacao(ultimaDirecao);
        }

        anim.SetTrigger("Attack");
    }

    private void AtualizarDirecaoAnimacao(Vector2 direcao)
    {
        anim.SetFloat("MoveX", direcao.x);
        anim.SetFloat("LastMoveX", direcao.x);
    }

    // =========================================================
    // EVENTOS DE ANIMAÇÃO (Animation Events)
    // =========================================================

    /// <summary>
    /// Adicione este evento no frame da animação de ataque em que o projétil deve ser disparado.
    /// </summary>
    public void DispararProjetil()
    {
        if (jogador == null)
            return;

        if (poolProjetil == null)
        {
            poolProjetil = FindFirstObjectByType<ProjetilPool>();
            if (poolProjetil == null)
            {
                Debug.LogWarning("EnemyGriffin: ProjetilPool não encontrado na cena!");
                return;
            }
        }

        Projetil projetil = poolProjetil.Pegar();
        if (projetil != null)
        {
            Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
            projetil.transform.position = origem;

            Vector2 direcaoAtaque = (jogador.position - origem).normalized;
            projetil.Disparar(direcaoAtaque, velocidadeProjetil);
        }
    }

    /// <summary>
    /// Adicione este evento no último frame da animação de ataque para liberar o movimento.
    /// </summary>
    public void FinalizarAtaque()
    {
        atacando = false;

        if (waypointMover != null)
        {
            waypointMover.ResumePatrol();
        }
    }

    // =========================================================
    // KNOCKBACK
    // =========================================================

    public void ReceberKnockback(Vector2 direcao)
    {
        StopCoroutine(nameof(AplicarKnockback));
        StartCoroutine(AplicarKnockback(direcao));
    }

    private IEnumerator AplicarKnockback(Vector2 direcao)
    {
        sofrendoKnockback = true;
        atacando = false;

        anim.SetBool("IsMoving", false);
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = direcao.normalized * forcaKnockback;

        yield return new WaitForSeconds(duracaoKnockback);

        rb.linearVelocity = Vector2.zero;
        sofrendoKnockback = false;

        if (waypointMover != null)
        {
            waypointMover.ResumePatrol();
        }
    }

    private void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}