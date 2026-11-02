using UnityEngine;

public class ProjetilAlberich : MonoBehaviour
{
    [Header("Configuração de Dano")]
    [SerializeField] private int danoNoJogador = 1;
    [SerializeField] private int danoNoBoss = 5;
    [SerializeField] private float tempoVidaMaximo = 5f;

    [Header("Ajuste de Rotação do Sprite")]
    [Tooltip("0 se o sprite aponta para a DIREITA, 180 se aponta para a ESQUERDA, -90 se aponta para CIMA.")]
    [SerializeField] private float offsetAnguloSprite = 180f;

    [Header("Efeitos")]
    [SerializeField] private ParticleSystem particulaImpacto;

    private Rigidbody2D rb;
    private ProjetilPool poolOrigem;
    private bool foiRefletido;
    private bool podeSerRefletido = true;
    private float timerVida;
    private float velocidadeAtual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ConfigurarPool(ProjetilPool pool)
    {
        poolOrigem = pool;
    }

    public void Disparar(Vector2 direcao, float velocidade, bool refletivel = true)
    {
        foiRefletido = false;
        podeSerRefletido = refletivel;
        velocidadeAtual = velocidade;
        timerVida = tempoVidaMaximo;
        gameObject.SetActive(true);

        if (rb != null)
        {
            rb.linearVelocity = direcao.normalized * velocidadeAtual;
        }

        // Atualiza a rotação do sprite no disparo
        AtualizarRotacao(direcao);
    }

    private void Update()
    {
        timerVida -= Time.deltaTime;
        if (timerVida <= 0f)
        {
            DevolverOuDesativar();
        }
    }

    private void AtualizarRotacao(Vector2 direcao)
    {
        if (direcao.sqrMagnitude < 0.001f) return;

        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angulo + offsetAnguloSprite);
    }

    public bool PodeSerRefletido() => podeSerRefletido && !foiRefletido;

    public void Refletir(Vector2 novaDirecao)
    {
        if (!PodeSerRefletido())
            return;

        foiRefletido = true;

        if (rb != null)
        {
            rb.linearVelocity = novaDirecao.normalized * (velocidadeAtual * 1.3f);
        }

        // Atualiza a rotação para virar na direção do rebatimento
        AtualizarRotacao(novaDirecao);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Impacto com Obstáculos ou Paredes
        if (other.CompareTag("Parede") || other.CompareTag("Obstaculo"))
        {
            GerarEfeitoEImpacto();
            return;
        }

        // Se NÃO foi refletido, atinge o Jogador
        if (!foiRefletido && other.CompareTag("Player"))
        {
            other.SendMessage("ReceberDano", danoNoJogador, SendMessageOptions.DontRequireReceiver);
            GerarEfeitoEImpacto();
            return;
        }

        // Se FOI refletido pelo jogador, atinge o Boss Alberich
        if (foiRefletido)
        {
            BossAlberich boss = other.GetComponentInParent<BossAlberich>();
            if (boss != null)
            {
                boss.ReceberDanoFeiticoRefletido(danoNoBoss);
                GerarEfeitoEImpacto();
            }
        }
    }

    private void GerarEfeitoEImpacto()
    {
        if (particulaImpacto != null)
        {
            // Instancia as partículas no espaço do mundo
            ParticleSystem efeito = Instantiate(particulaImpacto, transform.position, Quaternion.identity);
            efeito.gameObject.SetActive(true);
            efeito.Play();

            // Calcula o tempo total necessário: Duração da emissão + Tempo de vida das partículas geradas
            var mainModule = efeito.main;
            float tempoDestruicao = mainModule.duration + mainModule.startLifetime.constantMax;

            Destroy(efeito.gameObject, tempoDestruicao);
        }

        DevolverOuDesativar();
    }

    private void DevolverOuDesativar()
    {
        Projetil componenteProjetil = GetComponent<Projetil>();
        if (poolOrigem != null && componenteProjetil != null)
        {
            poolOrigem.Devolver(componenteProjetil);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}