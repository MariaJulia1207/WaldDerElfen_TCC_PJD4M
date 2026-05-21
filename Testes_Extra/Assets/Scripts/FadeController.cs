using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float velocidadeFade = 2f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        Color cor = fadeImage.color;

        while (cor.a > 0)
        {
            cor.a -= velocidadeFade * Time.deltaTime;
            fadeImage.color = cor;

            yield return null;
        }

        cor.a = 0;
        fadeImage.color = cor;
    }

    public IEnumerator FadeOut()
    {
        Color cor = fadeImage.color;

        while (cor.a < 1)
        {
            cor.a += velocidadeFade * Time.deltaTime;
            fadeImage.color = cor;

            yield return null;
        }

        cor.a = 1;
        fadeImage.color = cor;
    }
}