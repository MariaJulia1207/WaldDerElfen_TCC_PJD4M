using UnityEngine;

public class TriggerDamage : MonoBehaviour
{
    [SerializeField] private HealthSystem heart;

    [SerializeField] private float knockbackForce = 5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Dano
            heart.ReceberDano(1);

            // Knockback
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                Vector2 direcao = (collision.transform.position - transform.position).normalized;

                playerRb.AddForce(direcao * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDamage : MonoBehaviour
{
    public HealthSystem heart;
        
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            heart.vida--;
        }
    }
}
*/