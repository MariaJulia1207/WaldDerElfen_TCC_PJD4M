using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.Rendering;
using static UnityEngine.InputSystem.HID.HID;

public class BossBruxa : MonoBehaviour, IBoss
{
    public enum BossState
    {
        Facil,
        Medio,
        Dificil
    }

    private enum EstadoAtual 
    { Esperando, Preparando, Perseguindo, Knockback, Vulneravel, Invocando, Derrotado }

    // =========================================================
    // VIDA
    // =========================================================

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 60;

    private int vidaAtual;


    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("Referências")]
    [SerializeField] private Transform jogador;
    [SerializeField] private Transform centroArena;
    [SerializeField] private BossArena bossArena;

    [Header("Diálogo Inicial")]
    [SerializeField] private DialogueData dialogoInicial;

    private bool dialogoInicialConcluido;
    private bool aguardandoDialogoInicial;


    // =========================================================
    // FÍSICA
    // =========================================================

    [Header("Física")]
    [SerializeField] private Rigidbody2D rb;

    [Tooltip("Collider específico da parede que deixa a Bruxa vulnerável.")]
    [SerializeField] private Collider2D paredeArena;

    [Tooltip("Força do recuo ao bater na parede.")]
    [SerializeField] private float forcaKnockbackParede = 2f;

    [Tooltip("Duração do recuo ao bater na parede.")]
    [SerializeField] private float duracaoKnockbackParede = 0.12f;


    // =========================================================
    // ATAQUE
    // =========================================================

    [Header("Ataque")]
    [SerializeField] private int danoAtaque = 1;

    [Header("Ataque de Projéteis")]
    [SerializeField] private ProjetilPool projetilPool;
    [SerializeField] private float raioOrbita = 1.2f;
    [SerializeField] private float velocidadeOrbita = 180f;
    [SerializeField] private float tempoOrbita = 1.5f;
    [SerializeField] private float tempoEntreProjeteis = 0.05f;

    private readonly List<Projetil> projetisAtivos = new List<Projetil>(); 
    private float anguloInicial;


    // =========================================================
    // ANIMAÇÃO
    // =========================================================

    [Header("Animação")]
    [SerializeField] private Animator anim;


    // =========================================================
    // FEEDBACK DE DANO
    // =========================================================

    [Header("Feedback de Dano")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;


    // =========================================================
    // MORTE
    // =========================================================

    [Header("Morte")]
    [SerializeField] private ParticleSystem particulasMorte;
    [SerializeField] private float tempoAteParticulas = 1.5f;
    [SerializeField] private float tempoDoEfeitoMorte = 2f;


    // =========================================================
    // FASE
    // =========================================================

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

        [Header("Projéteis")]
        public int quantidadeProjetis = 6;
        public float velocidadeProjetil = 5f;
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

    private Vector2 ultimoPontoJogador;
    private Vector2 direcaoPerseguicao;

    private Vector2 direcaoKnockback;

    private float tempoPreparacao;
    private float tempoVulneravel;
    private float tempoKnockback;


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
            Debug.LogWarning(
                "BossBruxa: Player não encontrado.");

            return;
        }

        Debug.Log("BossBruxa: combate iniciado!");

        // ---------------------------------------------------------
        // DIÁLOGO INICIAL
        // ---------------------------------------------------------

        if (!dialogoInicialConcluido &&
            dialogoInicial != null)
        {
            aguardandoDialogoInicial = true;

            BossDialogueManager.Instance.StartBossDialogue(
                dialogoInicial,
                this);

            return;
        }

        // ---------------------------------------------------------
        // SEM DIÁLOGO / DIÁLOGO JÁ CONCLUÍDO
        // ---------------------------------------------------------

        IniciarPreparacao();
    }

    public void ConcluirDialogoInicial()
    {
        if (!aguardandoDialogoInicial)
            return;

        aguardandoDialogoInicial = false;
        dialogoInicialConcluido = true;

        Debug.Log(
            "BossBruxa: diálogo inicial concluído.");

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

        AtualizarDirecaoParaJogador();
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

        // Captura a posição UMA ÚNICA VEZ.
        ultimoPontoJogador = jogador.position;

        direcaoPerseguicao =
            (ultimoPontoJogador - rb.position).normalized;

        if (direcaoPerseguicao.sqrMagnitude < 0.001f)
        {
            direcaoPerseguicao = Vector2.down;
        }

        AtualizarBlendTree(direcaoPerseguicao);

        anim.SetBool("IsMoving", true);
        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsDead", false);
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
            ConfiguracaoAtual.velocidadePerseguicao *
            Time.fixedDeltaTime
        );

        rb.MovePosition(novaPosicao);

        AtualizarBlendTree(direcaoPerseguicao);

        if (Vector2.Distance(
                novaPosicao,
                ultimoPontoJogador)
            <= ConfiguracaoAtual.distanciaParada)
        {
            rb.MovePosition(ultimoPontoJogador);

            rb.linearVelocity = Vector2.zero;

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
    // KNOCKBACK
    // =========================================================

    private void IniciarKnockbackParede()
    {
        if (estadoAtual != EstadoAtual.Perseguindo)
            return;

        estadoAtual = EstadoAtual.Knockback;

        direcaoKnockback =
            -direcaoPerseguicao.normalized;

        tempoKnockback =
            duracaoKnockbackParede;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);
    }


    private void AtualizarKnockback()
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 novaPosicao =
            rb.position +
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
        if (estadoAtual != EstadoAtual.Knockback)
            return;

        estadoAtual = EstadoAtual.Vulneravel;

        tempoVulneravel =
            ConfiguracaoAtual.tempoVulneravel;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", true);
        anim.SetBool("IsDead", false);

        // Força a entrada na animação.
        anim.Play("Vulnerable", 0, 0f);
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
        StartCoroutine(AtaqueProjetis()); 
    }


    // =========================================================
    // RECEBER DANO
    // =========================================================

    public void ReceberDano(int dano)
    {
        if (estadoAtual != EstadoAtual.Vulneravel)
            return;

        vidaAtual -= dano;

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
        if (estadoAtual == EstadoAtual.Derrotado)
            return;

        estadoAtual = EstadoAtual.Derrotado;

        rb.linearVelocity = Vector2.zero;

        // Impede novas interações físicas.
        rb.simulated = false;

        // Desliga parâmetros de combate.
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsDead", true);

        // Força diretamente a animação de morte.
        anim.Play("Death", 0, 0f);

        Debug.Log("BossBruxa: derrotada!");

        StartCoroutine(SequenciaMorte());
    }


    // =========================================================
    // SEQUÊNCIA DE MORTE
    // =========================================================

    private IEnumerator SequenciaMorte()
    {
        // Dá tempo para a animação de morte começar.
        yield return new WaitForSeconds(tempoAteParticulas);


        // -----------------------------------------------------
        // PARTÍCULAS
        // -----------------------------------------------------

        if (particulasMorte != null)
        {
            ParticleSystem efeito =
                Instantiate(
                    particulasMorte,
                    transform.position,
                    Quaternion.identity
                );

            efeito.Play();

            Debug.Log("BossBruxa: partículas de morte ativadas.");
        }


        // -----------------------------------------------------
        // TEMPO DO EFEITO
        // -----------------------------------------------------

        yield return new WaitForSeconds(tempoDoEfeitoMorte);


        // -----------------------------------------------------
        // ARENA
        // -----------------------------------------------------

        if (bossArena != null)
        {
            bossArena.BossDerrotado();
        }


        // -----------------------------------------------------
        // DESTRUIÇÃO
        // -----------------------------------------------------

        Destroy(gameObject);
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

    private IEnumerator AtaqueProjetis()
    {
        estadoAtual = EstadoAtual.Invocando;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsMoving", false);
        anim.SetBool("IsVulnerable", false);
        anim.SetBool("IsDead", false);
        anim.SetBool("IsInvocando", true);

        projetisAtivos.Clear();
        anguloInicial = 0f;

        int quantidade = ConfiguracaoAtual.quantidadeProjetis;
        float velocidade = ConfiguracaoAtual.velocidadeProjetil;

        // =========================================================
        // CRIA OS PROJÉTEIS
        // =========================================================

        for (int i = 0; i < quantidade; i++)
        {
            Projetil projetil = projetilPool.Pegar();

            float angulo =
                anguloInicial +
                (360f / quantidade) * i;

            float radianos =
                angulo * Mathf.Deg2Rad;

            Vector2 offset =
                new Vector2(
                    Mathf.Cos(radianos),
                    Mathf.Sin(radianos)
                ) * raioOrbita;

            projetil.transform.position =
                (Vector2)transform.position + offset;

            projetisAtivos.Add(projetil);

            yield return new WaitForSeconds(tempoEntreProjeteis);
        }

        // =========================================================
        // ÓRBITA
        // =========================================================

        float tempo = 0f;

        while (tempo < tempoOrbita)
        {
            tempo += Time.deltaTime;

            anguloInicial += velocidadeOrbita * Time.deltaTime;

            for (int i = 0; i < projetisAtivos.Count; i++)
            {
                Projetil projetil = projetisAtivos[i];

                if (projetil == null || !projetil.gameObject.activeSelf)
                    continue;

                float angulo =
                    anguloInicial +
                    (360f / quantidade) * i;

                float radianos =
                    angulo * Mathf.Deg2Rad;

                Vector2 offset =
                    new Vector2(
                        Mathf.Cos(radianos),
                        Mathf.Sin(radianos)
                    ) * raioOrbita;

                projetil.transform.position =
                    (Vector2)transform.position + offset;
            }

            yield return null;
        }

        // =========================================================
        // DISPARO
        // =========================================================

        for (int i = 0; i < projetisAtivos.Count; i++)
        {
            Projetil projetil = projetisAtivos[i];

            if (projetil == null || !projetil.gameObject.activeSelf)
                continue;

            float angulo =
                anguloInicial +
                (360f / quantidade) * i;

            float radianos =
                angulo * Mathf.Deg2Rad;

            Vector2 direcao =
                new Vector2(
                    Mathf.Cos(radianos),
                    Mathf.Sin(radianos)
                );

            projetil.Disparar(
                direcao,
                velocidade
            );
        }

        projetisAtivos.Clear();

        anim.SetBool("IsInvocando", false);

        yield return new WaitForSeconds(0.2f);

        IniciarPreparacao();
    }
}