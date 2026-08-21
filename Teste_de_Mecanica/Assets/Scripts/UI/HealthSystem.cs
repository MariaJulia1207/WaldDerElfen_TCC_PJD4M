using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    private PlayerController player;
    private SpriteRenderer sprite;

    [Header("Vida")]
    public bool isDead;
    public int vida;
    public int vidaMaxima;

    [Header("HUD")]
    public Image[] coracao;
    public Sprite cheio;
    public Sprite vazio;

    [Header("Flash de Dano")]
    [SerializeField] private float tempoFlash = 0.15f;

    [Header("Morte")]
    [SerializeField] private float tempoAnimacaoMorte = 1.0f;

    [Header("Efeito Branco")]
    [SerializeField] private Image efeitoBranco;
    [SerializeField] private float tempoEfeitoBranco = 0.5f;

    private Color corOriginal;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        sprite = GetComponent<SpriteRenderer>();

        if (sprite != null)
        {
            corOriginal = sprite.color;
        }

        isDead = false;

        // Garante que o efeito branco começa invisível
        if (efeitoBranco != null)
        {
            Color cor = efeitoBranco.color;
            cor.a = 0f;
            efeitoBranco.color = cor;
        }
    }

    private void Update()
    {
        HealthLogic();

        if (vida <= 0 && !isDead)
        {
            DeadState();
        }
    }

    // =========================================================
    // VIDA / HUD
    // =========================================================

    private void HealthLogic()
    {
        if (vida > vidaMaxima)
        {
            vida = vidaMaxima;
        }

        for (int i = 0; i < coracao.Length; i++)
        {
            if (i < vida)
            {
                coracao[i].sprite = cheio;
            }
            else
            {
                coracao[i].sprite = vazio;
            }

            if (i < vidaMaxima)
            {
                coracao[i].enabled = true;
            }
            else
            {
                coracao[i].enabled = false;
            }
        }
    }

    // =========================================================
    // RECEBER DANO
    // =========================================================

    public void ReceberDano(int dano)
    {
        if (isDead)
            return;

        vida -= dano;

        if (vida < 0)
        {
            vida = 0;
        }

        StartCoroutine(FlashVermelho());

        if (vida <= 0)
        {
            DeadState();
        }
    }

    // =========================================================
    // FLASH VERMELHO
    // =========================================================

    private IEnumerator FlashVermelho()
    {
        if (sprite == null)
            yield break;

        sprite.color = Color.red;

        yield return new WaitForSeconds(tempoFlash);

        sprite.color = corOriginal;
    }

    // =========================================================
    // MORTE
    // =========================================================

    private void DeadState()
    {
        if (isDead)
            return;

        isDead = true;

        // Para o jogador
        if (player != null)
        {
            player.PararJogador();
            player.enabled = false;
        }

        // Ativa animação de morte
        if (player != null && player.anim != null)
        {
            player.anim.SetBool("IsDead", true);
        }

        // Desliga colisão
        Collider2D collider = GetComponent<Collider2D>();

        if (collider != null)
        {
            collider.enabled = false;
        }

        StartCoroutine(SequenciaMorte());
    }

    // =========================================================
    // SEQUÊNCIA DA MORTE
    // =========================================================

    private IEnumerator SequenciaMorte()
    {
        // Espera a animação de morte
        yield return new WaitForSeconds(tempoAnimacaoMorte);

        // Faz o cenário/tela ficar branco
        yield return StartCoroutine(EfeitoBranco());

        // Desaparece
        Destroy(gameObject);
    }

    // =========================================================
    // EFEITO BRANCO
    // =========================================================

    private IEnumerator EfeitoBranco()
    {
        if (efeitoBranco == null)
            yield break;

        float tempo = 0f;

        Color cor = efeitoBranco.color;
        cor.a = 0f;
        efeitoBranco.color = cor;

        while (tempo < tempoEfeitoBranco)
        {
            tempo += Time.deltaTime;

            float porcentagem = tempo / tempoEfeitoBranco;

            cor.a = Mathf.Lerp(0f, 1f, porcentagem);
            efeitoBranco.color = cor;

            yield return null;
        }

        cor.a = 1f;
        efeitoBranco.color = cor;
    }
}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    PlayerController player;
    public bool isDead;
    public int vida;
    public int vidaMaxima;

    public Image[] coracao;
    public Sprite cheio;
    public Sprite vazio;

    private SpriteRenderer sprite;
    private Color corOriginal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<PlayerController>();
        private SpriteRenderer sprite;
        private Color corOriginal;
    }

    // Update is called once per frame
    void Update()
    {
        HeslthLogic();
        DeadState();
    }

    void HeslthLogic()
    {
        if(vida > vidaMaxima)
        {
            vida = vidaMaxima;
        }

        for (int i = 0; i < coracao.Length; i++)
        {
            if(i < vida)
            {
                coracao[i].sprite = cheio;
            }
            else
            {
                coracao[i].sprite = vazio;
            }

            if (i < vidaMaxima)
            {
                coracao[i].enabled = true;
            }
            else
            {
                coracao[i].enabled = false;
            }
        }
    }

    IEnumerator FlashVermelho()
    {
        sprite.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        sprite.color = corOriginal;
    }

    void DeadState()
    {
        isDead = true;
        player.anim.SetBool("IsDead", isDead);
        if(vida <= 0)
        {
            player.enabled = false;
            Destroy(gameObject, 1.0f);
        }
    }
}
*/