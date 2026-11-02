using System;
using System.Collections;
using UnityEngine;

public class CetroAlberich : MonoBehaviour
{
    [Header("Tempos de Ataque")]
    [Tooltip("Tempo que o cetro fica girando na mão do boss antes de disparar.")]
    [SerializeField] private float tempoCargaNaMao = 1.2f;
    [Tooltip("Tempo que o cetro fica girando no ponto final antes de voltar.")]
    [SerializeField] private float tempoPausaNoDestino = 0.6f;

    [Header("Velocidades")]
    [SerializeField] private float velocidadeIda = 10f;
    [SerializeField] private float velocidadeVolta = 12f;

    [Header("Dano")]
    [SerializeField] private int danoNoJogador = 1;

    private Transform pontoMaoBoss;
    private Transform targetJogador;
    private Action onRetornoConcluido;
    private bool atacando;

    public void IniciarAtaque(Transform pontoMao, Transform jogador, Action callbackConcluido)
    {
        pontoMaoBoss = pontoMao;
        targetJogador = jogador;
        onRetornoConcluido = callbackConcluido;

        if (pontoMaoBoss != null)
        {
            transform.position = pontoMaoBoss.position;
        }

        gameObject.SetActive(true);
        StartCoroutine(RotinaAtaqueCetro());
    }

    private IEnumerator RotinaAtaqueCetro()
    {
        atacando = true;

        // 1. Fica no ponto da mão pelo tempo de carga (X segundos)
        float timerCarga = 0f;
        while (timerCarga < tempoCargaNaMao)
        {
            timerCarga += Time.deltaTime;
            if (pontoMaoBoss != null)
            {
                transform.position = pontoMaoBoss.position;
            }
            yield return null;
        }

        // 2. Trava o ponto de destino no jogador no momento do lançamento
        Vector3 destino = targetJogador != null ? targetJogador.position : transform.position + Vector3.down * 3f;

        // 3. Move até o ponto do jogador
        while (Vector3.Distance(transform.position, destino) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidadeIda * Time.deltaTime);
            yield return null;
        }

        // 4. Aguarda X segundos no ponto de destino
        yield return new WaitForSeconds(tempoPausaNoDestino);

        // 5. Retorna para o ponto da mão do Alberich
        while (pontoMaoBoss != null && Vector3.Distance(transform.position, pontoMaoBoss.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pontoMaoBoss.position, velocidadeVolta * Time.deltaTime);
            yield return null;
        }

        // 6. Garante posição final na mão e finaliza o ciclo
        if (pontoMaoBoss != null)
        {
            transform.position = pontoMaoBoss.position;
        }

        atacando = false;
        gameObject.SetActive(false);

        // Notifica o BossAlberich que o ataque terminou
        onRetornoConcluido?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!atacando) return;

        if (other.CompareTag("Player"))
        {
            other.SendMessage("ReceberDano", danoNoJogador, SendMessageOptions.DontRequireReceiver);
        }
    }
}