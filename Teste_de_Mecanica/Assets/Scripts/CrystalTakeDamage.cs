using UnityEngine;

public class CrystalTakeDamage : MonoBehaviour
{
    [Header("Vida do Cristal")]
    [SerializeField] private int vidaMaxima = 3;
    private int vidaAtual;

    [Header("Regra do Puzzle")]
    [Tooltip("Marque TRUE no cristal correto que revela a Princesa.")]
    [SerializeField] private bool eOCristalCorreto = false;

    [Header("NPC da Princesa (GameObject da Cena)")]
    [Tooltip("Arraste o GameObject da Princesa que já está colocado na cena e desativado.")]
    [SerializeField] private GameObject princesaNPC;

    [Header("Feedback e Som")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;
    [SerializeField] private ParticleSystem particulaDestruicao;
    [SerializeField] private string somDano = "Damage";
    [SerializeField] private string somQuebra = "CrystalBreak";

    private bool destruido = false;

    private void Start()
    {
        vidaAtual = vidaMaxima;

        if (feedbackDano == null)
            feedbackDano = GetComponent<ControladorFeedBackDano>();
    }

    public void ReceberDano(int dano)
    {
        if (destruido || dano <= 0)
            return;

        vidaAtual -= dano;

        if (!string.IsNullOrWhiteSpace(somDano))
            SoundEffectManager.Play(somDano);

        if (feedbackDano != null)
            feedbackDano.ExecutarFeedback();

        if (vidaAtual <= 0)
            QuebrarCristal();
    }

    private void QuebrarCristal()
    {
        if (destruido) return;
        destruido = true;

        if (!string.IsNullOrWhiteSpace(somQuebra))
            SoundEffectManager.Play(somQuebra);

        if (particulaDestruicao != null)
        {
            ParticleSystem efeito = Instantiate(particulaDestruicao, transform.position, Quaternion.identity);
            efeito.Play();
            Destroy(efeito.gameObject, 2f);
        }

        // Ativa o GameObject da Princesa já existente na cena
        if (eOCristalCorreto && princesaNPC != null)
        {
            princesaNPC.SetActive(true);
        }

        Destroy(gameObject);
    }
}