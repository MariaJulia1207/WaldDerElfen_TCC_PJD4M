using UnityEngine;
using UnityEngine.UI;

public class LifeHUD : MonoBehaviour
{
    [Header("Corações")]
    [SerializeField] private Image[] coracao;

    [SerializeField] private Sprite cheio;
    [SerializeField] private Sprite vazio;

    private HealthSystem healthSystem;

    private void OnEnable()
    {
        EncontrarPlayer();
    }

    private void EncontrarPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("LifeHUD: Player não encontrado.");
            return;
        }

        healthSystem =
            playerObject.GetComponent<HealthSystem>();

        if (healthSystem == null)
        {
            Debug.LogWarning(
                "LifeHUD: Player não possui HealthSystem."
            );

            return;
        }

        AtualizarHUD();
    }

    private void Update()
    {
        if (healthSystem == null)
        {
            EncontrarPlayer();
            return;
        }

        AtualizarHUD();
    }

    private void AtualizarHUD()
    {
        if (healthSystem == null)
            return;

        int vida = healthSystem.vida;
        int vidaMaxima = healthSystem.vidaMaxima;

        for (int i = 0; i < coracao.Length; i++)
        {
            if (i < vida)
            {
                coracao[i].sprite = cheio;
            }
            else
            {
                coracao[i].sprite = vazio;
            }

            coracao[i].enabled = i < vidaMaxima;
        }
    }
}