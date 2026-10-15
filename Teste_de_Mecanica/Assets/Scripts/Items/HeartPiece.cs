using UnityEngine;

public class HeartPiece : MonoBehaviour, IItem
{
    [Header("Configurações do Pedaço de Coração")]
    [SerializeField] private int aumentoVidaMaxima = 1;
    [SerializeField] private bool preencherNovoCoracao = true;

    [Header("Efeito Sonoro (Opcional)")]
    [SerializeField] private string nomeSomColeta = "";

    public void Collect()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        HealthSystem healthSystem = playerObject.GetComponent<HealthSystem>();

        if (healthSystem != null)
        {
            // Aumenta a vida máxima do jogador
            healthSystem.vidaMaxima += aumentoVidaMaxima;

            // Preenche a vida do novo coração coletado
            if (preencherNovoCoracao)
            {
                healthSystem.ReceberCura(aumentoVidaMaxima);
            }

            // Toca o som de coleta
            if (!string.IsNullOrEmpty(nomeSomColeta))
            {
                SoundEffectManager.Play(nomeSomColeta);
            }

            Destroy(gameObject);
        }
    }
}