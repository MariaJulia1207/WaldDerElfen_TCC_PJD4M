using UnityEngine;

public class EnemyTakeDamage : MonoBehaviour
{
    [Header("Dados do Inimigo")]
    [SerializeField] private EnemyData enemyData;

    [Header("Vida")]
    private int vida;

    [Header("Feedback")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;

    private void Start()
    {
        vida = enemyData.vidaMaxima;
    }

    public void ReceberDano(int dano)
    {
        vida -= dano;

        Debug.Log("Inimigo recebeu " + dano + " de dano.");

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