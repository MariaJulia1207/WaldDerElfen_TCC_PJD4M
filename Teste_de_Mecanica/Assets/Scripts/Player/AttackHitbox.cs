using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("Dano")]
    [SerializeField] private int dano = 1;

    private Collider2D meuCollider;

    private readonly HashSet<Obstaculo> obstaculosAtingidos = new HashSet<Obstaculo>();
    private readonly HashSet<EnemyTakeDamage> inimigosAtingidos = new HashSet<EnemyTakeDamage>();
    private readonly HashSet<BossPapoula> papoulasAtingidas = new HashSet<BossPapoula>();
    private readonly HashSet<BossBruxa> bruxasAtingidas = new HashSet<BossBruxa>();
    private readonly HashSet<BossAlberich> alberichsAtingidos = new HashSet<BossAlberich>();
    private readonly HashSet<ProjetilAlberich> projeteisRebatidos = new HashSet<ProjetilAlberich>();

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
        alberichsAtingidos.Clear();
        projeteisRebatidos.Clear();
    }

    public void VerificarAcerto()
    {
        if (meuCollider == null)
            return;

        ContactFilter2D filtro = ContactFilter2D.noFilter;
        Collider2D[] resultados = new Collider2D[20];

        int quantidade = Physics2D.OverlapCollider(meuCollider, filtro, resultados);

        for (int i = 0; i < quantidade; i++)
        {
            if (resultados[i] == null)
                continue;

            CausarDano(resultados[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CausarDano(other);
    }

    private void CausarDano(Collider2D other)
    {
        // -----------------------------------------------------
        // FEITIÇO DO ALBERICH (REBATIMENTO)
        // -----------------------------------------------------
        ProjetilAlberich feitico = other.GetComponentInParent<ProjetilAlberich>();
        if (feitico != null && feitico.PodeSerRefletido())
        {
            if (!projeteisRebatidos.Contains(feitico))
            {
                projeteisRebatidos.Add(feitico);
                Vector2 direcaoRebatida = (feitico.transform.position - transform.position).normalized;
                feitico.Refletir(direcaoRebatida);
            }
            return;
        }

        // -----------------------------------------------------
        // OBSTÁCULO
        // -----------------------------------------------------
        Obstaculo obstaculo = other.GetComponentInParent<Obstaculo>();
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
        EnemyTakeDamage enemy = other.GetComponentInParent<EnemyTakeDamage>();
        if (enemy != null)
        {
            if (inimigosAtingidos.Contains(enemy))
                return;

            inimigosAtingidos.Add(enemy);
            enemy.ReceberDano(dano);

            EnemyGoblin goblin = enemy.GetComponentInParent<EnemyGoblin>();
            if (goblin != null)
            {
                Vector2 direcaoKnockback = (enemy.transform.position - transform.position).normalized;
                goblin.ReceberKnockback(direcaoKnockback);
            }

            EnemyGriffin griffin = enemy.GetComponentInParent<EnemyGriffin>();
            if (griffin != null)
            {
                Vector2 direcaoKnockback = (enemy.transform.position - transform.position).normalized;
                griffin.ReceberKnockback(direcaoKnockback);
            }

            return;
        }

        // -----------------------------------------------------
        // BOSS PAPOULA
        // -----------------------------------------------------
        BossPapoula papoula = other.GetComponentInParent<BossPapoula>();
        if (papoula != null)
        {
            if (papoulasAtingidas.Contains(papoula))
                return;

            papoulasAtingidas.Add(papoula);
            papoula.ReceberDano(dano);
            return;
        }

        // -----------------------------------------------------
        // BOSS BRUXA
        // -----------------------------------------------------
        BossBruxa bruxa = other.GetComponentInParent<BossBruxa>();
        if (bruxa != null)
        {
            if (bruxasAtingidas.Contains(bruxa))
                return;

            bruxasAtingidas.Add(bruxa);
            bruxa.ReceberDano(dano);
            return;
        }

        // -----------------------------------------------------
        // BOSS ALBERICH
        // -----------------------------------------------------
        BossAlberich alberich = other.GetComponentInParent<BossAlberich>();
        if (alberich != null)
        {
            if (alberichsAtingidos.Contains(alberich))
                return;

            alberichsAtingidos.Add(alberich);
            alberich.ReceberDanoAtaqueDireto(dano);
            return;
        }
    }
}