using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [Header("Vida")]
    public bool isDead;
    public int vida;
    public int vidaMaxima;

    [Header("HUD")]
    public Image[] coracao;
    public Sprite cheio;
    public Sprite vazio;

    [Header("Visual do Player")]
    [SerializeField] private SpriteRenderer sprite;

    private PlayerController player;
    private Color corOriginal;

    private void Start()
    {
        player = GetComponent<PlayerController>();

        if (sprite == null)
        {
            sprite = GetComponent<SpriteRenderer>();
        }

        if (sprite != null)
        {
            corOriginal = sprite.color;
        }
    }

    private void Update()
    {
        HealthLogic();
        DeadState();
    }

    // =========================================================
    // VIDA / HUD
    // =========================================================

    void HealthLogic()
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
    }

    // =========================================================
    // FLASH VERMELHO
    // =========================================================

    IEnumerator FlashVermelho()
    {
        if (sprite == null)
            yield break;

        sprite.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        sprite.color = corOriginal;
    }

    // =========================================================
    // MORTE
    // =========================================================

    private void DeadState()
    {
        if (vida <= 0 && !isDead)
        {
            isDead = true;

            // Garante que a animação de morte
            // comece com a cor normal
            if (sprite != null)
            {
                sprite.color = corOriginal;
            }

            // Para o PlayerController
            if (player != null)
            {
                player.enabled = false;
            }

            // Para o Rigidbody
            Rigidbody2D rb = GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Inicia animação de morte
            if (player != null && player.Anim != null)
            {
                player.Anim.SetBool("IsDead", true);
            }
        }
    }

    // =========================================================
    // DESTRUIR PLAYER
    // =========================================================

    public void DestroyGameObject()
    {
        Destroy(gameObject);
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