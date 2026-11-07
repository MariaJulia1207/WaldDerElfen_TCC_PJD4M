using System.Collections;
using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private RectTransform rectTransformConteudo;
    [SerializeField] private RectTransform rectTransformCanvas;

    [Header("Configurações de Rolagem")]
    [Tooltip("Velocidade do movimento vertical dos créditos.")]
    [SerializeField] private float velocidade = 80f;

    [Tooltip("Distância extra em pixels para ajustar onde o último texto para na tela.")]
    [SerializeField] private float offsetParadaFinal = 150f;

    public bool TerminouRolagem { get; private set; }

    private float _posYInicial;
    private float _posYFinal;
    private Coroutine _corrotinaRolagem;

    private void Awake()
    {
        if (rectTransformConteudo == null)
        {
            rectTransformConteudo = GetComponent<RectTransform>();
        }

        if (rectTransformCanvas == null)
        {
            Canvas canvasPai = GetComponentInParent<Canvas>();
            if (canvasPai != null)
            {
                rectTransformCanvas = canvasPai.GetComponent<RectTransform>();
            }
        }
    }

    public void IniciarRolagem()
    {
        TerminouRolagem = false;

        // Força o Layout a recalcular o tamanho correto do Vertical Layout Group no primeiro frame
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransformConteudo);

        float alturaCanvas = rectTransformCanvas != null ? rectTransformCanvas.rect.height : Screen.height;
        float alturaConteudo = rectTransformConteudo.rect.height;

        // Posição inicial: Todo o container abaixo da borda inferior da tela
        _posYInicial = -(alturaCanvas / 2f + alturaConteudo / 2f);

        // Posição final: O fundo do container atinge o centro/borda da tela e para
        _posYFinal = (alturaConteudo / 2f) - (alturaCanvas / 2f) + offsetParadaFinal;

        // Posiciona no ponto de partida
        rectTransformConteudo.anchoredPosition = new Vector2(rectTransformConteudo.anchoredPosition.x, _posYInicial);

        if (_corrotinaRolagem != null)
        {
            StopCoroutine(_corrotinaRolagem);
        }

        _corrotinaRolagem = StartCoroutine(ExecutarRolagem());
    }

    private IEnumerator ExecutarRolagem()
    {
        Vector2 pos = rectTransformConteudo.anchoredPosition;

        while (pos.y < _posYFinal)
        {
            pos.y += velocidade * Time.deltaTime;

            if (pos.y >= _posYFinal)
            {
                pos.y = _posYFinal;
                rectTransformConteudo.anchoredPosition = pos;
                break;
            }

            rectTransformConteudo.anchoredPosition = pos;
            yield return null;
        }

        TerminouRolagem = true;
    }
}