using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CetroAlberich : MonoBehaviour
{
    [Header("Tempos de Carga")]
    [Tooltip("Tempo que o cetro fica girando na mão do boss antes de disparar.")]
    [SerializeField] private float tempoCargaNaMao = 1.0f;
    [Tooltip("Tempo limite de voo de segurança (caso não atinja nenhuma colisão).")]
    [SerializeField] private float tempoVidaMaximoVoo = 6f;

    [Header("Velocidades")]
    [SerializeField] private float velocidadeIda = 10f;
    [SerializeField] private float velocidadeVolta = 12f;

    [Header("Dano")]
    [SerializeField] private int danoNoJogador = 1;

    [Header("Colisão e Paredes")]
    [Tooltip("Colliders de parede específicos da arena (opcional).")]
    [SerializeField] private List<Collider2D> paredesArena = new List<Collider2D>();

    [Header("Áudio")]
    [SerializeField] private string nomeSomLancamento = "ScepterLaunch";

    private Transform pontoMaoBoss;
    private Transform targetJogador;
    private Action onRetornoConcluido;
    private bool emVoo;
    private bool bateuNoAlvo;

    public void IniciarAtaque(Transform pontoMao, Transform jogador, Action callbackConcluido)
    {
        pontoMaoBoss = pontoMao;
        targetJogador = jogador;
        onRetornoConcluido = callbackConcluido;
        bateuNoAlvo = false;
        emVoo = false;

        if (pontoMaoBoss != null)
        {
            transform.position = pontoMaoBoss.position;
        }

        gameObject.SetActive(true);
        StartCoroutine(RotinaAtaqueCetro());
    }

    private IEnumerator RotinaAtaqueCetro()
    {
        // 1. Carga inicial flutuando no ponto da mão
        float timer = 0f;
        while (timer < tempoCargaNaMao)
        {
            timer += Time.deltaTime;
            if (pontoMaoBoss != null)
            {
                transform.position = pontoMaoBoss.position;
            }
            yield return null;
        }

        // 2. Calcula a direção até o jogador no instante do lançamento
        Vector3 direcao = Vector3.down;
        if (targetJogador != null)
        {
            direcao = (targetJogador.position - transform.position).normalized;
        }

        // Toca o efeito sonoro do lançamento via SoundEffectManager
        SoundEffectManager.Play(nomeSomLancamento);

        emVoo = true;
        bateuNoAlvo = false;
        float tempoVoo = 0f;

        // 3. Voo contínuo até colidir com parede/jogador ou estourar o tempo limite
        while (!bateuNoAlvo && tempoVoo < tempoVidaMaximoVoo)
        {
            tempoVoo += Time.deltaTime;
            transform.position += direcao * (velocidadeIda * Time.deltaTime);
            yield return null;
        }

        emVoo = false;

        // 4. Retorno para a mão do Alberich
        while (pontoMaoBoss != null && Vector3.Distance(transform.position, pontoMaoBoss.position) > 0.15f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pontoMaoBoss.position, velocidadeVolta * Time.deltaTime);
            yield return null;
        }

        // 5. Finalização
        if (pontoMaoBoss != null)
        {
            transform.position = pontoMaoBoss.position;
        }

        gameObject.SetActive(false);
        onRetornoConcluido?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!emVoo || bateuNoAlvo) return;

        // Colisão com o Jogador
        if (other.CompareTag("Player"))
        {
            other.SendMessage("ReceberDano", danoNoJogador, SendMessageOptions.DontRequireReceiver);
            bateuNoAlvo = true;
            return;
        }

        // Colisão com Paredes ou Obstáculos da Arena
        if (other.CompareTag("Parede") || other.CompareTag("Obstaculo") || paredesArena.Contains(other))
        {
            bateuNoAlvo = true;
        }
    }
}