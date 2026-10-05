using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjetilPool : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private Projetil prefabProjetil;
    [SerializeField] private int quantidadeInicial = 12;

    private readonly Queue<Projetil> pool =
        new Queue<Projetil>();


    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Awake()
    {
        for (int i = 0; i < quantidadeInicial; i++)
        {
            CriarProjetil();
        }
    }


    // =========================================================
    // CRIAR
    // =========================================================

    private Projetil CriarProjetil()
    {
        Projetil projetil =
            Instantiate(prefabProjetil, transform);

        projetil.ConfigurarPool(this);

        projetil.gameObject.SetActive(false);

        pool.Enqueue(projetil);

        return projetil;
    }


    // =========================================================
    // PEGAR DO POOL
    // =========================================================

    public Projetil Pegar()
    {
        if (pool.Count == 0)
        {
            CriarProjetil();
        }

        Projetil projetil = pool.Dequeue();

        projetil.Ativar();

        return projetil;
    }


    // =========================================================
    // DEVOLVER
    // =========================================================

    public void Devolver(Projetil projetil)
    {
        if (projetil == null)
            return;

        projetil.gameObject.SetActive(false);

        pool.Enqueue(projetil);
    }
}