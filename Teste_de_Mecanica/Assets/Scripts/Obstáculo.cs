using UnityEngine;

public class Obstaculo : MonoBehaviour
{
    [SerializeField] private int vida = 3;

    public void ReceberDano(int dano)
    {
        vida -= dano;

        Debug.Log("Obstáculo recebeu " + dano + " de dano.");

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