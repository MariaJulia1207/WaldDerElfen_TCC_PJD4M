/*
using System.Collections;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    [Range(0, 1)] 
    [SerializeField] private float _transparencyValue = 0.7f;
    [SerializeField] private float _transparencyFadeTime = 0.4f;
    
    private SpriteRenderer _spriteRenderer;
    private Coroutine _fadeCoroutine;

    void Start()
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
}
*/