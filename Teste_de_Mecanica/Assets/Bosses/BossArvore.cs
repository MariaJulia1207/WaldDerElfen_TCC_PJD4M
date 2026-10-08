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

    // =========================================================
    // MORTE
    // =========================================================

    [Header("Morte")]
    [SerializeField] private ParticleSystem particulasMorte;
    [SerializeField] private float tempoAteParticulas = 1.5f;
    [SerializeField] private float tempoDoEfeitoMorte = 2f;

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
        if (combateIniciado)
            return;

        if (morreu)
            return;

        combateIniciado = true;

        AtualizarEstado();

        Debug.Log(
            "BossArvore: combate iniciado!"
        );

        if (rotinaCombate != null)
            StopCoroutine(rotinaCombate);

        rotinaCombate =
            StartCoroutine(
                RotinaCombate()
            );
    }

    public void ConcluirDialogoInicial()
    {
        if (morreu)
            return;

        Debug.Log(
            "BossArvore: diálogo inicial concluído."
        );

        IniciarBoss();
    }

    // =========================================================
    // COMPATIBILIDADE
    // =========================================================

    public void IniciarCombate()
    {
        IniciarBoss();
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
        if (morreu)
            return;

        // Por enquanto existe apenas um ataque.
        Debug.Log(
            "BossArvore: escolhendo ataque -> RAÍZES."
        );

        StartCoroutine(
            AtaqueRaizes()
        );
    }

    // =========================================================
    // ATAQUE DAS RAÍZES
    // =========================================================

    private IEnumerator AtaqueRaizes()
    {
        Debug.Log("========== TESTE RAÍZES ==========");

        if (animator != null)
        {
            Debug.Log(
                "Animator encontrado: " +
                animator.gameObject.name
            );

            animator.SetBool(
                "IsInvocando",
                true
            );

            Debug.Log(
                "IsInvocando = TRUE"
            );
        }

        yield return new WaitForSeconds(3f);

        animator.SetBool(
            "IsInvocando",
            false
        );

        Debug.Log(
            "IsInvocando = FALSE"
        );

        atacando = false;
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
        yield return new WaitForSeconds(
            tempoAteParticulas
        );

        if (particulasMorte != null)
        {
            ParticleSystem efeito =
                Instantiate(
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

        yield return new WaitForSeconds(
            tempoDoEfeitoMorte
        );

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