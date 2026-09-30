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

    [Tooltip("Collider específico da parede que faz a Bruxa ficar vulnerável.")]
    [SerializeField] private Collider2D paredeArena;

    [Header("Ataque")]
    [SerializeField] private int danoAtaque = 1;

    [Header("Animação")]
    [SerializeField] private Animator anim;

    [Header("Feedback de Dano")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;

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

        [Header("Distância")]
        public float distanciaParada = 0.1f;
    }

    [Header("Configuração - Fácil")]
    [SerializeField] private ConfiguracaoFase faseFacil;

    [Header("Configuração - Médio")]
    [SerializeField] private ConfiguracaoFase faseMedia;

    [Header("Configuração - Difícil")]
    [SerializeField] private ConfiguracaoFase faseDificil;

    private EstadoAtual estadoAtual = EstadoAtual.Esperando;

    // Posição capturada no começo da investida.
    private Vector2 ultimoPontoJogador;

    // Direção da investida atual.
    private Vector2 direcaoPerseguicao;

    private float tempoPreparacao;
    private float tempoVulneravel;

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

            case EstadoAtual.Perseguindo:
                AtualizarPerseguicao();
                break;

            case EstadoAtual.Vulneravel:
                AtualizarVulnerabilidade();
                break;
        }
    }

    // =========================================================
    // IDLE
    // =========================================================

    private void AtualizarIdle()
    {
        rb.linearVelocity = Vector2.zero;

        anim.SetFloat("MoveX", 0f);
        anim.SetFloat("MoveY", 0f);
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsDead", false);
    }

    // =========================================================
    // INÍCIO DO BOSS
    // =========================================================

    public void IniciarBoss()
    {
        if (estadoAtual != EstadoAtual.Esperando)
            return;

        if (jogador == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                jogador = playerObject.transform;
        }

        if (jogador == null)
        {
            Debug.LogWarning("BossBruxa: Player não encontrado.");
            return;
        }

        IniciarPreparacao();
    }

    // =========================================================
    // PREPARAÇÃO
    // =========================================================

    private void IniciarPreparacao()
    {
        estadoAtual = EstadoAtual.Preparando;

        tempoPreparacao = ConfiguracaoAtual.tempoPreparacao;

        rb.linearVelocity = Vector2.zero;

        // A direção é definida olhando para o jogador.
        AtualizarDirecaoParaJogador();

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", false);

        // Por enquanto usamos Idle.anim durante a preparação.
        // Uma animação específica de preparação pode ser adicionada depois.
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
    // PERSEGUIÇÃO
    // =========================================================

    private void IniciarPerseguicao()
    {
        if (jogador == null)
            return;

        estadoAtual = EstadoAtual.Perseguindo;

        // Captura a posição UMA ÚNICA VEZ.
        ultimoPontoJogador = jogador.position;

        direcaoPerseguicao =
            (ultimoPontoJogador - (Vector2)transform.position).normalized;

        AtualizarBlendTree(direcaoPerseguicao);

        anim.SetBool("IsMoving", true);
        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsDead", false);
    }

    private void AtualizarPerseguicao()
    {
        Vector2 novaPosicao = Vector2.MoveTowards(
            rb.position,
            ultimoPontoJogador,
            ConfiguracaoAtual.velocidadePerseguicao * Time.deltaTime
        );

        rb.MovePosition(novaPosicao);

        // Mantém a direção da investida.
        AtualizarBlendTree(direcaoPerseguicao);

        if (Vector2.Distance(rb.position, ultimoPontoJogador)
            <= ConfiguracaoAtual.distanciaParada)
        {
            rb.linearVelocity = Vector2.zero;

            // Não inicia outra investida imediatamente.
            // Faz uma nova preparação primeiro.
            IniciarPreparacao();
        }
    }

    // =========================================================
    // DIREÇÃO / BLEND TREE
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

        // -----------------------------------------
        // BATEU NO PLAYER
        // -----------------------------------------

        if (collision.gameObject.CompareTag("Player"))
        {
            CausarDanoNoJogador(collision.gameObject);
            return;
        }

        // -----------------------------------------
        // BATEU NA PAREDE DA ARENA
        // -----------------------------------------

        if (paredeArena == null)
            return;

        foreach (ContactPoint2D contato in collision.contacts)
        {
            if (contato.collider == paredeArena)
            {
                rb.linearVelocity = Vector2.zero;

                ComecarVulnerabilidade();

                return;
            }
        }
    }

    // =========================================================
    // DANO AO PLAYER
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

    private void ComecarVulnerabilidade()
    {
        if (estadoAtual != EstadoAtual.Perseguindo)
            return;

        estadoAtual = EstadoAtual.Vulneravel;

        tempoVulneravel = ConfiguracaoAtual.tempoVulneravel;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", true);
        anim.SetBool("IsDead", false);
    }

    private void AtualizarVulnerabilidade()
    {
        rb.linearVelocity = Vector2.zero;

        tempoVulneravel -= Time.deltaTime;

        if (tempoVulneravel <= 0f)
        {
            VoltarAoCentro();
        }
    }

    // =========================================================
    // TELEPORTE
    // =========================================================

    private void VoltarAoCentro()
    {
        if (centroArena != null)
        {
            rb.position = centroArena.position;
        }

        anim.SetBool("IsVulnerable", false);

        // Depois do teleporte, começa novamente
        // com o pequeno período de preparação.
        IniciarPreparacao();
    }

    // =========================================================
    // DANO DO PLAYER
    // =========================================================

    public void ReceberDano(int dano)
    {
        // Só pode receber dano quando vulnerável.
        if (estadoAtual != EstadoAtual.Vulneravel)
            return;

        vidaAtual -= dano;

        // Feedback visual/partículas.
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
        float porcentagemVida = (float)vidaAtual / vidaMaxima;

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

        // A arena só abre quando o boss realmente morreu.
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
