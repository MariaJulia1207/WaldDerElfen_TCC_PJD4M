using System.Collections;
using TMPro;
using UnityEngine;

public class IntroTextWriter : MonoBehaviour
{
    [Header("Texto")]
    private TMP_Text _texto;

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
        _texto = GetComponent<TMP_Text>();

        if (_texto == null)
        {
            Debug.LogError("IntroTextWriter: este objeto não possui um TMP_Text/TextMeshProUGUI.");
            enabled = false;
            return;
        }

        _canvasGroup = _texto.GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _canvasGroup = _texto.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void IniciarIntroducao()
    {
        StopAllCoroutines();

        StartCoroutine(ExecutarIntroducao());
    }

    private IEnumerator ExecutarIntroducao()
    {
        _canvasGroup.alpha = 1f;

        foreach (string paragrafo in paragrafos)
        {
            // -----------------------------
            // COMEÇA COM O TEXTO INVISÍVEL
            // -----------------------------

            _canvasGroup.alpha = 1f;
            _texto.text = "";

            // -----------------------------
            // DIGITAÇÃO
            // -----------------------------

            foreach (char letra in paragrafo)
            {
                _texto.text += letra;

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

                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, progresso);

                yield return null;
            }

            _canvasGroup.alpha = 0f;

            // Pequena garantia de que o texto anterior
            // desapareceu antes do próximo começar.
            _texto.text = "";
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