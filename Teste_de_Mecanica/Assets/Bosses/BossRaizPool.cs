using System.Collections.Generic;
using UnityEngine;

public class BossRaizPool : MonoBehaviour
{
    [Header("Prefab da raiz")]
    [SerializeField] private BossRaiz prefabRaiz;

    [Header("Quantidade no Pool")]
    [SerializeField] private int quantidadeNoPool = 6;

    [Header("Pontos possíveis da arena")]
    [SerializeField] private Transform[] pontosSpawn;

    private readonly List<BossRaiz> pool =
        new List<BossRaiz>();

    private void Awake()
    {
        CriarPool();
    }

    private void CriarPool()
    {
        for (int i = 0; i < quantidadeNoPool; i++)
        {
            BossRaiz raiz = Instantiate(
                prefabRaiz,
                transform
            );

            raiz.gameObject.SetActive(false);

            raiz.DefinirPool(this);

            pool.Add(raiz);
        }
    }

    // =========================================================
    // INVOCAR RAÍZES
    // =========================================================

    public void InvocarRaizes(int quantidade)
    {
        int invocadas = 0;

        List<Transform> pontosDisponiveis =
            new List<Transform>(pontosSpawn);

        // Embaralha os pontos
        Embaralhar(pontosDisponiveis);

        foreach (BossRaiz raiz in pool)
        {
            if (invocadas >= quantidade)
                break;

            if (raiz.EstaAtiva())
                continue;

            if (pontosDisponiveis.Count == 0)
                break;

            Transform ponto =
                pontosDisponiveis[invocadas];

            raiz.Ativar(ponto.position);

            invocadas++;
        }
    }

    // =========================================================
    // DEVOLVER PARA O POOL
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

    private void Embaralhar(List<Transform> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            int indiceAleatorio =
                Random.Range(i, lista.Count);

            Transform temporario = lista[i];

            lista[i] = lista[indiceAleatorio];
            lista[indiceAleatorio] = temporario;
        }
    }
}