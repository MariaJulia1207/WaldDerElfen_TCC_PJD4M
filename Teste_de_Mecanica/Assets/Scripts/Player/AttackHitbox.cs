using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("Dano")]
    [SerializeField] private int dano = 1;

    private Collider2D meuCollider;

    private readonly HashSet<Obstaculo> obstaculosAtingidos =
        new HashSet<Obstaculo>();

    private readonly HashSet<EnemyTakeDamage> inimigosAtingidos =
        new HashSet<EnemyTakeDamage>();

    private void Awake()
    {
        meuCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        obstaculosAtingidos.Clear();
        inimigosAtingidos.Clear();
    }

    // =========================================================
    // VERIFICAÇÃO IMEDIATA
    // =========================================================

    public void VerificarAcerto()
    {
        if (meuCollider == null)
            return;

        ContactFilter2D filtro = ContactFilter2D.noFilter;

        Collider2D[] resultados = new Collider2D[20];

        int quantidade = Physics2D.OverlapCollider(
            meuCollider,
            filtro,
            resultados
        );

        for (int i = 0; i < quantidade; i++)
        {
            if (resultados[i] == null)
                continue;

            CausarDano(resultados[i]);
        }
    }

    // =========================================================
    // ACERTO POR TRIGGER
    // =========================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        CausarDano(other);
    }

    // =========================================================
    // CAUSAR DANO
    // =========================================================

    private void CausarDano(Collider2D other)
    {
        // -----------------------------------------------------
        // OBSTÁCULO
        // -----------------------------------------------------

        Obstaculo obstaculo =
            other.GetComponentInParent<Obstaculo>();

        if (obstaculo != null)
        {
            // Já recebeu dano neste ataque?
            if (obstaculosAtingidos.Contains(obstaculo))
                return;

            obstaculosAtingidos.Add(obstaculo);

            obstaculo.ReceberDano(dano);

            return;
        }

        // -----------------------------------------------------
        // INIMIGO
        // -----------------------------------------------------

        EnemyTakeDamage enemy = 
            other.GetComponentInParent<EnemyTakeDamage>();

        if (enemy != null)
        {
            if (inimigosAtingidos.Contains(enemy))
                return;
            inimigosAtingidos.Add(enemy);
            enemy.ReceberDano(dano);

            EnemyMoblin moblin = enemy.GetComponent<EnemyMoblin>();

            if (moblin != null)
            {
                Vector2 direcaoKnockback = 
                    (enemy.transform.position - transform.position).normalized;
                moblin.ReceberKnockback(direcaoKnockback);
            }

            return;
        }
    }
}