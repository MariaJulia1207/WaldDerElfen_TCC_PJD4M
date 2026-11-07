using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EndingTextWriter : MonoBehaviour
{
    [System.Serializable]
    public struct ParagrafoEnding
    {
        [TextArea(3, 10)]
        public string texto;

        [Tooltip("Se marcado, a moldura faz fade out (alpha = 0) e executa a cutscene animada antes do próximo parágrafo.")]
        public bool temCutsceneDepois;
    }

    [Header("Componentes de UI")]
    [SerializeField] private TMP_Text texto;
    [Tooltip("CanvasGroup da Moldura contendo o painel de fundo e o texto.")]
    [SerializeField] private CanvasGroup molduraCanvasGroup;

    [Header("Parágrafos")]
    [SerializeField] private ParagrafoEnding[] paragrafos;

    [Header("Configurações de Digitação")]
    [SerializeField] private float velocidadeDigitacao = 0.03f;
    [SerializeField] private float tempoDepoisDaDigitacao = 3f;

    [Header("Transição da Moldura")]
    [Tooltip("Tempo em segundos para o Fade In e Fade Out da moldura.")]
    [SerializeField] private float tempoFadeMoldura = 1.0f;

    [Header("Controle")]
    [SerializeField] private EndingDirector endingDirector;

    [Header("Eventos")]
    public UnityEvent aoFinalizarTodosParagrafos;

    private void Awake()
    {
        if (texto == null)
        {
            texto = GetComponentInChildren<TMP_Text>();
        }

        if (molduraCanvasGroup == null)
        {
            molduraCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (molduraCanvasGroup == null)
        {
            Debug.LogError("EndingTextWriter: Nenhum CanvasGroup atribuído para a Moldura!", this);
            enabled = false;
            return;
        }

        if (endingDirector == null)
        {
            endingDirector = FindAnyObjectByType<EndingDirector>();
        }

        // Moldura começa totalmente visível (alpha = 1) na inicialização da cena
        molduraCanvasGroup.alpha = 1f;
    }

    public void IniciarEscritaFinal()
    {
        if (paragrafos == null || paragrafos.Length == 0)
        {
            Debug.LogWarning("EndingTextWriter: Array de parágrafos vazio!");
            molduraCanvasGroup.alpha = 0f;
            aoFinalizarTodosParagrafos?.Invoke();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ExecutarEscritaFinal());
    }

    private IEnumerator ExecutarEscritaFinal()
    {
        int indiceCutscene = 0;

        // Garante alpha = 1 no primeiro frame da escrita
        molduraCanvasGroup.alpha = 1f;

        for (int i = 0; i < paragrafos.Length; i++)
        {
            ParagrafoEnding paragrafoAtual = paragrafos[i];
            bool ehUltimoParagrafo = (i == paragrafos.Length - 1);

            // Se o alpha estiver em 0 (após voltar de uma cutscene), faz Fade In da moldura
            if (molduraCanvasGroup.alpha < 1f)
            {
                yield return StartCoroutine(FadeCanvasGroup(molduraCanvasGroup, 0f, 1f, tempoFadeMoldura));
            }

            texto.text = "";

            // 1. Efeito de Digitação
            foreach (char letra in paragrafoAtual.texto)
            {
                texto.text += letra;

                if (!char.IsWhiteSpace(letra))
                {
                    try
                    {
                        SoundEffectManager.Play("Typing");
                    }
                    catch { }
                }

                yield return new WaitForSeconds(velocidadeDigitacao);
            }

            // 2. Tempo de Leitura
            yield return new WaitForSeconds(tempoDepoisDaDigitacao);

            // 3. Checagem de Transição
            if (paragrafoAtual.temCutsceneDepois || ehUltimoParagrafo)
            {
                // Se for seguido de Timeline OU for o último elemento da lista, faz Fade Out até alpha = 0
                yield return StartCoroutine(FadeCanvasGroup(molduraCanvasGroup, 1f, 0f, tempoFadeMoldura));
                texto.text = "";

                if (paragrafoAtual.temCutsceneDepois)
                {
                    if (endingDirector != null)
                    {
                        yield return StartCoroutine(endingDirector.TocarCutsceneIntermediaria(indiceCutscene));
                        indiceCutscene++;
                    }
                    else
                    {
                        Debug.LogWarning("EndingTextWriter: 'temCutsceneDepois' ativado, mas o EndingDirector não foi atribuído!");
                    }
                }
            }
            else
            {
                // Se o elemento seguinte for apenas outro texto, a moldura continua visível (alpha = 1)
                texto.text = "";
            }
        }

        // Garante alpha 0 ao término total da sequência
        molduraCanvasGroup.alpha = 0f;
        aoFinalizarTodosParagrafos?.Invoke();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }

        cg.alpha = endAlpha;
    }
}