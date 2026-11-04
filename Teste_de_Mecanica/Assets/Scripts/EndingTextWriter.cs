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

        [Tooltip("Se marcado, o sistema faz uma pausa nos textos e chama uma Timeline animada após este parágrafo.")]
        public bool temCutsceneDepois;
    }

    [Header("Texto")]
    [SerializeField] private TMP_Text texto;
    [SerializeField] private ParagrafoEnding[] paragrafos;

    [Header("Digitação")]
    [SerializeField] private float velocidadeDigitacao = 0.03f;

    [Header("Tempo")]
    [SerializeField] private float tempoDepoisDaDigitacao = 3f;
    [SerializeField] private float tempoDesaparecimento = 1.5f;

    [Header("Controle")]
    [SerializeField] private EndingDirector endingDirector;

    [Header("Eventos")]
    public UnityEvent aoFinalizarTodosParagrafos;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (texto == null)
        {
            texto = GetComponent<TMP_Text>();
        }

        if (texto == null)
        {
            Debug.LogError("EndingTextWriter: Nenhum TMP_Text encontrado!");
            enabled = false;
            return;
        }

        _canvasGroup = texto.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = texto.gameObject.AddComponent<CanvasGroup>();
        }

        if (endingDirector == null)
        {
            endingDirector = FindAnyObjectByType<EndingDirector>();
        }
    }

    public void IniciarEscritaFinal()
    {
        if (paragrafos == null || paragrafos.Length == 0)
        {
            Debug.LogWarning("EndingTextWriter: Array de parágrafos está vazio!");
            aoFinalizarTodosParagrafos?.Invoke();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ExecutarEscritaFinal());
    }

    private IEnumerator ExecutarEscritaFinal()
    {
        _canvasGroup.alpha = 1f;
        int indiceCutscene = 0;

        for (int i = 0; i < paragrafos.Length; i++)
        {
            ParagrafoEnding paragrafoAtual = paragrafos[i];

            _canvasGroup.alpha = 1f;
            texto.text = "";

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

            yield return new WaitForSeconds(tempoDepoisDaDigitacao);

            float tempo = 0f;
            while (tempo < tempoDesaparecimento)
            {
                tempo += Time.deltaTime;
                float progresso = tempo / tempoDesaparecimento;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, progresso);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            texto.text = "";

            if (paragrafoAtual.temCutsceneDepois)
            {
                if (endingDirector != null)
                {
                    yield return StartCoroutine(endingDirector.TocarCutsceneIntermediaria(indiceCutscene));
                    indiceCutscene++;
                }
            }
        }

        aoFinalizarTodosParagrafos?.Invoke();
    }
}