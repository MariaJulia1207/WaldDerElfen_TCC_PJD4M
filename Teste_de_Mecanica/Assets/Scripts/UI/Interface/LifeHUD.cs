using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LifeHUD : MonoBehaviour
{
    [Header("Configurações dos Corações")]
    [SerializeField] private List<Image> coracoes = new List<Image>();
    [SerializeField] private GameObject coracaoPrefab;
    [SerializeField] private Transform containerCoracoes; // Ex: objeto Pai com Horizontal Layout Group

    [Header("Sprites")]
    [SerializeField] private Sprite cheio;
    [SerializeField] private Sprite vazio;

    private HealthSystem healthSystem;

    private void Awake()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnEnable()
    {
        EncontrarPlayer();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EncontrarPlayer();
    }

    private void Update()
    {
        if (healthSystem == null)
        {
            EncontrarPlayer();
            return;
        }

        AtualizarHUD();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void EncontrarPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        healthSystem = playerObject.GetComponent<HealthSystem>();

        if (healthSystem != null)
        {
            AtualizarHUD();
        }
    }

    private void AtualizarHUD()
    {
        if (healthSystem == null)
            return;

        // Limpa referências destruídas de trocas de cena passadas
        coracoes.RemoveAll(item => item == null);

        // Se o HUD tiver menos corações que a vida máxima, cria os faltantes
        while (coracoes.Count < healthSystem.vidaMaxima)
        {
            AdicionarCoracaoPrefab();
        }

        // Se tiver mais corações que a vida máxima, remove o excesso
        while (coracoes.Count > healthSystem.vidaMaxima)
        {
            int index = coracoes.Count - 1;
            if (coracoes[index] != null)
            {
                Destroy(coracoes[index].gameObject);
            }
            coracoes.RemoveAt(index);
        }

        int vida = healthSystem.vida;

        for (int i = 0; i < coracoes.Count; i++)
        {
            if (coracoes[i] == null) continue;

            coracoes[i].sprite = (i < vida) ? cheio : vazio;
            coracoes[i].enabled = true;
        }
    }

    public void AdicionarCoracaoPrefab()
    {
        if (coracaoPrefab == null)
        {
            Debug.LogWarning("LifeHUD: Prefab de Coração não atribuído no Inspector!");
            return;
        }

        Transform parentTransform = containerCoracoes != null ? containerCoracoes : transform;
        GameObject novoCoracaoObj = Instantiate(coracaoPrefab, parentTransform);
        Image novoCoracaoImg = novoCoracaoObj.GetComponent<Image>();

        if (novoCoracaoImg == null)
        {
            novoCoracaoImg = novoCoracaoObj.GetComponentInChildren<Image>();
        }

        if (novoCoracaoImg != null)
        {
            coracoes.Add(novoCoracaoImg);
        }
        else
        {
            Debug.LogWarning("LifeHUD: O Prefab instanciado não possui um componente Image.");
        }
    }
}