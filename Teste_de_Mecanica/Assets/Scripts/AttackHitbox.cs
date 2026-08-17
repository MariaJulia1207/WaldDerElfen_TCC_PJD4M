using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int dano = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Obstaculo obstaculo = other.GetComponent<Obstaculo>();

        if (obstaculo != null)
        {
            obstaculo.ReceberDano(dano);
        }
    }
}