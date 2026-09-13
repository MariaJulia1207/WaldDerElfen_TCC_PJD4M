using System.Collections;
using TMPro;
using UnityEngine;

public class IntroTextWriter : MonoBehaviour
{
    [Header("Texto")]
    [SerializeField] private TMP_Text texto;

    [TextArea(5, 20)]
    [SerializeField] private string[] paragrafos;

    [Header("Digitação")]
    [SerializeField] private float velocidadeDigitacao = 0.03f;

    [Header("Tempo")]
    [SerializeField] private float tempoDepoisDaDigitacao = 3f;
    [SerializeField] private float tempoDesaparecimento = 1.5f;

    [Header("Controle")]
    [SerializeField] private IntroDirector introDirector;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = texto.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = texto.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void IniciarIntroducao()
    {
        StopAllCoroutines();

        StartCoroutine(ExecutarIntroducao());
    }

    private IEnumerator ExecutarIntroducao()
    {
        canvasGroup.alpha = 1f;

        foreach (string paragrafo in paragrafos)
        {
            // -----------------------------
            // COMEÇA COM O TEXTO INVISÍVEL
            // -----------------------------

            canvasGroup.alpha = 1f;
            texto.text = "";

            // -----------------------------
            // DIGITAÇÃO
            // -----------------------------

            foreach (char letra in paragrafo)
            {
                texto.text += letra;

                yield return new WaitForSeconds(velocidadeDigitacao);
            }

            // -----------------------------
            // ESPERA APÓS TERMINAR
            // -----------------------------

            yield return new WaitForSeconds(tempoDepoisDaDigitacao);

            // -----------------------------
            // DESAPARECIMENTO
            // -----------------------------

            float tempo = 0f;

            while (tempo < tempoDesaparecimento)
            {
                tempo += Time.deltaTime;

                float progresso = tempo / tempoDesaparecimento;

                canvasGroup.alpha = Mathf.Lerp(1f, 0f, progresso);

                yield return null;
            }

            canvasGroup.alpha = 0f;

            // Pequena garantia de que o texto anterior
            // desapareceu antes do próximo começar.
            texto.text = "";
        }

        // -----------------------------
        // FIM DA INTRODUÇÃO
        // -----------------------------

        if (introDirector != null)
        {
            introDirector.LoadLevel1();
        }
    }
}