using System.Collections;
using UnityEngine;

public class BounceEffect : MonoBehaviour
{
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceDuration = 0.4f;
    [SerializeField] private int bounceCount = 1;

    private Coroutine bounceCoroutine;

    public void StartBounce()
    {
        // Evita iniciar vários bounces ao mesmo tempo
        if (bounceCoroutine != null)
            StopCoroutine(bounceCoroutine);

        bounceCoroutine = StartCoroutine(BounceHandler());
    }

    private IEnumerator BounceHandler()
    {
        Vector3 startPosition = transform.position;

        float currentHeight = bounceHeight;
        float currentDuration = bounceDuration;

        for (int i = 0; i < bounceCount; i++)
        {
            yield return Bounce(
                transform,
                startPosition,
                currentHeight,
                currentDuration
            );

            // Próximo bounce fica menor
            currentHeight *= 0.5f;
            currentDuration *= 0.8f;
        }

        // Garante que volte exatamente ao lugar original
        transform.position = startPosition;

        bounceCoroutine = null;
    }

    private IEnumerator Bounce(
        Transform objectTransform,
        Vector3 start,
        float height,
        float duration)
    {
        Vector3 peak = start + Vector3.up * height;

        float elapsed = 0f;

        // SUBIDA
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / (duration / 2f));

            objectTransform.position = Vector3.Lerp(start, peak, t);

            yield return null;
        }

        elapsed = 0f;

        // DESCIDA
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / (duration / 2f));

            objectTransform.position = Vector3.Lerp(peak, start, t);

            yield return null;
        }

        // Garante o final exato
        objectTransform.position = start;
    }
}