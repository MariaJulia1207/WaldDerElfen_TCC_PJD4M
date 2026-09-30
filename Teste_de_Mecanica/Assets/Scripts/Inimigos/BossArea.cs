using UnityEngine;

public class BossArena : MonoBehaviour
{
    [Header("Configuração da Arena")]
    [SerializeField] private GameObject aberturaEntrada;
    [SerializeField] private GameObject aberturaSaida;

[Header("Boss")]
    [SerializeField] private GameObject boss;

    [Header("Objeto de Teste - Opcional")]
    [SerializeField] private GameObject objetoTeste;

    private bool combateIniciado = false;
    private bool combateFinalizado = false;

    private IBoss bossScript;

    private void Awake()
    {
        if (boss != null)
        {
            bossScript = boss.GetComponent<IBoss>();

            if (bossScript == null)
            {
                Debug.LogError(
                    "BossArena: O Boss não possui um script que implemente IBoss."
                );
            }
        }
    }

    private void Update()
    {
        // O objeto de teste continua disponível para testes.
        // Se estiver preenchido, sua destruição também pode
        // abrir a arena.
        if (combateIniciado && !combateFinalizado)
        {
            if (objetoTeste != null && objetoTeste == null)
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

        // Fecha as aberturas.
        if (aberturaEntrada != null)
            aberturaEntrada.SetActive(true);

        if (aberturaSaida != null)
            aberturaSaida.SetActive(true);

        Debug.Log("Boss Arena: Combate iniciado!");

        // Inicia o boss.
        if (bossScript != null)
        {
            bossScript.IniciarBoss();
        }
        else
        {
            Debug.LogWarning(
                "BossArena: Nenhum Boss válido foi configurado."
            );
        }
    }

    // =========================================================
    // BOSS DERROTADO
    // =========================================================

    public void BossDerrotado()
    {
        if (combateFinalizado)
            return;

        combateFinalizado = true;

        // Abre as passagens.
        if (aberturaEntrada != null)
            aberturaEntrada.SetActive(false);

        if (aberturaSaida != null)
            aberturaSaida.SetActive(false);

        Debug.Log("Boss Arena: Boss derrotado! Arena aberta.");
    }

}