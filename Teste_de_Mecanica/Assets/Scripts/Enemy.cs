using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int vida = 3;

    public void ReceberDano(int dano)
    {
        vida -= dano;

        if(vida <= 0)
        {
            Destroy(gameObject);
        }
    }
}