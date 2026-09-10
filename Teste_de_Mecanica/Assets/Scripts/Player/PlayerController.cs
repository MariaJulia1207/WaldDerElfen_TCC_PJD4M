using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 5f;

    [Header("Hitboxes do Ataque")]
    [SerializeField] private GameObject attackUp;
    [SerializeField] private GameObject attackDown;
    [SerializeField] private GameObject attackLeft;
    [SerializeField] private GameObject attackRight;

    private Rigidbody2D rb;
    private Animator anim;

    public Animator Anim => anim;

    private Vector2 movimento;

    private bool atacando;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        DesativarTodasHitboxes();
    }

    private void Update()
    {
        // Se estiver atacando, não pode andar
        if (atacando)
        {
            movimento = Vector2.zero;
            anim.SetBool("IsMoving", false);

            return;
        }

        LerMovimento();
        AtualizarAnimacao();

        // Ataque com X
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            Atacar();
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movimento * velocidade;
    }

    // =========================================================
    // MOVIMENTO
    // =========================================================

    private void LerMovimento()
    {
        movimento = Vector2.zero;

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            movimento.x = -1;
        }

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            movimento.x = 1;
        }

        if (Keyboard.current.upArrowKey.isPressed)
        {
            movimento.y = 1;
        }

        if (Keyboard.current.downArrowKey.isPressed)
        {
            movimento.y = -1;
        }

        movimento = movimento.normalized;
    }

    // =========================================================
    // ANIMAÇÃO DE MOVIMENTO
    // =========================================================

    private void AtualizarAnimacao()
    {
        bool andando = movimento != Vector2.zero;

        anim.SetBool("IsMoving", andando);

        anim.SetFloat("MoveX", movimento.x);
        anim.SetFloat("MoveY", movimento.y);

        if (andando)
        {
            anim.SetFloat("LastMoveX", movimento.x);
            anim.SetFloat("LastMoveY", movimento.y);
        }
    }

    // =========================================================
    // ATAQUE
    // =========================================================

    private void Atacar()
    {
        atacando = true;

        movimento = Vector2.zero;

        float x = anim.GetFloat("LastMoveX");
        float y = anim.GetFloat("LastMoveY");

        int direcaoAtaque;

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            if (x < 0)
            {
                direcaoAtaque = 2; // Esquerda
            }
            else
            {
                direcaoAtaque = 3; // Direita
            }
        }
        else
        {
            if (y < 0)
            {
                direcaoAtaque = 0; // Baixo
            }
            else
            {
                direcaoAtaque = 1; // Cima
            }
        }

        anim.SetInteger("AttackDirection", direcaoAtaque);

        anim.SetTrigger("Attack");
    }

    // =========================================================
    // HITBOX
    // =========================================================

    public void AtivarHitbox()
    {
        DesativarTodasHitboxes();

        float x = anim.GetFloat("LastMoveX");
        float y = anim.GetFloat("LastMoveY");

        GameObject hitboxSelecionada = null;

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            if (x < 0)
            {
                hitboxSelecionada = attackLeft;
            }
            else
            {
                hitboxSelecionada = attackRight;
            }
        }
        else
        {
            if (y < 0)
            {
                hitboxSelecionada = attackDown;
            }
            else
            {
                hitboxSelecionada = attackUp;
            }
        }

        if (hitboxSelecionada != null)
        {
            hitboxSelecionada.SetActive(true);

            AttackHitbox hitbox =
                hitboxSelecionada.GetComponent<AttackHitbox>();

            if (hitbox != null)
            {
                hitbox.VerificarAcerto();
            }
        }
    }

    public void DesativarHitbox()
    {
        DesativarTodasHitboxes();
    }

    private void DesativarTodasHitboxes()
    {
        if (attackUp != null)
            attackUp.SetActive(false);

        if (attackDown != null)
            attackDown.SetActive(false);

        if (attackLeft != null)
            attackLeft.SetActive(false);

        if (attackRight != null)
            attackRight.SetActive(false);
    }

    // =========================================================
    // FINAL DO ATAQUE
    // =========================================================

    public void FinalizarAtaque()
    {
        DesativarTodasHitboxes();

        atacando = false;
    }

}