using System.Collections.Generic;
using UnityEngine;
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

    private void OnEnable()
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

    private void EncontrarPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("LifeHUD: Player não encontrado.");
            return;
        }

        healthSystem = playerObject.GetComponent<HealthSystem>();

        if (healthSystem == null)
        {
            Debug.LogWarning("LifeHUD: Player não possui HealthSystem.");
            return;
        }

        AtualizarHUD();
    }

    private void AtualizarHUD()
    {
        if (healthSystem == null)
            return;

        // Se o jogador tiver mais vida máxima do que corações criados no HUD, instancia os novos prefabs
        while (coracoes.Count < healthSystem.vidaMaxima)
        {
            AdicionarCoracaoPrefab();
        }

        int vida = healthSystem.vida;

        for (int i = 0; i < coracoes.Count; i++)
        {
            if (coracoes[i] == null) continue;

            // Se o índice for menor que a vida atual, exibe o sprite cheio; senão, o vazio
            coracoes[i].sprite = (i < vida) ? cheio : vazio;

            // Garante que o coração permaneça ativo/visível na tela (mesmo se vazio)
            coracoes[i].enabled = true;
        }
    }

    /// <summary>
    /// Instancia o prefab do coração no container da GUI e adiciona à lista do HUD.
    /// </summary>
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