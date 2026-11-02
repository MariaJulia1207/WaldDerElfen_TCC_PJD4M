using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjetilPool : MonoBehaviour
{
    [System.Serializable]
    public struct ElementoPool
    {
        [Tooltip("Identificador único para buscar o projétil por texto (ex: 'FeiticoAlberich', 'FogoGoblin').")]
        public string id;
        public Projetil prefabProjetil;
        public int quantidadeInicial;
    }

    [Header("Configuração dos Projéteis")]
    [SerializeField] private List<ElementoPool> listaProjeteis = new List<ElementoPool>();

    private readonly Dictionary<string, Projetil> prefabsPorId = new Dictionary<string, Projetil>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Projetil, Queue<Projetil>> poolsPorPrefab = new Dictionary<Projetil, Queue<Projetil>>();
    private readonly Dictionary<Projetil, Projetil> mapaInstanciaParaPrefab = new Dictionary<Projetil, Projetil>();

    private void Awake()
    {
        InicializarPools();
    }

    private void InicializarPools()
    {
        foreach (var elemento in listaProjeteis)
        {
            if (elemento.prefabProjetil == null)
                continue;

            // Registra busca por ID
            if (!string.IsNullOrEmpty(elemento.id) && !prefabsPorId.ContainsKey(elemento.id))
            {
                prefabsPorId.Add(elemento.id, elemento.prefabProjetil);
            }

            int quantidade = elemento.quantidadeInicial > 0 ? elemento.quantidadeInicial : 5;
            for (int i = 0; i < quantidade; i++)
            {
                CriarInstancia(elemento.prefabProjetil);
            }
        }
    }

    private Projetil CriarInstancia(Projetil prefab)
    {
        Projetil novaInstancia = Instantiate(prefab, transform);
        novaInstancia.ConfigurarPool(this);
        novaInstancia.gameObject.SetActive(false);

        if (!poolsPorPrefab.ContainsKey(prefab))
        {
            poolsPorPrefab[prefab] = new Queue<Projetil>();
        }

        poolsPorPrefab[prefab].Enqueue(novaInstancia);
        mapaInstanciaParaPrefab[novaInstancia] = prefab;

        return novaInstancia;
    }

    // =========================================================
    // PEGAR PROJÉTIL
    // =========================================================

    /// <summary>
    /// Sorteia e pega um projétil aleatório entre todos os cadastrados na lista.
    /// </summary>
    public Projetil Pegar()
    {
        if (listaProjeteis == null || listaProjeteis.Count == 0)
        {
            Debug.LogWarning("ProjetilPool: Nenhum projétil configurado na lista do Inspector.", this);
            return null;
        }

        int indiceAleatorio = UnityEngine.Random.Range(0, listaProjeteis.Count);
        return Pegar(listaProjeteis[indiceAleatorio].prefabProjetil);
    }

    /// <summary>
    /// Pega um projétil por seu índice na lista do Inspector (0, 1, 2...).
    /// </summary>
    public Projetil Pegar(int index)
    {
        if (listaProjeteis == null || index < 0 || index >= listaProjeteis.Count)
        {
            Debug.LogWarning($"ProjetilPool: Índice {index} fora do alcance da lista.", this);
            return null;
        }

        return Pegar(listaProjeteis[index].prefabProjetil);
    }

    /// <summary>
    /// Pega um projétil passando a referência direta do Prefab.
    /// </summary>
    public Projetil Pegar(Projetil prefab)
    {
        if (prefab == null)
            return null;

        if (!poolsPorPrefab.ContainsKey(prefab) || poolsPorPrefab[prefab].Count == 0)
        {
            CriarInstancia(prefab);
        }

        Projetil projetil = poolsPorPrefab[prefab].Dequeue();
        projetil.Ativar();

        return projetil;
    }

    /// <summary>
    /// Pega um projétil usando a chave de ID cadastrada no Inspector.
    /// </summary>
    public Projetil Pegar(string id)
    {
        if (prefabsPorId.TryGetValue(id, out Projetil prefab))
        {
            return Pegar(prefab);
        }

        Debug.LogWarning($"ProjetilPool: Nenhum projétil com ID '{id}' foi encontrado na pool.", this);
        return null;
    }

    // =========================================================
    // DEVOLVER PROJÉTIL
    // =========================================================

    public void Devolver(Projetil projetil)
    {
        if (projetil == null)
            return;

        projetil.gameObject.SetActive(false);

        if (mapaInstanciaParaPrefab.TryGetValue(projetil, out Projetil prefabOrigem))
        {
            if (!poolsPorPrefab.ContainsKey(prefabOrigem))
            {
                poolsPorPrefab[prefabOrigem] = new Queue<Projetil>();
            }

            poolsPorPrefab[prefabOrigem].Enqueue(projetil);
        }
        else
        {
            Destroy(projetil.gameObject);
        }
    }
}