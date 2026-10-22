using System;
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

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (texto == null)
        {
            texto = GetComponent<TMP_Text>();
        }

        if (texto == null)
        {
            Debug.LogError("IntroTextWriter: Nenhum componente TMP_Text foi atribuído ou encontrado no objeto!");
            enabled = false;
            return;
        }

        _canvasGroup = texto.GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _canvasGroup = texto.gameObject.AddComponent<CanvasGroup>();
        }

        if (introDirector == null)
        {
            introDirector = FindAnyObjectByType<IntroDirector>();
        }
    }

    public void IniciarIntroducao()
    {
        if (paragrafos == null || paragrafos.Length == 0)
        {
            Debug.LogWarning("IntroTextWriter: O array de parágrafos está vazio!");
            if (introDirector != null) introDirector.LoadLevel1();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ExecutarIntroducao());
    }

    private IEnumerator ExecutarIntroducao()
    {
        _canvasGroup.alpha = 1f;

        foreach (string paragrafo in paragrafos)
        {
            _canvasGroup.alpha = 1f;
            texto.text = "";

            foreach (char letra in paragrafo)
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
        }

        if (introDirector != null)
        {
            introDirector.LoadLevel1();
        }
    }
}