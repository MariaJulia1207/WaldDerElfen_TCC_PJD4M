using UnityEngine;

public class BossArena : MonoBehaviour
{
    [Header("Configuração da Arena")]
    [SerializeField] private GameObject aberturaEntrada;
    [SerializeField] private GameObject aberturaSaida;

    [Header("Boss")]
    [SerializeField] private GameObject bossObjeto;
    [SerializeField] private string saveId;
    private IBoss boss;

    [Header("NPC Pós-Derrota")]
    [Tooltip("Pode ser o Prefab do NPC ou um GameObject de NPC já colocado na hierarquia da cena.")]
    [SerializeField] private GameObject arvoreCuradaNPC;
    [Tooltip("Ponto onde o NPC vai surgir (opcional, usa a posição do Boss se ficar vazio).")]
    [SerializeField] private Transform pontoSpawnNPC;

    [Header("Objeto de Teste - Opcional")]
    [SerializeField] private GameObject objetoTeste;

    private bool combateIniciado = false;
    private bool combateFinalizado = false;
    private GameObject npcInstanciado;

    public string SaveId => string.IsNullOrEmpty(saveId) ? gameObject.name : saveId;
    public bool IsDefeated => combateFinalizado;

    private void Awake()
    {
        if (bossObjeto != null)
            boss = bossObjeto.GetComponent<IBoss>();
    }

    private void Update()
    {
        if (combateIniciado && !combateFinalizado)
        {
            // 1. Verificação via objeto de teste
            if (objetoTeste != null && objetoTeste == null)
            {
                BossDerrotado();
                return;
            }

            // 2. Verificação se o GameObject do Boss foi destruído
            if (bossObjeto == null)
            {
                BossDerrotado();
                return;
            }

            // 3. Verificação se o Boss morreu
            if (boss != null && boss.EstaMorto())
            {
                BossDerrotado();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (combateIniciado)
            return;

        if (!other.CompareTag("Player"))
            return;

        IniciarCombate();
    }

    private void IniciarCombate()
    {
        combateIniciado = true;

        if (aberturaEntrada != null)
            aberturaEntrada.SetActive(true);

        if (aberturaSaida != null)
            aberturaSaida.SetActive(true);

        if (boss != null)
            boss.IniciarBoss();

        Debug.Log("Boss Arena: Combate iniciado!");
    }

    public void ApplySaveState(bool defeated)
    {
        combateFinalizado = defeated;

        if (defeated)
        {
            combateIniciado = true;

            if (bossObjeto != null)
                bossObjeto.SetActive(false);

            if (aberturaEntrada != null)
                aberturaEntrada.SetActive(false);

            if (aberturaSaida != null)
                aberturaSaida.SetActive(false);

            GarantirNPCNaCena();
        }
        else
        {
            // Se o boss NÃO foi derrotado (ex: jogador morreu e voltou)
            combateIniciado = false;

            if (bossObjeto != null)
                bossObjeto.SetActive(true);

            // Mantém as passagens abertas até o jogador entrar na arena novamente
            if (aberturaEntrada != null)
                aberturaEntrada.SetActive(false);

            if (aberturaSaida != null)
                aberturaSaida.SetActive(false);

            // Reseta o estado interno do boss
            if (boss != null)
            {
                boss.ResetarBoss();
            }
        }
    }

    public void BossDerrotado()
    {
        if (combateFinalizado)
            return;

        combateFinalizado = true;

        if (aberturaEntrada != null)
            aberturaEntrada.SetActive(false);

        if (aberturaSaida != null)
            aberturaSaida.SetActive(false);

        GarantirNPCNaCena();

        Debug.Log("Boss Arena: Boss derrotado! Arena aberta e NPC garantido.");
    }

    public void GarantirNPCNaCena()
    {
        if (arvoreCuradaNPC == null)
        {
            Debug.LogWarning("BossArena: Referência do 'arvoreCuradaNPC' não foi atribuída no Inspector.", this);
            return;
        }

        if (arvoreCuradaNPC.scene.IsValid())
        {
            arvoreCuradaNPC.SetActive(true);
            return;
        }

        if (npcInstanciado == null)
        {
            Vector3 posicao = pontoSpawnNPC != null ? pontoSpawnNPC.position :
                             (bossObjeto != null ? bossObjeto.transform.position : transform.position);
            Quaternion rotacao = pontoSpawnNPC != null ? pontoSpawnNPC.rotation : Quaternion.identity;

            npcInstanciado = Instantiate(arvoreCuradaNPC, posicao, rotacao);
        }
    }
}