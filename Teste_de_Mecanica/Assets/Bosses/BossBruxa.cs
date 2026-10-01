using UnityEngine;

public class BossBruxa : MonoBehaviour, IBoss
{
    public enum BossState
    {
        Facil,
        Medio,
        Dificil
    }

    private enum EstadoAtual
    {
        Esperando,
        Preparando,
        Perseguindo,
        Knockback,
        Vulneravel,
        Derrotado
    }

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 60;

    private int vidaAtual;

    [Header("Referências")]
    [SerializeField] private Transform jogador;
    [SerializeField] private Transform centroArena;
    [SerializeField] private BossArena bossArena;

    [Header("Física")]
    [SerializeField] private Rigidbody2D rb;
    [Tooltip("Collider específico da parede que deixa a Bruxa vulnerável.")]
    [SerializeField] private Collider2D paredeArena;
    [Tooltip("Força do recuo ao bater na parede.")]
    [SerializeField] private float forcaKnockbackParede = 2f;
    [Tooltip("Duração do recuo ao bater na parede.")]
    [SerializeField] private float duracaoKnockbackParede = 0.12f;


    [Header("Ataque")]
    [SerializeField] private int danoAtaque = 1;


    [Header("Animação")]
    [SerializeField] private Animator anim;


    [Header("Feedback de Dano")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;
    private float tempoKnockback;
    private Vector2 direcaoKnockback;


    [Header("Fase")]
    [SerializeField] private BossState faseAtual = BossState.Facil;


    [System.Serializable]
    public class ConfiguracaoFase
    {
        [Header("Movimento")]
        public float velocidadePerseguicao = 5f;

        [Header("Vulnerabilidade")]
        public float tempoVulneravel = 3f;

        [Header("Preparação")]
        public float tempoPreparacao = 0.6f;

        [Header("Distância para considerar que chegou")]
        public float distanciaParada = 0.1f;
    }


    [Header("Configuração - Fácil")]
    [SerializeField] private ConfiguracaoFase faseFacil;

    [Header("Configuração - Médio")]
    [SerializeField] private ConfiguracaoFase faseMedia;

    [Header("Configuração - Difícil")]
    [SerializeField] private ConfiguracaoFase faseDificil;


    // =========================================================
    // ESTADO
    // =========================================================

    private EstadoAtual estadoAtual = EstadoAtual.Esperando;


    // Ponto onde o jogador estava quando a investida começou.
    private Vector2 ultimoPontoJogador;

    // Direção da investida.
    private Vector2 direcaoPerseguicao;

    private float tempoPreparacao;
    private float tempoVulneravel;


    // =========================================================
    // CONFIGURAÇÃO DA FASE ATUAL
    // =========================================================

    private ConfiguracaoFase ConfiguracaoAtual
    {
        get
        {
            switch (faseAtual)
            {
                case BossState.Medio:
                    return faseMedia;

                case BossState.Dificil:
                    return faseDificil;

                default:
                    return faseFacil;
            }
        }
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        vidaAtual = vidaMaxima;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponent<Animator>();

        if (feedbackDano == null)
            feedbackDano = GetComponent<ControladorFeedBackDano>();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (estadoAtual == EstadoAtual.Derrotado)
            return;

        switch (estadoAtual)
        {
            case EstadoAtual.Esperando:
                AtualizarIdle();
                break;

            case EstadoAtual.Preparando:
                AtualizarPreparacao();
                break;

            case EstadoAtual.Vulneravel:
                AtualizarVulnerabilidade();
                break;
        }
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        if (estadoAtual == EstadoAtual.Perseguindo)
        {
            AtualizarPerseguicao();
        }
        else if (estadoAtual == EstadoAtual.Knockback)
        {
            AtualizarKnockback();
        }
    }


    // =========================================================
    // IDLE
    // =========================================================

    private void AtualizarIdle()
    {
        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsDead", false);
    }


    // =========================================================
    // INICIAR BOSS
    // =========================================================

    public void IniciarBoss()
    {
        if (estadoAtual != EstadoAtual.Esperando)
            return;

        EncontrarJogador();

        if (jogador == null)
        {
            Debug.LogWarning("BossBruxa: Player não encontrado.");
            return;
        }

        Debug.Log("BossBruxa: combate iniciado!");

        IniciarPreparacao();
    }


    // =========================================================
    // ENCONTRAR PLAYER
    // =========================================================

    private void EncontrarJogador()
    {
        if (jogador != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            jogador = playerObject.transform;
    }


    // =========================================================
    // PREPARAÇÃO
    // =========================================================

    private void IniciarPreparacao()
    {
        estadoAtual = EstadoAtual.Preparando;

        tempoPreparacao = ConfiguracaoAtual.tempoPreparacao;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsDead", false);

        // Durante a preparação ela apenas olha para o jogador.
        AtualizarDirecaoParaJogador();

        Debug.Log("BossBruxa: preparando investida.");
    }


    private void AtualizarPreparacao()
    {
        rb.linearVelocity = Vector2.zero;

        AtualizarDirecaoParaJogador();

        tempoPreparacao -= Time.deltaTime;

        if (tempoPreparacao <= 0f)
        {
            IniciarPerseguicao();
        }
    }


    // =========================================================
    // INICIAR PERSEGUIÇÃO
    // =========================================================

    private void IniciarPerseguicao()
    {
        EncontrarJogador();

        if (jogador == null)
            return;

        estadoAtual = EstadoAtual.Perseguindo;

        // =====================================================
        // IMPORTANTE:
        // captura a posição SOMENTE UMA VEZ.
        // =====================================================

        ultimoPontoJogador = jogador.position;

        direcaoPerseguicao =
            (ultimoPontoJogador - rb.position).normalized;

        // Se o jogador estiver praticamente sobre a Bruxa.
        if (direcaoPerseguicao.sqrMagnitude < 0.001f)
        {
            direcaoPerseguicao = Vector2.down;
        }

        AtualizarBlendTree(direcaoPerseguicao);

        anim.SetBool("IsMoving", true);
        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsDead", false);

        Debug.Log(
            "BossBruxa: investida iniciada. " +
            "Alvo capturado: " + ultimoPontoJogador
        );
    }


    // =========================================================
    // PERSEGUIÇÃO
    // =========================================================

    private void AtualizarPerseguicao()
    {
        Vector2 posicaoAtual = rb.position;

        Vector2 novaPosicao = Vector2.MoveTowards(
            posicaoAtual,
            ultimoPontoJogador,
            ConfiguracaoAtual.velocidadePerseguicao * Time.fixedDeltaTime
        );

        rb.MovePosition(novaPosicao);

        // Mantém a animação apontada para a direção da investida.
        AtualizarBlendTree(direcaoPerseguicao);

        // Chegou ao ponto onde o jogador estava.
        if (Vector2.Distance(novaPosicao, ultimoPontoJogador)
            <= ConfiguracaoAtual.distanciaParada)
        {
            rb.MovePosition(ultimoPontoJogador);

            rb.linearVelocity = Vector2.zero;

            Debug.Log("BossBruxa: chegou ao ponto do jogador.");

            IniciarPreparacao();
        }
    }


    // =========================================================
    // BLEND TREE
    // =========================================================

    private void AtualizarDirecaoParaJogador()
    {
        if (jogador == null)
            return;

        Vector2 direcao =
            (jogador.position - transform.position).normalized;

        AtualizarBlendTree(direcao);
    }


    private void AtualizarBlendTree(Vector2 direcao)
    {
        if (direcao.sqrMagnitude <= 0.001f)
            return;

        anim.SetFloat("MoveX", direcao.x);
        anim.SetFloat("MoveY", direcao.y);
    }


    // =========================================================
    // COLISÃO
    // =========================================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (estadoAtual != EstadoAtual.Perseguindo)
            return;


        // -----------------------------------------------------
        // PLAYER
        // -----------------------------------------------------

        if (collision.gameObject.CompareTag("Player"))
        {
            CausarDanoNoJogador(collision.gameObject);

            Debug.Log("BossBruxa: atingiu o jogador.");

            return;
        }


        // -----------------------------------------------------
        // PAREDE DA ARENA
        // -----------------------------------------------------

        if (paredeArena == null)
            return;

        foreach (ContactPoint2D contato in collision.contacts)
        {
            if (contato.collider == paredeArena)
            {
                IniciarKnockbackParede();
                return;
            }
        }
    }


    // =========================================================
    // DANO NO PLAYER
    // =========================================================

    private void CausarDanoNoJogador(GameObject player)
    {
        player.SendMessage(
            "ReceberDano",
            danoAtaque,
            SendMessageOptions.DontRequireReceiver
        );
    }


    // =========================================================
    // VULNERABILIDADE
    // =========================================================

    private void IniciarKnockbackParede()
    {
        if (estadoAtual != EstadoAtual.Perseguindo)
            return;

        estadoAtual = EstadoAtual.Knockback;

        // Recuar na direção oposta à investida.
        direcaoKnockback = -direcaoPerseguicao.normalized;

        tempoKnockback = duracaoKnockbackParede;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);

        Debug.Log("BossBruxa: impacto com a parede! Knockback.");
    }

