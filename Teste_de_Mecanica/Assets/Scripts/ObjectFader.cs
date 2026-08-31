using System.Collections;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    [Header("Transparência")]
    [Range(0f, 1f)]
    [SerializeField] private float transparencyValue = 0.7f;

    [SerializeField] private float transparencyFadeTime = 0.4f;

    private SpriteRenderer spriteRenderer;
    private Coroutine fadeCoroutine;

    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Start()
    {
        // Se o script estiver no objeto filho (Trigger_Copa),
        // procura o SpriteRenderer no objeto pai (Tree_Prefab).
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
    }

    // =========================================================
    // ENTRADA DO PLAYER
    // =========================================================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            IniciarFade(transparencyValue);
        }
    }

    // =========================================================
    // SAÍDA DO PLAYER
    // =========================================================

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            IniciarFade(1f);
        }
    }

    // =========================================================
    // FADE
    // =========================================================

    private void IniciarFade(float transparencia)
    {
        if (spriteRenderer == null)
            return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(
            FadeTree(spriteRenderer.color.a, transparencia)
        );
    }

    private IEnumerator FadeTree(float valorInicial, float valorFinal)
    {
        float tempo = 0f;

        Color cor = spriteRenderer.color;

        while (tempo < transparencyFadeTime)
        {
            tempo += Time.deltaTime;

            float alpha = Mathf.Lerp(
                valorInicial,
                valorFinal,
                tempo / transparencyFadeTime
            );

            cor.a = alpha;
            spriteRenderer.color = cor;

            yield return null;
        }

        // Garante que o valor final seja exatamente o desejado.
        cor.a = valorFinal;
        spriteRenderer.color = cor;

        fadeCoroutine = null;
    }
}