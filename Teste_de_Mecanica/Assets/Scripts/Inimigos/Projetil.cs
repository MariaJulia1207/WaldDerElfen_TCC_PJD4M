using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Projetil : MonoBehaviour
{
    private ProjetilPool pool;

    private Vector2 direcao;
    private float velocidade;

    private bool disparado;

    [Header("Configuração")]
    [SerializeField] private float tempoMaximoDeVida = 5f;

    private float tempoVida;


    // =========================================================
    // CONFIGURAÇÃO
    // =========================================================

    public void ConfigurarPool(ProjetilPool novoPool)
    {
        pool = novoPool;
    }


    // =========================================================
    // ATIVAR PROJÉTIL
    // =========================================================

    public void Ativar()
    {
        gameObject.SetActive(true);

        disparado = false;
        tempoVida = 0f;

        direcao = Vector2.zero;
        velocidade = 0f;
    }


    // =========================================================
    // DISPARAR
    // =========================================================

    public void Disparar(Vector2 novaDirecao, float novaVelocidade)
    {
        direcao = novaDirecao.normalized;
        velocidade = novaVelocidade;

        disparado = true;
        tempoVida = 0f;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!disparado)
            return;

        transform.position +=
            (Vector3)(direcao * velocidade * Time.deltaTime);

        tempoVida += Time.deltaTime;

        if (tempoVida >= tempoMaximoDeVida)
        {
            RetornarAoPool();
        }
    }


    // =========================================================
    // COLISÃO
    // =========================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!disparado)
            return;

        if (!other.CompareTag("Player"))
            return;

        other.SendMessage(
            "ReceberDano",
            1,
            SendMessageOptions.DontRequireReceiver
        );

        RetornarAoPool();
    }


    // =========================================================
    // DEVOLVER AO POOL
    // =========================================================

    public void RetornarAoPool()
    {
        disparado = false;

        if (pool != null)
        {
            pool.Devolver(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}