using UnityEngine;

public class Obstaculo : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vida = 3;

    [Header("Feedback")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;

    [Header("Som de dano")]
    [SerializeField] private string soundEffectName = "Damage";

    public void ReceberDano(int dano)
    {
        if (dano <= 0)
            return;

        vida -= dano;

        Debug.Log("Obstáculo recebeu " + dano + " de dano.");

        if (!string.IsNullOrWhiteSpace(soundEffectName))
        {
            SoundEffectManager.Play(soundEffectName);
        }

        // Feedback visual
        if (feedbackDano != null)
        {
            feedbackDano.ExecutarFeedback();
        }

        if (vida <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        Destroy(gameObject);
    }
}