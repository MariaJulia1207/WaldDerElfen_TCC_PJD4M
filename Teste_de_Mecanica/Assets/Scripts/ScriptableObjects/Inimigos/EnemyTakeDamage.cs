using UnityEngine;

public class EnemyTakeDamage : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 3;

    [Header("Feedback")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;

    private int vida;

    private void Start()
    {
        vida = vidaMaxima;
    }

    public void ReceberDano(int dano)
    {
        vida -= dano;

        Debug.Log("Inimigo recebeu " + dano + " de dano. Vida restante: " + vida);

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