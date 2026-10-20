using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArvore : MonoBehaviour, IBoss
{
    // =========================================================
    // IDENTIFICAÇÃO DE SAVE
    // =========================================================
    [Header("Identificação do Save")]
    [SerializeField] private string bossId = "BossArvore";
    
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

    private enum TipoAtaque
    {
        Nenhum,
        Raizes,
        BracoEsquerdo,
        BracoDireito
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

    [Header("Duração da invocação (Raízes)")]
    [SerializeField] private float duracaoInvocacaoFacil = 3f;
    [SerializeField] private float duracaoInvocacaoMedio = 4f;
    [SerializeField] private float duracaoInvocacaoDificil = 5f;

    [Header("Duração Ataque dos Braços")]
    [SerializeField] private float duracaoAtaqueBraco;

    [Header("Hitboxes dos Braços")]
    [SerializeField] private BossHitboxBraco hitboxBracoEsquerdo;
    [SerializeField] private BossHitboxBraco hitboxBracoDireito;

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

    private TipoAtaque ultimoAtaque = TipoAtaque.Nenhum;

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
            Debug.LogError("BossArvore: Animator não encontrado.", this);
            return;
        }

        animator.SetBool("IsInvocando", false);
        animator.SetBool("IsAtacandoEsquerdo", false);
        animator.SetBool("IsAtacandoDireito", false);
        animator.SetBool("IsDead", false);

        animator.Play("Idle", 0, 0f);

        Debug.Log("BossArvore: iniciado em Idle.");
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

        if (dialogoInicial == null)
        {
            dialogoInicialConcluido = true;
            IniciarCombate();
            return;
        }

        if (BossDialogueManager.Instance == null)
        {
            Debug.LogWarning("BossArvore: BossDialogueManager não encontrado. O combate será iniciado sem diálogo.", this);
            dialogoInicialConcluido = true;
            IniciarCombate();
            return;
        }

        aguardandoDialogoInicial = true;
        Debug.Log("BossArvore: iniciando diálogo inicial.");

        BossDialogueManager.Instance.StartBossDialogue(dialogoInicial, this);
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
        yield return new WaitForSeconds(1f);

        while (!morreu)
        {
            float intervalo = ObterIntervaloAtaque();
            Debug.Log("BossArvore: esperando " + intervalo + " segundos.");

            yield return new WaitForSeconds(intervalo);

            if (morreu)
                yield break;

            if (!atacando)
            {
                EscolherAtaque();
            }

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

        List<TipoAtaque> ataquesDisponiveis = new List<TipoAtaque>();
        ataquesDisponiveis.Add(TipoAtaque.Raizes);

        if (ultimoAtaque != TipoAtaque.BracoEsquerdo)
        {
            ataquesDisponiveis.Add(TipoAtaque.BracoEsquerdo);
        }

        if (ultimoAtaque != TipoAtaque.BracoDireito)
        {
            ataquesDisponiveis.Add(TipoAtaque.BracoDireito);
        }

        int indiceSorteado = Random.Range(0, ataquesDisponiveis.Count);
        TipoAtaque ataqueEscolhido = ataquesDisponiveis[indiceSorteado];

        ultimoAtaque = ataqueEscolhido;
        atacando = true;

        switch (ataqueEscolhido)
        {
            case TipoAtaque.Raizes:
                Debug.Log("BossArvore: escolheu ataque -> RAÍZES.");
                rotinaAtaque = StartCoroutine(AtaqueRaizes());
                break;

            case TipoAtaque.BracoEsquerdo:
                Debug.Log("BossArvore: escolheu ataque -> BRAÇO ESQUERDO.");
                rotinaAtaque = StartCoroutine(AtaqueBracoEsquerdo());
                break;

            case TipoAtaque.BracoDireito:
                Debug.Log("BossArvore: escolheu ataque -> BRAÇO DIREITO.");
                rotinaAtaque = StartCoroutine(AtaqueBracoDireito());
                break;
        }
    }

    // =========================================================
    // CORROTINAS DOS BRAÇOS
    // =========================================================

    private IEnumerator AtaqueBracoEsquerdo()
    {
        if (morreu)
            yield break;

        if (hitboxBracoEsquerdo != null)
        {
            hitboxBracoEsquerdo.ResetarHitbox();
        }

        if (animator != null)
        {
            animator.SetBool("IsAtacandoEsquerdo", true);
        }

        yield return new WaitForSeconds(duracaoAtaqueBraco);

        if (animator != null)
        {
            animator.SetBool("IsAtacandoEsquerdo", false);
        }

        FinalizarAtaque();
    }

    private IEnumerator AtaqueBracoDireito()
    {
        if (morreu)
            yield break;

        if (hitboxBracoDireito != null)
        {
            hitboxBracoDireito.ResetarHitbox();
        }

        if (animator != null)
        {
            animator.SetBool("IsAtacandoDireito", true);
        }

        yield return new WaitForSeconds(duracaoAtaqueBraco);

        if (animator != null)
        {
            animator.SetBool("IsAtacandoDireito", false);
        }

        FinalizarAtaque();
    }

    public void FinalizarAtaqueAnimacao()
    {
        if (morreu)
            return;

        DesativarBoolsAtaque();
        FinalizarAtaque();
    }

    private void DesativarBoolsAtaque()
    {
        if (animator != null)
        {
            animator.SetBool("IsAtacandoEsquerdo", false);
            animator.SetBool("IsAtacandoDireito", false);
            animator.SetBool("IsInvocando", false);
        }
    }

    // =========================================================
    // ATAQUE DAS RAÍZES
    // =========================================================

    private IEnumerator AtaqueRaizes()
    {
        if (morreu)
            yield break;

        if (animator == null || raizPool == null)
        {
            FinalizarAtaque();
            yield break;
        }

        float duracao = ObterDuracaoInvocacao();
        animator.SetBool("IsInvocando", true);

        float espera = Mathf.Clamp(tempoAteInvocarRaizes, 0f, duracao);
        yield return new WaitForSeconds(espera);

        if (morreu)
            yield break;

        int quantidade = ObterQuantidadeRaizes();
        raizPool.InvocarRaizes(quantidade);

        yield return new WaitForSeconds(Mathf.Max(0f, duracao - espera));

        if (morreu)
            yield break;

        animator.SetBool("IsInvocando", false);
        FinalizarAtaque();
    }

    private void FinalizarAtaque()
    {
        atacando = false;
        rotinaAtaque = null;
    }

    // =========================================================
    // AUXILIARES E PARÂMETROS
    // =========================================================

    private float ObterDuracaoInvocacao()
    {
        switch (estadoAtual)
        {
            case EstadoBoss.Facil: return duracaoInvocacaoFacil;
            case EstadoBoss.Medio: return duracaoInvocacaoMedio;
            case EstadoBoss.Dificil: return duracaoInvocacaoDificil;
            default: return duracaoInvocacaoFacil;
        }
    }

    private float ObterIntervaloAtaque()
    {
        switch (estadoAtual)
        {
            case EstadoBoss.Facil: return intervaloFacil;
            case EstadoBoss.Medio: return intervaloMedio;
            case EstadoBoss.Dificil: return intervaloDificil;
            default: return intervaloFacil;
        }
    }

    public void ExecutarInvocacaoRaizes()
    {
        if (morreu || !combateIniciado || !atacando || raizPool == null)
            return;

        int quantidade = ObterQuantidadeRaizes();
        raizPool.InvocarRaizes(quantidade);
    }

    private int ObterQuantidadeRaizes()
    {
        switch (estadoAtual)
        {
            case EstadoBoss.Facil: return raizesFacil;
            case EstadoBoss.Medio: return raizesMedio;
            case EstadoBoss.Dificil: return raizesDificil;
            default: return raizesFacil;
        }
    }

    // =========================================================
    // ESTADO
    // =========================================================

    private void AtualizarEstado()
    {
        if (morreu)
        {
            estadoAtual = EstadoBoss.Morrendo;
            return;
        }

        if (!combateIniciado)
        {
            estadoAtual = EstadoBoss.Parado;
            return;
        }

        float porcentagemVida = (float)vida / vidaMaxima;

        if (porcentagemVida <= 0.33f)
        {
            estadoAtual = EstadoBoss.Dificil;
        }
        else if (porcentagemVida <= 0.66f)
        {
            estadoAtual = EstadoBoss.Medio;
        }
        else
        {
            estadoAtual = EstadoBoss.Facil;
        }
    }

    // =========================================================
    // DANO E MORTE
    // =========================================================

    public void ReceberDano(int dano)
    {
        if (morreu)
            return;

        if (papoula != null && !papoula.EstaAberta())
        {
            return;
        }

        vida -= dano;

        if (vida < 0)
            vida = 0;

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

    private void Morrer()
    {
        if (morreu)
            return;

        morreu = true;
        estadoAtual = EstadoBoss.Morrendo;
        atacando = false;

        if (rotinaCombate != null)
        {
            StopCoroutine(rotinaCombate);
            rotinaCombate = null;
        }

        if (rotinaAtaque != null)
        {
            StopCoroutine(rotinaAtaque);
            rotinaAtaque = null;
        }

        if (papoula != null)
        {
            papoula.Desativar();
        }

        if (animator != null)
        {
            animator.SetBool("IsInvocando", false);
            animator.SetBool("IsDead", true);
            animator.Play("Morrendo", 0, 0f);
        }

        Debug.Log("BossArvore: morreu.");
        StartCoroutine(SequenciaMorte());
    }

    private IEnumerator SequenciaMorte()
    {
        // 1. Aguarda o tempo da animação antes das partículas
        yield return new WaitForSeconds(tempoAteParticulas);

        // 2. Instancia e toca as partículas
        if (particulasMorte != null)
        {
            ParticleSystem efeito = Instantiate(
                particulasMorte,
                transform.position,
                Quaternion.identity
            );

            efeito.Play();
            Destroy(efeito.gameObject, tempoDoEfeitoMorte);
        }

        // 3. Aguarda o tempo de efeito do impacto da morte
        yield return new WaitForSeconds(tempoDoEfeitoMorte);

        // 4. Instancia a Árvore Curada no mesmo local
        Vector3 posicao = transform.position;
        Quaternion rotacao = transform.rotation;

        if (arvoreCuradaNPC != null)
        {
            Instantiate(arvoreCuradaNPC, posicao, rotacao);
            Debug.Log("BossArvore: árvore curada apareceu como NPC.");
        }
        else
        {
            Debug.LogWarning("BossArvore: prefab da árvore curada não foi atribuído.", this);
        }

        // 5. Destrói o GameObject do Boss
        Destroy(gameObject);
    }

    // =========================================================
    // ACESSORES / IBOSS
    // =========================================================

    public int GetVida() => vida;
    public EstadoBoss GetEstado() => estadoAtual;
    public bool EstaMorto() => morreu;
    public bool CombateIniciado() => combateIniciado;
    public bool EstaAtacando() => atacando;
}