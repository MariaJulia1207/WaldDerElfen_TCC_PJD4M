using UnityEngine;

public class EnemyTakeDamage : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 3;

    [Header("Feedback")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;

    [Header("Loot")]
    public LootItem[] lootTable;

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
        foreach (LootItem lootItem in lootTable)
        {
            if (Random.Range(0f, 100f) <= lootItem.dropChance)
            {
                InstantiateLoot(lootItem.itemPrefab);

                break;
            }
        }

        Destroy(gameObject);
    }

    void InstantiateLoot(GameObject loot)
    {
        if (loot)
        {
            GameObject droppedLoot = Instantiate(loot, transform.position,
            Quaternion.identity);
        }
    }
}