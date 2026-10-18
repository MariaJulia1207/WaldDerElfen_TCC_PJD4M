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

    [Header("Objeto de Teste - Opcional")]
    [SerializeField] private GameObject objetoTeste;

    private bool combateIniciado = false;
    private bool combateFinalizado = false;

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
        combateIniciado = defeated || combateIniciado;

        // Ao carregar um save onde o boss já foi derrotado, esconde o objeto imediatamente
        if (bossObjeto != null)
        {
            bossObjeto.SetActive(!defeated);
        }

        if (defeated)
        {
            if (aberturaEntrada != null)
                aberturaEntrada.SetActive(false);

            if (aberturaSaida != null)
                aberturaSaida.SetActive(false);
        }
        else if (combateIniciado)
        {
            if (aberturaEntrada != null)
                aberturaEntrada.SetActive(true);

            if (aberturaSaida != null)
                aberturaSaida.SetActive(true);
        }
    }

    public void BossDerrotado()
    {
        if (combateFinalizado)
            return;

        combateFinalizado = true;

        // Abre as passagens da arena
        if (aberturaEntrada != null)
            aberturaEntrada.SetActive(false);

        if (aberturaSaida != null)
            aberturaSaida.SetActive(false);

        // O bossObjeto NÃO é desativado via SetActive(false) aqui para permitir
        // que a animação de morte, partículas e o spawn do NPC terminem normalmente.

        Debug.Log("Boss Arena: Boss derrotado! Arena aberta.");
    }
}