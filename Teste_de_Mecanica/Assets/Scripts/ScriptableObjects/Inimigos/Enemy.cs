using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Variáveis do inimigo
    [Header("Componentes")]
    public EnemyObject enemyData;
    public GameObject player;
    public float distanciaAtaque = 1.5f;
    // Variáveis de ataque em top-down
    [Header("TriggerDamage")] 
    [SerializeField] private TriggerDamage triggerDamage;
    [SerializeField] private GameObject hitboxAtaqueUP;
    [SerializeField] private GameObject hitboxAtaqueDOWN;
    [SerializeField] private GameObject hitboxAtaqueLEFT;
    [SerializeField] private GameObject hitboxAtaqueRIGHT;
    // Feedback de dano
    [Header("Feedback")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;
    
    public void ReceberDano(int dano)
    {
        enemyData.vida -= dano;

        Debug.Log(enemyData.enemyName + " recebeu " + dano + " de dano.");

        // Feedback visual
        if (feedbackDano != null)
        {
            feedbackDano.ExecutarFeedback();
        }

        if (enemyData.vida <= 0)
        {
            Morrer();
        }
    }
    
    public void Morrer()
    {
        Destroy(gameObject);
    }
}
