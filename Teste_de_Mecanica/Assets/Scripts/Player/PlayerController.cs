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

    // =========================================================
    // CONTROLE DO PLAYER
    // =========================================================

    private bool controlesAtivos = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        DesativarTodasHitboxes();
    }

    private void Update()
    {
        // =====================================================
        // CONTROLES DESATIVADOS
        // =====================================================

        if (!controlesAtivos)
        {
            movimento = Vector2.zero;

            anim.SetBool("IsMoving", false);

            return;
        }

        // =====================================================
        // ATAQUE
        // =====================================================

        if (atacando)
        {
            movimento = Vector2.zero;
            anim.SetBool("IsMoving", false);

            return;
        }

        // =====================================================
        // MOVIMENTO
        // =====================================================

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
        if (!controlesAtivos)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = movimento * velocidade;
    }

    // =========================================================
    // TIMELINE - CONTROLE DO PLAYER
    // =========================================================

    public void DisableControls()
    {
        controlesAtivos = false;

        movimento = Vector2.zero;

        // Para imediatamente o Player
        rb.linearVelocity = Vector2.zero;

        // Impede que a animação de caminhada continue
        anim.SetBool("IsMoving", false);

        // Garante que nenhuma hitbox fique ativa
        DesativarTodasHitboxes();

        // Cancela estado de ataque
        atacando = false;
    }

    public void EnableControls()
    {
        controlesAtivos = true;

        movimento = Vector2.zero;

        rb.linearVelocity = Vector2.zero;
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