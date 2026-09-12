/*
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector2 movimento = rb.linearVelocity;

        bool estaMovendo = movimento.sqrMagnitude > 0.01f;

        anim.SetBool("IsMoving", estaMovendo);

        if (estaMovendo)
        {
            Vector2 direcao = movimento.normalized;

            anim.SetFloat("MoveX", direcao.x);
            anim.SetFloat("MoveY", direcao.y);

            anim.SetFloat("LastMoveX", direcao.x);
            anim.SetFloat("LastMoveY", direcao.y);
        }
    }

    public void DefinirAtacando(bool estado)
    {
        anim.SetBool("IsAttacking", estado);
    }
}
*/