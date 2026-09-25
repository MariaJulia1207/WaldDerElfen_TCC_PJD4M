using System.Collections;
using UnityEngine;

public class BossArvore : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 5;
    private int vida;

    [Header("Referências")]
    [SerializeField] private Animator animator;
    [SerializeField] private BossPapoula papoula;

    [Header("Feedback de Dano")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;

    [Header("Morte")]
    [SerializeField] private ParticleSystem particulasMorte;
    [SerializeField] private float tempoAteParticulas = 1.5f;
    [SerializeField] private float tempoDoEfeitoMorte = 2f;

    private bool morreu = false;

    private void Start()
    {
        vida = vidaMaxima;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void ReceberDano(int dano)
    {
        if (morreu)
            return;

        // Só recebe dano se a papoula estiver aberta
        if (papoula != null && !papoula.EstaAberta())
            return;

        vida -= dano;

        Debug.Log("Boss Árvore recebeu dano. Vida: " + vida);

        // Feedback de dano
        if (feedbackDano != null)
            feedbackDano.ExecutarFeedback();

        if (vida <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        if (morreu)
            return;

        morreu = true;

        // Impede novos danos
        if (papoula != null)
            papoula.Desativar();

        // Animação de morte
        if (animator != null)
            animator.SetTrigger("Morrer");

        StartCoroutine(SequenciaMorte());
    }

    private IEnumerator SequenciaMorte()
    {
        // Espera um pouco enquanto a animação de morte acontece
        yield return new WaitForSeconds(tempoAteParticulas);

        // Cria o efeito de morte
        if (particulasMorte != null)
        {
            ParticleSystem efeito = Instantiate(
                particulasMorte,
                transform.position,
                Quaternion.identity
            );

            efeito.Play();

            Destroy(
                efeito.gameObject,
                tempoDoEfeitoMorte
            );
        }

        // Espera o efeito terminar
        yield return new WaitForSeconds(tempoDoEfeitoMorte);

        // Destrói a árvore
        // A BossArena vai detectar isso através de objetoTeste == null
        Destroy(gameObject);
    }

    public bool EstaMorta()
    {
        return morreu;
    }
}