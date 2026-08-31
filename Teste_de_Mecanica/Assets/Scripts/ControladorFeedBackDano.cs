using UnityEngine;

public class ControladorFeedBackDano : MonoBehaviour
{
    [Header("Feedback de Dano")]
    [SerializeField] private ParticleSystem particulaDano;

    public void ExecutarFeedback()
    {
        if (particulaDano == null)
            return;

        ParticleSystem efeito = Instantiate(
            particulaDano,
            transform.position,
            Quaternion.identity
        );

        efeito.Play();
    }
}
