using UnityEngine;

public class Obstaculo : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vida = 3;

    [Header("Feedback")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;

    public void ReceberDano(int dano)
    {
        vida -= dano;

        Debug.Log("Obstáculo recebeu " + dano + " de dano.");

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