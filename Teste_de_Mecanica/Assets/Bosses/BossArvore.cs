using System.Collections;
using UnityEngine;

public class BossArvore : MonoBehaviour, IBoss
{
    // =========================================================
    // ESTADO DO BOSS
    // =========================================================

    public enum EstadoBoss
    {
        Parado,
        Facil,
        Medio,
        Dificil,
        Morrendo
    }

    [Header("Estado")]
    [SerializeField] private EstadoBoss estadoAtual = EstadoBoss.Parado;

    // =========================================================
    // VIDA
    // =========================================================

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 35;

    private int vida;

    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("Componentes")]
    [SerializeField] private Animator animator;
    [SerializeField] private BossPapoula papoula;
    [SerializeField] private BossRaizPool raizPool;

    [Header("Feedback de Dano")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;

    [Header("Diálogo Inicial")]
    // Se houver um diálogo inicial, ele será exibido antes do combate começar.
    [SerializeField] private DialogueData dialogoInicial;

    private bool aguardandoDialogoInicial;
    private bool dialogoInicialConcluido;

    // =========================================================
    // MORTE
    // =========================================================

    [Header("Morte")]
    [SerializeField] private ParticleSystem particulasMorte;
    [SerializeField] private float tempoAteParticulas = 1.5f;
    [SerializeField] private float tempoDoEfeitoMorte = 2f;

    [Header("NPC após a derrota")]
    [SerializeField] private GameObject arvoreCuradaNPC;

    // =========================================================
    // ATAQUES
    // =========================================================

    [Header("Intervalo entre ataques")]
    [SerializeField] private float intervaloFacil = 5f;
    [SerializeField] private float intervaloMedio = 3.5f;
    [SerializeField] private float intervaloDificil = 2.5f;

    [Header("Duração da invocação")]
    [SerializeField] private float duracaoInvocacaoFacil = 3f;
    [SerializeField] private float duracaoInvocacaoMedio = 4f;
    [SerializeField] private float duracaoInvocacaoDificil = 5f;

    // =========================================================
    // RAÍZES
    // =========================================================

    [Header("Quantidade de raízes por invocação")]
    [SerializeField] private int raizesFacil = 2;
    [SerializeField] private int raizesMedio = 4;
    [SerializeField] private int raizesDificil = 6;

    [Header("Momento da invocação")]
    [SerializeField] private float tempoAteInvocarRaizes = 1.5f;

    // =========================================================
    // CONTROLE
    // =========================================================

    private bool combateIniciado;
    private bool atacando;
    private bool morreu;

    private Coroutine rotinaCombate;
    private Coroutine rotinaAtaque;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        vida = vidaMaxima;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (feedbackDano == null)
            feedbackDano = GetComponent<ControladorFeedBackDano>();
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (animator == null)
        {
            Debug.LogError(
                "BossArvore: Animator não encontrado.",
                this
            );

            return;
        }

        // Mesmo padrão usado na Bruxa.
        animator.SetBool("IsInvocando", false);
        animator.SetBool("IsDead", false);

        animator.Play("Idle", 0, 0f);

        Debug.Log(
            "BossArvore: iniciado em Idle."
        );
    }

    // =========================================================
    // IBOSS
    // =========================================================


    public void IniciarBoss()
    {
        if (morreu || combateIniciado)
            return;

        if (aguardandoDialogoInicial || dialogoInicialConcluido)
            return;

        // Sem diálogo configurado, começa o combate diretamente.
        if (dialogoInicial == null)
        {
            dialogoInicialConcluido = true;
            IniciarCombate();
            return;
        }

        // Impede que o boss fique bloqueado caso o manager não exista.
        if (BossDialogueManager.Instance == null)
        {
            Debug.LogWarning(
                "BossArvore: BossDialogueManager não encontrado. " +
                "O combate será iniciado sem diálogo.",
                this
            );

            dialogoInicialConcluido = true;
            IniciarCombate();
            return;
        }

        aguardandoDialogoInicial = true;

        Debug.Log("BossArvore: iniciando diálogo inicial.");

        BossDialogueManager.Instance.StartBossDialogue(
            dialogoInicial,
            this
        );
    }

    public void ConcluirDialogoInicial()
    {
        if (morreu || combateIniciado)
            return;

        if (dialogoInicialConcluido)
            return;

        aguardandoDialogoInicial = false;
        dialogoInicialConcluido = true;

        Debug.Log("BossArvore: diálogo inicial concluído.");

        IniciarCombate();
    }

    public void IniciarCombate()
    {
        if (morreu || combateIniciado)
            return;

        // Não inicia o combate enquanto o diálogo estiver aberto.
        if (aguardandoDialogoInicial)
            return;

        combateIniciado = true;

        AtualizarEstado();

        Debug.Log("BossArvore: combate iniciado!");

        if (rotinaCombate != null)
            StopCoroutine(rotinaCombate);

        rotinaCombate = StartCoroutine(RotinaCombate());
    }
    // =========================================================
    // ROTINA DE COMBATE
    // =========================================================

    private IEnumerator RotinaCombate()
    {
        // Pequena espera antes do primeiro ataque.
        yield return new WaitForSeconds(1f);

        while (!morreu)
        {
            // =================================================
            // ESPERA ENTRE ATAQUES
            // =================================================

            float intervalo =
                ObterIntervaloAtaque();

            Debug.Log(
                "BossArvore: esperando " +
                intervalo +
                " segundos."
            );

            yield return new WaitForSeconds(
                intervalo
            );

            if (morreu)
                yield break;

            // =================================================
            // ESCOLHER ATAQUE
            // =================================================

            if (!atacando)
            {
                EscolherAtaque();
            }

            // Espera o ataque terminar.
            while (atacando && !morreu)
            {
                yield return null;
            }
        }
    }

    // =========================================================
    // ESCOLHER ATAQUE
    // =========================================================

    private void EscolherAtaque()
    {
        if (morreu || atacando)
            return;

        Debug.Log("BossArvore: escolhendo ataque -> RAÍZES.");

        atacando = true;

        rotinaAtaque = StartCoroutine(AtaqueRaizes());
    }

    // =========================================================
    // ATAQUE DAS RAÍZES
    // =========================================================
    private IEnumerator AtaqueRaizes()
    {
        if (morreu)
            yield break;

        if (animator == null)
        {
            Debug.LogError("BossArvore: Animator não atribuído.", this);
            FinalizarAtaque();
            yield break;
        }

        if (raizPool == null)
        {
            Debug.LogError("BossArvore: BossRaizPool não atribuído.", this);
            FinalizarAtaque();
            yield break;
        }

        float duracao = ObterDuracaoInvocacao();

        Debug.Log("BossArvore: iniciando invocação de raízes.");

        // Inicia a animação de invocação.
        animator.SetBool("IsInvocando", true);

        // Aguarda o momento em que as raízes devem surgir.
        float espera = Mathf.Clamp(
            tempoAteInvocarRaizes,
            0f,
            duracao
        );

        yield return new WaitForSeconds(espera);

        if (morreu)
            yield break;

        // Invoca as raízes sem depender de Animation Event.
        int quantidade = ObterQuantidadeRaizes();

        raizPool.InvocarRaizes(quantidade);

        Debug.Log(
            "BossArvore: invocou " + quantidade + " raízes."
        );

        // Aguarda o restante da animação.
        yield return new WaitForSeconds(
            Mathf.Max(0f, duracao - espera)
        );

        if (morreu)
            yield break;

        // Encerra a animação e libera o próximo ataque.
        animator.SetBool("IsInvocando", false);

        FinalizarAtaque();
    }

    private void FinalizarAtaque()
    {
        atacando = false;
        rotinaAtaque = null;

        Debug.Log("BossArvore: ataque finalizado.");
    }
    // =========================================================
    // DURAÇÃO DA INVocação
    // =========================================================

    private float ObterDuracaoInvocacao()
    {
        switch (estadoAtual)
        {
            case EstadoBoss.Facil:
                return duracaoInvocacaoFacil;

            case EstadoBoss.Medio:
                return duracaoInvocacaoMedio;

            case EstadoBoss.Dificil:
                return duracaoInvocacaoDificil;

            default:
                return duracaoInvocacaoFacil;
        }
    }

    // =========================================================
    // INTERVALO ENTRE ATAQUES
    // =========================================================

    private float ObterIntervaloAtaque()
    {
        switch (estadoAtual)
        {
            case EstadoBoss.Facil:
                return intervaloFacil;

            case EstadoBoss.Medio:
                return intervaloMedio;

            case EstadoBoss.Dificil:
                return intervaloDificil;

            default:
                return intervaloFacil;
        }
    }

    // =========================================================
    // ANIMATION EVENT
    // =========================================================

    public void ExecutarInvocacaoRaizes()
    {
        if (morreu)
            return;

        if (!combateIniciado)
            return;

        if (!atacando)
            return;

        if (raizPool == null)
        {
            Debug.LogWarning(
                "BossArvore: BossRaizPool não foi atribuído.",
                this
            );

            return;
        }

        int quantidade =
            ObterQuantidadeRaizes();

        raizPool.InvocarRaizes(
            quantidade
        );

        Debug.Log(
            "BossArvore: Animation Event -> " +
            "Invocando " +
            quantidade +
            " raízes."
        );
    }

    // =========================================================
    // QUANTIDADE DE RAÍZES
    // =========================================================

    private int ObterQuantidadeRaizes()
    {
        switch (estadoAtual)
        {
            case EstadoBoss.Facil:
                return raizesFacil;

            case EstadoBoss.Medio:
                return raizesMedio;

            case EstadoBoss.Dificil:
                return raizesDificil;

            default:
                return raizesFacil;
        }
    }

    // =========================================================
    // ESTADO
    // =========================================================

    private void AtualizarEstado()
    {
        if (morreu)
        {
            estadoAtual =
                EstadoBoss.Morrendo;

            return;
        }

        if (!combateIniciado)
        {
            estadoAtual =
                EstadoBoss.Parado;

            return;
        }

        float porcentagemVida =
            (float)vida / vidaMaxima;

        if (porcentagemVida <= 0.33f)
        {
            estadoAtual =
                EstadoBoss.Dificil;
        }
        else if (porcentagemVida <= 0.66f)
        {
            estadoAtual =
                EstadoBoss.Medio;
        }
        else
        {
            estadoAtual =
                EstadoBoss.Facil;
        }

        Debug.Log(
            "BossArvore: fase = " +
            estadoAtual
        );
    }

    // =========================================================
    // DANO
    // =========================================================

    public void ReceberDano(int dano)
    {
        if (morreu)
            return;

        if (papoula != null &&
            !papoula.EstaAberta())
        {
            return;
        }

        vida -= dano;

        if (vida < 0)
            vida = 0;

        Debug.Log(
            "BossArvore recebeu dano. Vida: " +
            vida
        );

        if (feedbackDano != null)
        {
            feedbackDano.ExecutarFeedback();
        }

        AtualizarEstado();

        if (vida <= 0)
        {
            Morrer();
        }
    }

    // =========================================================
    // MORTE
    // =========================================================

    private void Morrer()
    {
        if (morreu)
            return;

        morreu = true;

        estadoAtual =
            EstadoBoss.Morrendo;

        atacando = false;

        if (rotinaCombate != null)
        {
            StopCoroutine(
                rotinaCombate
            );

            rotinaCombate = null;
        }

        if (rotinaAtaque != null)
        {
            StopCoroutine(
                rotinaAtaque
            );

            rotinaAtaque = null;
        }

        if (papoula != null)
        {
            papoula.Desativar();
        }

        if (animator != null)
        {
            animator.SetBool(
                "IsInvocando",
                false
            );

            animator.SetBool(
                "IsDead",
                true
            );

            animator.Play(
                "Morrendo",
                0,
                0f
            );
        }

        Debug.Log(
            "BossArvore: morreu."
        );

        StartCoroutine(
            SequenciaMorte()
        );
    }

    // =========================================================
    // SEQUÊNCIA DE MORTE
    // =========================================================


    private IEnumerator SequenciaMorte()
    {
        // Aguarda o início do efeito de morte.
        yield return new WaitForSeconds(tempoAteParticulas);

        if (particulasMorte != null)
        {
            ParticleSystem efeito = Instantiate(
                particulasMorte,
                transform.position,
                Quaternion.identity
            );

            efeito.Play();

            Destroy(
                efeito.gameObject,
                tempoDoEfeitoMorte
            );
        }

        // Aguarda o efeito de morte terminar.
        yield return new WaitForSeconds(tempoDoEfeitoMorte);

        // Guarda a posição e a rotação do boss.
        Vector3 posicao = transform.position;
        Quaternion rotacao = transform.rotation;

        // Cria a árvore curada no lugar do boss.
        if (arvoreCuradaNPC != null)
        {
            Instantiate(
                arvoreCuradaNPC,
                posicao,
                rotacao
            );

            Debug.Log(
                "BossArvore: árvore curada apareceu como NPC."
            );
        }
        else
        {
            Debug.LogWarning(
                "BossArvore: prefab da árvore curada não foi atribuído.",
                this
            );
        }

        // Destrói o boss derrotado.
        Destroy(gameObject);
    }
    // =========================================================
    // ACESSORES
    // =========================================================

    public int GetVida()
    {
        return vida;
    }

    public EstadoBoss GetEstado()
    {
        return estadoAtual;
    }

    public bool EstaMorto()
    {
        return morreu;
    }

    public bool CombateIniciado()
    {
        return combateIniciado;
    }

    public bool EstaAtacando()
    {
        return atacando;
    }
}