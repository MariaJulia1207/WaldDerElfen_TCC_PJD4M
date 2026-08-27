using System.Collections;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    [Range(0, 1)] 
    [SerializeField] private float _transparencyValue = 0.7f;
    [SerializeField] private float _transparencyFadeTime = 0.4f;
    
    private SpriteRenderer _spriteRenderer;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        // Se o script estiver no objeto filho (Trigger_Copa), 
        // pegamos o SpriteRenderer no pai (Tree_Prefab)
        _spriteRenderer = GetComponentInParent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTree(_spriteRenderer.color.a, _transparencyValue));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTree(_spriteRenderer.color.a, 1f));
        }
    }

    private IEnumerator FadeTree(float startAlpha, float targetAlpha)
    {
        float time = 0;
        Color currentColor = _spriteRenderer.color;

        while (time < _transparencyFadeTime)
        {
            time += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / _transparencyFadeTime);
            _spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        _spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }
}
