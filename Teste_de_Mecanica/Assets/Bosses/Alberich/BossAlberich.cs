using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAlberich : MonoBehaviour, IBoss
{
    public enum EstadoBoss
    {
        Parado,
        Teleportando,
        InvocandoAgahnim,
        InvocandoGanon,
        Vulneravel,
        Morrendo
    }

    public enum FaseDificuldade
    {
        Facil,
        Medio,
        Dificil
    }

    [System.Serializable]
    public class ConfiguracaoFase
    {
        public float tempoEntreAtaques = 2.5f;
        public float velocidadeFeitico = 6f;
        public int quantidadeFeiticosAgahnim = 3;
        public float tempoVulneravelDesarmado = 4f;
    }

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 50;
    private int vidaAtual;

    [Header("Estado e Dificuldade")]
    [SerializeField] private EstadoBoss estadoAtual = EstadoBoss.Parado;
    [SerializeField] private FaseDificuldade faseAtual = FaseDificuldade.Facil;

    [Header("Configurações de Fase")]
    [SerializeField] private ConfiguracaoFase faseFacil;
    [SerializeField] private ConfiguracaoFase faseMedia;
    [SerializeField] private ConfiguracaoFase faseDificil;

    [Header("Teletransporte")]
    [SerializeField] private Transform[] pontosTeleporte;
    [SerializeField] private ParticleSystem particulasTeleporte;

    [Header("Ataques & Cetro")]
    [SerializeField] private ProjetilPool poolFeiticos;
    [SerializeField] private Transform pontoDisparoCetro;
    [SerializeField] private GameObject cetroIndependente; // Cetro voador (Ganon style)

    [Header("Componentes")]
    [SerializeField] private Animator animator;
    [SerializeField] private ControladorFeedBackDano feedbackDano;

    [Header("Diálogo Inicial")]
    [SerializeField] private DialogueData dialogoInicial;
    private bool aguardandoDialogoInicial;
    private bool dialogoInicialConcluido;

    [Header("Morte")]
    [SerializeField] private ParticleSystem particulasMorte;
    [SerializeField] private float tempoAteParticulas = 1.5f;
    [SerializeField] private float tempoDoEfeitoMorte = 2f;
    [SerializeField] private GameObject npcAposDerrota;

    private Transform jogador;
    private Coroutine rotinaCombate;
    private Coroutine rotinaEstadoAtual;
    private int ultimoIndiceTeleporte = -1;
    private bool combateIniciado;
    private bool morreu;

    private ConfiguracaoFase ConfiguracaoAtual
    {
        get
        {
            switch (faseAtual)
            {
                case FaseDificuldade.Medio: return faseMedia;
                case FaseDificuldade.Dificil: return faseDificil;
                default: return faseFacil;
            }
        }
    }

    private void Awake()
    {
        vidaAtual = vidaMaxima;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (feedbackDano == null)
            feedbackDano = GetComponent<ControladorFeedBackDano>();
    }

    private void Start()
    {
        EncontrarJogador();
        if (cetroIndependente != null)
            cetroIndependente.SetActive(false);

        ResetarAnimacoes();
    }

    // =========================================================
    // IBOSS IMPLEMENTAÇÃO
    // =========================================================

    public void IniciarBoss()
    {
        if (morreu || combateIniciado)
            return;

        EncontrarJogador();

        if (!dialogoInicialConcluido && dialogoInicial != null)
        {
            aguardandoDialogoInicial = true;

            if (BossDialogueManager.Instance != null)
            {
                BossDialogueManager.Instance.StartBossDialogue(dialogoInicial, this);
            }
            else
            {
                ConcluirDialogoInicial();
            }
            return;
        }

        IniciarCombate();
    }

    public void ConcluirDialogoInicial()
    {
        if (!aguardandoDialogoInicial)
            return;

        aguardandoDialogoInicial = false;
        dialogoInicialConcluido = true;
        IniciarCombate();
    }

    public bool EstaMorto() => morreu;

    public void ResetarBoss()
    {
        if (rotinaCombate != null) StopCoroutine(rotinaCombate);
        if (rotinaEstadoAtual != null) StopCoroutine(rotinaEstadoAtual);

        vidaAtual = vidaMaxima;
        morreu = false;
        combateIniciado = false;
        estadoAtual = EstadoBoss.Parado;

        if (cetroIndependente != null)
            cetroIndependente.SetActive(false);

        ResetarAnimacoes();
    }

    // =========================================================
    // LÓGICA DE COMBATE
    // =========================================================

    private void IniciarCombate()
    {
        combateIniciado = true;
        AtualizarFaseDificuldade();

        if (rotinaCombate != null)
            StopCoroutine(rotinaCombate);

        rotinaCombate = StartCoroutine(RotinaLoopCombate());
    }

    private IEnumerator RotinaLoopCombate()
    {
        yield return new WaitForSeconds(1f);

        while (!morreu)
        {
            // 1. Teletransporta para uma nova posição
            yield return StartCoroutine(ExecutarTeletransporte());

            if (morreu) yield break;

            // 2. Alterna aleatoriamente entre ataque Agahnim (Com Cetro) e Ganon (Lança Cetro)
            bool usarAtaqueAgahnim = Random.value > 0.4f;

            if (usarAtaqueAgahnim)
            {
                yield return StartCoroutine(AtaqueEstiloAgahnim());
            }
            else
            {
                yield return StartCoroutine(AtaqueEstiloGanon());
            }

            yield return new WaitForSeconds(ConfiguracaoAtual.tempoEntreAtaques);
        }
    }

    // =========================================================
    // TELETRANSPORTE
    // =========================================================

    private IEnumerator ExecutarTeletransporte()
    {
        estadoAtual = EstadoBoss.Teleportando;

        // Partícula de saída
        TocarEfeitoTeleporte();
        yield return new WaitForSeconds(0.25f);

        if (pontosTeleporte != null && pontosTeleporte.Length > 0)
        {
            int novoIndice = Random.Range(0, pontosTeleporte.Length);
            if (novoIndice == ultimoIndiceTeleporte && pontosTeleporte.Length > 1)
            {
                novoIndice = (novoIndice + 1) % pontosTeleporte.Length;
            }

            ultimoIndiceTeleporte = novoIndice;
            transform.position = pontosTeleporte[novoIndice].position;
        }

        // Partícula de chegada
        TocarEfeitoTeleporte();
        yield return new WaitForSeconds(0.25f);
    }

    private void TocarEfeitoTeleporte()
    {
        if (particulasTeleporte != null)
        {
            ParticleSystem ps = Instantiate(particulasTeleporte, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration);
        }
    }

    // =========================================================
    // ATAQUE AGAHNIM (Lança Feitiço Rebatível)
    // =========================================================

    private IEnumerator AtaqueEstiloAgahnim()
    {
        estadoAtual = EstadoBoss.InvocandoAgahnim;
        ResetarAnimacoes();
        animator.SetBool("IsInvocandoAgahnim", true);

        yield return new WaitForSeconds(0.5f);

        int tiros = ConfiguracaoAtual.quantidadeFeiticosAgahnim;
        for (int i = 0; i < tiros; i++)
        {
            if (morreu || estadoAtual == EstadoBoss.Vulneravel) yield break;

            DispararFeiticoRebativel();
            yield return new WaitForSeconds(0.6f);
        }

        animator.SetBool("IsInvocandoAgahnim", false);
    }

    private void DispararFeiticoRebativel()
    {
        if (poolFeiticos == null || jogador == null) return;

        Projetil p = poolFeiticos.Pegar();
        if (p == null) return;

        ProjetilAlberich feitico = p.GetComponent<ProjetilAlberich>();
        if (feitico != null)
        {
            Vector3 origem = pontoDisparoCetro != null ? pontoDisparoCetro.position : transform.position;
            p.transform.position = origem;

            Vector2 direcao = (jogador.position - origem).normalized;
            feitico.Disparar(direcao, ConfiguracaoAtual.velocidadeFeitico, true);
        }
    }

    // =========================================================
    // ATAQUE GANON (Gira o Cetro Desarmado & Vulnerabilidade)
    // =========================================================

    private IEnumerator AtaqueEstiloGanon()
    {
        estadoAtual = EstadoBoss.InvocandoGanon;
        ResetarAnimacoes();
        animator.SetBool("IsInvocandoGanon", true);

        // Ativa o Cetro voador independente
        if (cetroIndependente != null)
        {
            cetroIndependente.transform.position = transform.position + new Vector3(0, 1.2f, 0);
            cetroIndependente.SetActive(true);
        }

        yield return new WaitForSeconds(1.0f);

        // O Alberich fica tonto/sem cetro vulnerável a ataques diretos do jogador
        estadoAtual = EstadoBoss.Vulneravel;
        animator.SetBool("IsInvocandoGanon", false);
        animator.SetBool("IsVulnerable", true);

        yield return new WaitForSeconds(ConfiguracaoAtual.tempoVulneravelDesarmado);

        // Retorna o cetro
        if (cetroIndependente != null)
            cetroIndependente.SetActive(false);

        animator.SetBool("IsVulnerable", false);
    }

    // =========================================================
    // RECEBER DANO & VULNERABILIDADE
    // =========================================================

    public void ReceberDanoFeiticoRefletido(int dano)
    {
        if (morreu) return;

        // Ao ser atingido pelo próprio feitiço rebatido, o Boss toma dano pesado e fica atordoado
        AplicarDano(dano);

        if (!morreu)
        {
            if (rotinaEstadoAtual != null) StopCoroutine(rotinaEstadoAtual);
            rotinaEstadoAtual = StartCoroutine(AtordoarPorRebatimento());
        }
    }

    public void ReceberDanoAtaqueDireto(int dano)
    {
        if (morreu) return;

        // O Alberich só aceita dano direto de espada quando estiver VULNERÁVEL (Sem o cetro)
        if (estadoAtual == EstadoBoss.Vulneravel)
        {
            AplicarDano(dano);
        }
    }

    private void AplicarDano(int dano)
    {
        vidaAtual -= dano;
        if (vidaAtual < 0) vidaAtual = 0;

        if (feedbackDano != null)
            feedbackDano.ExecutarFeedback();

        AtualizarFaseDificuldade();

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private IEnumerator AtordoarPorRebatimento()
    {
        estadoAtual = EstadoBoss.Vulneravel;
        ResetarAnimacoes();
        animator.SetBool("IsVulnerable", true);

        if (cetroIndependente != null)
            cetroIndependente.SetActive(false);

        yield return new WaitForSeconds(2.5f);

        animator.SetBool("IsVulnerable", false);
    }

    private void AtualizarFaseDificuldade()
    {
        float pct = (float)vidaAtual / vidaMaxima;
        if (pct <= 0.33f) faseAtual = FaseDificuldade.Dificil;
        else if (pct <= 0.66f) faseAtual = FaseDificuldade.Medio;
        else faseAtual = FaseDificuldade.Facil;
    }

    // =========================================================
    // SEQUÊNCIA DE MORTE
    // =========================================================

    private void Morrer()
    {
        if (morreu) return;

        morreu = true;
        estadoAtual = EstadoBoss.Morrendo;

        if (rotinaCombate != null) StopCoroutine(rotinaCombate);
        if (rotinaEstadoAtual != null) StopCoroutine(rotinaEstadoAtual);

        if (cetroIndependente != null)
            cetroIndependente.SetActive(false);

        ResetarAnimacoes();
        animator.SetBool("IsDead", true);

        StartCoroutine(SequenciaMorte());
    }

    private IEnumerator SequenciaMorte()
    {
        yield return new WaitForSeconds(tempoAteParticulas);

        if (particulasMorte != null)
        {
            ParticleSystem efeito = Instantiate(particulasMorte, transform.position, Quaternion.identity);
            efeito.Play();
            Destroy(efeito.gameObject, tempoDoEfeitoMorte);
        }

        yield return new WaitForSeconds(tempoDoEfeitoMorte);

        if (npcAposDerrota != null)
        {
            Instantiate(npcAposDerrota, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    // =========================================================
    // UTILITÁRIOS
    // =========================================================

    private void ResetarAnimacoes()
    {
        if (animator == null) return;
        animator.SetBool("IsInvocandoAgahnim", false);
        animator.SetBool("IsInvocandoGanon", false);
        animator.SetBool("IsVulnerable", false);
        animator.SetBool("IsDead", false);
    }

    private void EncontrarJogador()
    {
        if (jogador != null) return;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jogador = p.transform;
    }
}