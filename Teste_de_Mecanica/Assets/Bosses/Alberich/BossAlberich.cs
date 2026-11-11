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
        [Tooltip("Intervalo de tempo entre o lançamento de cada feitiço individual.")]
        public float intervaloEntreFeiticos = 1.2f;
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

    [Header("Pontos do Boss")]
    [SerializeField] private Transform pontoMaoAlberich;
    [SerializeField] private Transform pontoDisparoCetro;

    [Header("Ataques & Cetro")]
    [SerializeField] private ProjetilPool poolFeiticos;
    [SerializeField] private CetroAlberich scriptCetroIndependente;

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

    private Transform jogador;
    private Coroutine rotinaCombate;
    private Coroutine rotinaEstadoAtual;
    private int ultimoIndiceTeleporte = -1;
    private bool combateIniciado;
    private bool morreu;
    private bool aguardandoFimTeleporte;
    private bool jaMoveuNesteTeleporte;

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

        if (scriptCetroIndependente != null)
            scriptCetroIndependente.gameObject.SetActive(false);

        ResetarAnimacoes();
    }

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
        aguardandoFimTeleporte = false;
        estadoAtual = EstadoBoss.Parado;

        if (scriptCetroIndependente != null)
            scriptCetroIndependente.gameObject.SetActive(false);

        ResetarAnimacoes();
    }

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
            yield return StartCoroutine(ExecutarTeletransporte());

            if (morreu) yield break;

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

    private IEnumerator ExecutarTeletransporte()
    {
        estadoAtual = EstadoBoss.Teleportando;
        ResetarAnimacoes();

        aguardandoFimTeleporte = true;
        jaMoveuNesteTeleporte = false;

        if (animator != null && animator.gameObject.activeInHierarchy)
        {
            animator.Play("Teleporting", 0, 0f);
            animator.Update(0f);
        }
        else
        {
            MoverParaNovoPonto();
            aguardandoFimTeleporte = false;
        }

        float timeoutSeguranca = 3.0f;

        while (aguardandoFimTeleporte && timeoutSeguranca > 0f)
        {
            timeoutSeguranca -= Time.deltaTime;
            yield return null;
        }

        if (aguardandoFimTeleporte)
        {
            Debug.LogWarning("BossAlberich: Animation Event não disparou a tempo. Recuperando via Timeout.", this);
            MoverParaNovoPonto();
            OnAnimacaoTeleportedConcluida();
        }

        estadoAtual = EstadoBoss.Parado;
    }

    public void OnAnimacaoTeleportingConcluida()
    {
        MoverParaNovoPonto();

        if (animator != null && animator.gameObject.activeInHierarchy)
        {
            animator.Play("Teleported", 0, 0f);
            animator.Update(0f);
        }
        else
        {
            aguardandoFimTeleporte = false;
        }
    }

    public void OnAnimacaoTeleportedConcluida()
    {
        aguardandoFimTeleporte = false;
        ResetarAnimacoes();

        if (animator != null && animator.gameObject.activeInHierarchy)
        {
            animator.Play("Alberich_Idle", 0, 0f);
        }
    }

    private void MoverParaNovoPonto()
    {
        if (jaMoveuNesteTeleporte)
            return;

        jaMoveuNesteTeleporte = true;

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
    }

    private IEnumerator AtaqueEstiloAgahnim()
    {
        estadoAtual = EstadoBoss.InvocandoAgahnim;
        ResetarAnimacoes();
        animator.SetBool("IsInvocandoAgahnim", true);

        yield return new WaitForSeconds(0.4f);

        int tiros = ConfiguracaoAtual.quantidadeFeiticosAgahnim;
        for (int i = 0; i < tiros; i++)
        {
            if (morreu || estadoAtual == EstadoBoss.Vulneravel) yield break;

            DispararFeiticoRebativel();
            yield return new WaitForSeconds(ConfiguracaoAtual.intervaloEntreFeiticos);
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

            // Toca o efeito sonoro do feitiço
            SoundEffectManager.Play("Launch");
        }
    }

    private IEnumerator AtaqueEstiloGanon()
    {
        estadoAtual = EstadoBoss.InvocandoGanon;
        ResetarAnimacoes();
        animator.SetBool("IsInvocandoGanon", true);

        bool ataqueConcluido = false;

        if (scriptCetroIndependente != null)
        {
            Transform pontoOrigem = pontoMaoAlberich != null ? pontoMaoAlberich : transform;
            scriptCetroIndependente.IniciarAtaque(pontoOrigem, jogador, () =>
            {
                ataqueConcluido = true;
            });

            // Toca o efeito sonoro de lançamento do cetro
            SoundEffectManager.Play("ScepterLaunch");
        }
        else
        {
            ataqueConcluido = true;
        }

        estadoAtual = EstadoBoss.Vulneravel;

        yield return new WaitUntil(() => ataqueConcluido);

        animator.SetBool("IsInvocandoGanon", false);
        estadoAtual = EstadoBoss.Parado;
    }

    public void ReceberDanoFeiticoRefletido(int dano)
    {
        if (morreu) return;

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

        if (estadoAtual == EstadoBoss.Vulneravel)
        {
            AplicarDano(dano);
        }
    }

    private void AplicarDano(int dano)
    {
        vidaAtual -= dano;
        if (vidaAtual < 0) vidaAtual = 0;

        // Toca o efeito sonoro de dano
        SoundEffectManager.Play("EnemyDamage");

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

        if (scriptCetroIndependente != null)
            scriptCetroIndependente.gameObject.SetActive(false);

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

    private void Morrer()
    {
        if (morreu) return;

        morreu = true;
        estadoAtual = EstadoBoss.Morrendo;

        if (rotinaCombate != null) StopCoroutine(rotinaCombate);
        if (rotinaEstadoAtual != null) StopCoroutine(rotinaEstadoAtual);

        if (scriptCetroIndependente != null)
            scriptCetroIndependente.gameObject.SetActive(false);

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

        // O spawn do NPC/Cristal é gerenciado centralizadamente pelo BossArena.
        Destroy(gameObject);
    }

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