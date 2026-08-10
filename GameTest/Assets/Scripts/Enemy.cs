using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int vida = 3;

    public void ReceberDano(int dano)
    {
        vida -= dano;

        Debug.Log("Inimigo recebeu " + dano + " de dano.");

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