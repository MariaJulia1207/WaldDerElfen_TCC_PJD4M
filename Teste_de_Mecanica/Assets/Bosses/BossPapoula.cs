using UnityEngine;

public class BossPapoula : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] private BossArvore boss;

    [Header("Estado")]
    [SerializeField] private bool aberta = false;

    private Collider2D colisor;

    private void Start()
    {
        colisor = GetComponent<Collider2D>();
    }

    public bool EstaAberta()
    {
        return aberta;
    }

    // Chamar através de Animation Event
    public void AbrirPapoula()
    {
        aberta = true;

        if (colisor != null)
            colisor.enabled = true;

        Debug.Log("Papoula aberta!");
    }

    // Chamar através de Animation Event
    public void FecharPapoula()
    {
        aberta = false;

        if (colisor != null)
            colisor.enabled = false;

        Debug.Log("Papoula fechada!");
    }

    public void Desativar()
    {
        aberta = false;

        if (colisor != null)
            colisor.enabled = false;
    }

    // Chamado pelo sistema de ataque do jogador
    public void ReceberDano(int dano)
    {
        if (!aberta)
            return;

        if (boss == null)
            return;

        boss.ReceberDano(dano);
    }
}