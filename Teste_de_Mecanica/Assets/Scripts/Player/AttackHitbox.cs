using System;
using System.Collections.Generic;
using UnityEditor;
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

    private readonly HashSet<BossPapoula> papoulasAtingidas =
        new HashSet<BossPapoula>();

    private readonly HashSet<BossBruxa> bruxasAtingidas =
        new HashSet<BossBruxa>();

    private void Awake()
    {
        meuCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        obstaculosAtingidos.Clear();
        inimigosAtingidos.Clear();
        papoulasAtingidas.Clear();
        bruxasAtingidas.Clear();
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

            EnemyGoblin goblin =
                enemy.GetComponentInParent<EnemyGoblin>();

            if (goblin != null)
            {
                Vector2 direcaoKnockback =
                    (enemy.transform.position - transform.position).normalized;

                goblin.ReceberKnockback(direcaoKnockback);
            }

            return;
        }

        // -----------------------------------------------------
        // PAPOULA DO BOSS
        // -----------------------------------------------------

        BossPapoula papoula =
            other.GetComponentInParent<BossPapoula>();

        if (papoula != null)
        {
            if (papoulasAtingidas.Contains(papoula))
                return;

            papoulasAtingidas.Add(papoula);

            papoula.ReceberDano(dano);

            return;
        }

        // -----------------------------------------------------
        // BRUXA DO BOSS
        // -----------------------------------------------------

        BossBruxa bruxa =
            other.GetComponentInParent<BossBruxa>();

        if (bruxa != null)
        {
            if (bruxasAtingidas.Contains(bruxa))
                return;

            bruxasAtingidas.Add(bruxa);

            bruxa.ReceberDano(dano);

            return;
        }
    }
}