using System.Collections;
using UnityEngine;

public class BossPapoula : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] private BossArvore boss;

    [Header("Tempo")]
    [SerializeField] private float tempoAberta = 2f;
    [SerializeField] private float tempoFechada = 5f;

    [Header("Estado")]
    [SerializeField] private bool aberta = false;

    private Collider2D colisor;
    private Animator animator;

    private Coroutine rotinaPapoula;

    private bool desativada = false;

    private void Start()
    {
        colisor = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        FecharPapoula();

        if (boss != null)
        {
            rotinaPapoula = StartCoroutine(RotinaPapoula());
        }
    }

    private IEnumerator RotinaPapoula()
    {
        while (!desativada)
        {
            // =================================================
            // ABRIR
            // =================================================

            AbrirAnimacao();

            yield return new WaitForSeconds(tempoAberta);

            if (desativada)
                yield break;

            // =================================================
            // FECHAR
            // =================================================

            FecharAnimacao();

            yield return new WaitForSeconds(tempoFechada);
        }
    }

    // =========================================================
    // ANIMAÇÃO ABRINDO
    // =========================================================

    private void AbrirAnimacao()
    {
        if (animator != null)
            animator.SetBool("Abrir", true);

        // O Animation Event da animação
        // chama AbrirPapoula()
    }

    // =========================================================
    // ANIMAÇÃO FECHANDO
    // =========================================================

    private void FecharAnimacao()
    {
        if (animator != null)
        {
            animator.SetBool("Abrir", false);
            animator.SetBool("Fechar", true);
        }

        // O Animation Event da animação
        // chama FecharPapoula()
    }

    // =========================================================
    // ANIMATION EVENT
    // =========================================================

    public void AbrirPapoula()
    {
        if (desativada)
            return;

        aberta = true;

        if (colisor != null)
            colisor.enabled = true;

        if (animator != null)
            animator.SetBool("Fechar", false);

        Debug.Log("Papoula aberta!");
    }

    public void FecharPapoula()
    {
        aberta = false;

        if (colisor != null)
            colisor.enabled = false;

        Debug.Log("Papoula fechada!");
    }

    // =========================================================
    // DANO
    // =========================================================

    public void ReceberDano(int dano)
    {
        if (desativada)
            return;

        if (!aberta)
            return;

        if (boss == null)
            return;

        boss.ReceberDano(dano);
    }

    // =========================================================
    // MORTE DO BOSS
    // =========================================================

    public void Desativar()
    {
        desativada = true;
        aberta = false;

        if (rotinaPapoula != null)
            StopCoroutine(rotinaPapoula);

        if (colisor != null)
            colisor.enabled = false;

        gameObject.SetActive(false);
    }

    public bool EstaAberta()
    {
        return aberta;
    }
}