    private void AtualizarKnockback()
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 novaPosicao = rb.position +
            direcaoKnockback *
            forcaKnockbackParede *
            Time.fixedDeltaTime;

        rb.MovePosition(novaPosicao);

        tempoKnockback -= Time.fixedDeltaTime;

        if (tempoKnockback <= 0f)
        {
            rb.linearVelocity = Vector2.zero;

            ComecarVulnerabilidade();
        }
    }

    private void ComecarVulnerabilidade()
    {
        if (estadoAtual != EstadoAtual.Knockback)
            return;

        estadoAtual = EstadoAtual.Vulneravel;

        tempoVulneravel = ConfiguracaoAtual.tempoVulneravel;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", true);
        anim.SetBool("IsDead", false);

        // Força imediatamente a animação de vulnerabilidade.
        anim.Play("Vulnerable", 0, 0f);

        Debug.Log("BossBruxa: ficou vulnerável.");
    }
    private void AtualizarVulnerabilidade()
    {
        rb.linearVelocity = Vector2.zero;

        tempoVulneravel -= Time.deltaTime;

        if (tempoVulneravel <= 0f)
        {
            anim.SetBool("IsVulnerable", false);

            VoltarAoCentro();
        }
    }

    // =========================================================
    // VOLTAR AO CENTRO
    // =========================================================

    private void VoltarAoCentro()
    {
        if (centroArena != null)
        {
            rb.position = centroArena.position;
        }

        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsMoving", false);

        IniciarPreparacao();
    }

    // =========================================================
    // RECEBER DANO
    // =========================================================

    public void ReceberDano(int dano)
    {
        // Só pode receber dano quando vulnerável.
        if (estadoAtual != EstadoAtual.Vulneravel)
            return;

        vidaAtual -= dano;

        Debug.Log(
            "BossBruxa recebeu dano. Vida: " +
            vidaAtual + "/" + vidaMaxima
        );

        if (feedbackDano != null)
        {
            feedbackDano.ExecutarFeedback();
        }

        if (vidaAtual <= 0)
        {
            vidaAtual = 0;

            Morrer();

            return;
        }

        AtualizarFase();
    }


    // =========================================================
    // FASES
    // =========================================================

    private void AtualizarFase()
    {
        float porcentagemVida =
            (float)vidaAtual / vidaMaxima;

        if (porcentagemVida <= 0.33f)
        {
            faseAtual = BossState.Dificil;
        }
        else if (porcentagemVida <= 0.66f)
        {
            faseAtual = BossState.Medio;
        }
        else
        {
            faseAtual = BossState.Facil;
        }
    }


    // =========================================================
    // MORTE
    // =========================================================

    private void Morrer()
    {
        estadoAtual = EstadoAtual.Derrotado;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsDead", true);

        if (bossArena != null)
        {
            bossArena.BossDerrotado();
        }

        Debug.Log("BossBruxa: derrotada!");
    }


    // =========================================================
    // TESTES
    // =========================================================

    public bool EstaVulneravel()
    {
        return estadoAtual == EstadoAtual.Vulneravel;
    }

    public int GetVidaAtual()
    {
        return vidaAtual;
    }

    public BossState GetFaseAtual()
    {
        return faseAtual;
    }
}