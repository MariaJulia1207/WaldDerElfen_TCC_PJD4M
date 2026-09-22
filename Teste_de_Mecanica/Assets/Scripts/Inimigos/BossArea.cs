using UnityEngine;

public class BossArena : MonoBehaviour
{
    [Header("Configuração da Arena")]
    [SerializeField] private GameObject aberturaEntrada;
    [SerializeField] private GameObject aberturaSaida;

    [Header("Objeto de Teste")]
    [SerializeField] private GameObject objetoTeste;

    private bool combateIniciado = false;
    private bool combateFinalizado = false;

    private void Update()
    {
        // Se o combate começou e o objeto foi destruído
        if (combateIniciado && !combateFinalizado)
        {
            if (objetoTeste == null)
            {
                AbrirArena();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (combateIniciado)
            return;

        if (other.CompareTag("Player"))
        {
            IniciarCombate();
        }
    }

    private void IniciarCombate()
    {
        combateIniciado = true;

        // Fecha as aberturas
        if (aberturaEntrada != null)
            aberturaEntrada.SetActive(true);

        if (aberturaSaida != null)
            aberturaSaida.SetActive(true);

        Debug.Log("Boss Arena: Combate iniciado!");
    }

    private void AbrirArena()
    {
        combateFinalizado = true;

        // Abre as aberturas
        if (aberturaEntrada != null)
            aberturaEntrada.SetActive(false);

        if (aberturaSaida != null)
            aberturaSaida.SetActive(false);

        Debug.Log("Boss Arena: Boss derrotado! Arena aberta.");
    }
}
