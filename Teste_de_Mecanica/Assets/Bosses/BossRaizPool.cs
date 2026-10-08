using System.Collections.Generic;
using UnityEngine;

public class BossRaizPool : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private BossRaiz prefabRaiz;

    [Header("Pool")]
    [SerializeField] private int quantidadeNoPool = 6;

    [Header("Pontos da Arena")]
    [SerializeField] private Transform[] pontosSpawn;

    private readonly List<BossRaiz> pool =
        new List<BossRaiz>();

    private void Awake()
    {
        CriarPool();
    }

    // =========================================================
    // CRIAR POOL
    // =========================================================

    private void CriarPool()
    {
        if (prefabRaiz == null)
        {
            Debug.LogError(
                "BossRaizPool: prefab da raiz não foi atribuído.",
                this
            );

            return;
        }

        for (int i = 0; i < quantidadeNoPool; i++)
        {
            BossRaiz raiz =
                Instantiate(
                    prefabRaiz,
                    transform
                );

            raiz.gameObject.SetActive(false);

            raiz.DefinirPool(this);

            pool.Add(raiz);
        }
    }

    // =========================================================
    // INVOCAR
    // =========================================================

    public void InvocarRaizes(int quantidade)
    {
        if (pontosSpawn == null ||
            pontosSpawn.Length == 0)
        {
            Debug.LogWarning(
                "BossRaizPool: nenhum ponto de spawn foi configurado.",
                this
            );

            return;
        }

        List<Transform> pontosDisponiveis =
            new List<Transform>();

        for (int i = 0; i < pontosSpawn.Length; i++)
        {
            if (pontosSpawn[i] != null)
                pontosDisponiveis.Add(
                    pontosSpawn[i]
                );
        }

        Embaralhar(pontosDisponiveis);

        int invocadas = 0;

        foreach (BossRaiz raiz in pool)
        {
            if (invocadas >= quantidade)
                break;

            if (raiz.EstaAtiva())
                continue;

            if (invocadas >= pontosDisponiveis.Count)
                break;

            Transform ponto =
                pontosDisponiveis[invocadas];

            raiz.Ativar(
                ponto.position
            );

            invocadas++;
        }

        Debug.Log(
            "BossRaizPool: " +
            invocadas +
            " raízes ativadas."
        );
    }

    // =========================================================
    // DEVOLVER
    // =========================================================

    public void DevolverRaiz(BossRaiz raiz)
    {
        if (raiz == null)
            return;

        raiz.gameObject.SetActive(false);
    }

    // =========================================================
    // EMBARALHAR
    // =========================================================

    private void Embaralhar(
        List<Transform> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            int indice =
                Random.Range(
                    i,
                    lista.Count
                );

            Transform temporario =
                lista[i];

            lista[i] =
                lista[indice];

            lista[indice] =
                temporario;
        }
    }
}