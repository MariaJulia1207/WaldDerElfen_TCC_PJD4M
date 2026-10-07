using System.Collections;
using UnityEngine;

public class BossArvore : MonoBehaviour, IBoss
{
    // =========================================================
    // ESTADOS
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

    [Header("Referências")]
    [SerializeField] private Animator animator;
    [SerializeField] private BossPapoula papoula;

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

    [Header("Ataques")]
    [SerializeField] private float intervaloAtaqueFacil = 5f;
    [SerializeField] private float intervaloAtaqueMedio = 3.5f;
    [SerializeField] private float intervaloAtaqueDificil = 2.5f;

    [Header("Duração dos Ataques")]
    [SerializeField] private float duracaoAtaqueBraco = 1.5f;
    [SerializeField] private float duracaoInvocacao = 1.5f;

    [Header("Raízes")]
    [SerializeField] private BossRaizPool raizPool;

    [Header("Quantidade de raízes")]
    [SerializeField] private int raizesFacil = 2;
    [SerializeField] private int raizesMedio = 4;
    [SerializeField] private int raizesDificil = 6;

    private bool combateIniciado = false;
    private bool morreu = false;
    private bool atacando = false;

    private Coroutine rotinaCombate;
    private bool aguardandoDialogoInicial;
    private bool dialogoInicialConcluido;

    // =========================================================
    // INÍCIO
    // =========================================================

    private void Start()
    {
        vida = vidaMaxima;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    // =========================================================
    // INICIAR COMBATE
    // =========================================================

    public void IniciarBoss()
    {
        if (combateIniciado || morreu)
            return;

        combateIniciado = true;

        AtualizarEstado();

        rotinaCombate = StartCoroutine(RotinaCombate());

        Debug.Log("Boss Árvore: combate iniciado!");
    }

    public void ConcluirDialogoInicial()
    {
        Debug.Log("Boss Árvore: diálogo inicial concluído.");

        IniciarBoss();
    }


    public void IniciarCombate()
    {
        if (combateIniciado || morreu)
            return;

        combateIniciado = true;

        AtualizarEstado();

        rotinaCombate = StartCoroutine(RotinaCombate());

        Debug.Log("Boss Árvore: combate iniciado!");
    }

    // =========================================================
    // DANO
    // =========================================================

    public void ReceberDano(int dano)
    {
        if (morreu)
            return;

        // Só recebe dano enquanto a papoula está aberta
        if (papoula != null && !papoula.EstaAberta())
            return;

        vida -= dano;

        if (vida < 0)
            vida = 0;

        Debug.Log("Boss Árvore recebeu dano. Vida: " + vida);

        if (feedbackDano != null)
            feedbackDano.ExecutarFeedback();

        AtualizarEstado();

        if (vida <= 0)
        {
            Morrer();
        }
    }

    // =========================================================
    // ATUALIZAR ESTADO
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

        // 35 - 24 = Fácil
        if (vida >= 24)
        {
            estadoAtual = EstadoBoss.Facil;
        }
        // 23 - 12 = Médio
        else if (vida >= 12)
        {
            estadoAtual = EstadoBoss.Medio;
        }
        // 11 - 1 = Difícil
        else
        {
            estadoAtual = EstadoBoss.Dificil;
        }

        Debug.Log("Estado do Boss: " + estadoAtual);
    }

    // =========================================================
    // ROTINA DE COMBATE
    // =========================================================

    private IEnumerator RotinaCombate()
    {
        while (!morreu)
        {
            float intervalo = ObterIntervaloAtaque();

            yield return new WaitForSeconds(intervalo);

            if (morreu)
                yield break;

            if (atacando)
                continue;

            EscolherAtaque();
        }
    }

    // =========================================================
    // INTERVALO DE ATAQUE
    // =========================================================

    private float ObterIntervaloAtaque()
    {
        switch (estadoAtual)
        {
            case EstadoBoss.Facil:
                return intervaloAtaqueFacil;

            case EstadoBoss.Medio:
                return intervaloAtaqueMedio;

            case EstadoBoss.Dificil:
                return intervaloAtaqueDificil;

            default:
                return 5f;
        }
    }

    // =========================================================
    // ESCOLHER ATAQUE
    // =========================================================

    private void EscolherAtaque()
    {
        int ataque = Random.Range(0, 2);

        if (ataque == 0)
        {
            StartCoroutine(AtaqueBraco());
        }
        else
        {
            StartCoroutine(InvocarRaizes());
        }
    }

    // =========================================================
    // ATAQUE COM BRAÇO
    // =========================================================

    private IEnumerator AtaqueBraco()
    {
        atacando = true;

        // Escolhe aleatoriamente o braço
        bool usarBracoEsquerdo = Random.value < 0.5f;

        if (usarBracoEsquerdo)
        {
            if (animator != null)
                animator.SetTrigger("AtaqueBracoEsquerdo");
        }
        else
        {
            if (animator != null)
                animator.SetTrigger("AtaqueBracoDireito");
        }

        Debug.Log(
            usarBracoEsquerdo
            ? "Boss: ataque com braço esquerdo"
            : "Boss: ataque com braço direito"
        );

        // Tempo provisório até a animação ser adicionada
        yield return new WaitForSeconds(duracaoAtaqueBraco);

        atacando = false;
    }

    // =========================================================
    // INVOCAR RAÍZES
    // =========================================================

    private IEnumerator InvocarRaizes()
    {
        atacando = true;

        if (animator != null)
            animator.SetTrigger("Invocar");

        Debug.Log("Boss: preparando invocação de raízes!");

        yield return null;
    }
    
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
                return 0;
        }
    }

    public void ExecutarInvocacaoRaizes()
    {
        if (morreu)
            return;

        if (raizPool == null)
            return;

        int quantidade = ObterQuantidadeRaizes();

        raizPool.InvocarRaizes(quantidade);

        Debug.Log(
            "Boss invocou " +
            quantidade +
            " raízes."
        );
    }

    public void FinalizarAtaque()
    {
        atacando = false;
    }

    // =========================================================
    // MORTE
    // =========================================================

    private void Morrer()
    {
        if (morreu)
            return;

        morreu = true;
        estadoAtual = EstadoBoss.Morrendo;

        if (rotinaCombate != null)
            StopCoroutine(rotinaCombate);

        if (papoula != null)
            papoula.Desativar();

        if (animator != null)
            animator.SetTrigger("Morrer");

        StartCoroutine(SequenciaMorte());
    }

    // =========================================================
    // SEQUÊNCIA DE MORTE
    // =========================================================

    private IEnumerator SequenciaMorte()
    {
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

        yield return new WaitForSeconds(tempoDoEfeitoMorte);

        Destroy(gameObject);
    }

    // =========================================================
    // ACESSO
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

}