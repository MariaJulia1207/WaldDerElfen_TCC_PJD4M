using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 5f;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Sons")]
    [SerializeField] private float intervaloPasso = 0.42f;

    [Header("Hitboxes do Ataque")]
    [SerializeField] private GameObject attackUp;
    [SerializeField] private GameObject attackDown;
    [SerializeField] private GameObject attackLeft;
    [SerializeField] private GameObject attackRight;

    private Rigidbody2D rb;
    private Animator anim;
    private InputAction moveAction;
    private InputAction attackAction;

    public Animator Anim => anim;

    private Vector2 movimento;

    private bool atacando;
    private float proximoPassoTempo;

    // =========================================================
    // CONTROLE DO PLAYER
    // =========================================================

    private bool controlesAtivos = true;

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (playerInput != null)
        {
            moveAction = playerInput.actions.FindAction("Move");
            attackAction = playerInput.actions.FindAction("Attack");
        }
    }

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
        TocarSomDePasso();

        if (attackAction != null && attackAction.WasPressedThisFrame())
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

    public void IniciarRespawn()
    {
        controlesAtivos = false;
        movimento = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("IsMoving", false);
            anim.SetBool("IsDead", false);
            anim.ResetTrigger("Attack");
            anim.SetTrigger("Respawn");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void FinalizarRespawn()
    {
        controlesAtivos = true;
        movimento = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("IsMoving", false);
            anim.SetBool("IsDead", false);
            anim.Play("Idle");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // =========================================================
    // MOVIMENTO
    // =========================================================

    private void LerMovimento()
    {
        movimento = Vector2.zero;

        if (moveAction != null)
        {
            movimento = moveAction.ReadValue<Vector2>();
        }
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                movimento.x = -1f;
            }

            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                movimento.x = 1f;
            }

            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            {
                movimento.y = 1f;
            }

            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            {
                movimento.y = -1f;
            }
        }

        if (movimento.sqrMagnitude > 1f)
        {
            movimento = movimento.normalized;
        }
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

    private void TocarSomDePasso()
    {
        if (!controlesAtivos || atacando)
        {
            proximoPassoTempo = Time.time;
            return;
        }

        if (movimento == Vector2.zero)
        {
            proximoPassoTempo = Time.time;
            return;
        }

        if (Time.time >= proximoPassoTempo)
        {
            SoundEffectManager.Play("Walk");
            proximoPassoTempo = Time.time + intervaloPasso;
        }
    }

    private void Atacar()
    {
        atacando = true;
        SoundEffectManager.Play("Sword");

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