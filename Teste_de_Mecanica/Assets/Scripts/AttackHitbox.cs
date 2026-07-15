using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int dano = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if(enemy != null)
        {
            enemy.ReceberDano(dano);
        }
    }
}