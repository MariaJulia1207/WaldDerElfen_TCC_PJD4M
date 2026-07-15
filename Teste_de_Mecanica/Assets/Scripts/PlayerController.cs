using UnityEngine; 
using UnityEngine.InputSystem; 
public class PlayerController : MonoBehaviour 
{ 
    [Header("Movimento")] 
    public float velocidade = 5f; 
    private Rigidbody2D rb; 
    private Vector2 movimento;
    private Vector2 ultimaDirecao;

    public GameObject attackUp;
    public GameObject attackDown;
    public GameObject attackLeft;
    public GameObject attackRight;

    private Animator anim;

    private bool atacando;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    } 
    void Update() 
    { 
        if(atacando)
{
    movimento = Vector2.zero;
    return;
}

        movimento = Vector2.zero; 
        // Esquerda
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            movimento.x = -1;
        } 
        // Direita
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            movimento.x = 1;
        } 
        // Cima
        if (Keyboard.current.upArrowKey.isPressed)
        {
            movimento.y = 1;
        } 
        // Baixo
        if (Keyboard.current.downArrowKey.isPressed)
        {
            movimento.y = -1;
        } 
        // Corrige velocidade diagonal
        movimento = movimento.normalized;

        if (movimento != Vector2.zero)
        {
            ultimaDirecao = movimento;
        }

        anim.SetFloat("MoveX", movimento.x);
        anim.SetFloat("MoveY", movimento.y);

        bool andando = movimento != Vector2.zero;

        anim.SetBool("IsMoving", andando);

        if(andando)
        {
            anim.SetFloat("LastMoveX", movimento.x);
            anim.SetFloat("LastMoveY", movimento.y);
        }

        if (movimento.x != 0)
{
    movimento.y = 0;
}

        // Atacar
        if (Keyboard.current.xKey.wasPressedThisFrame && !atacando)
{
    Atacar();
}
    }
    void FixedUpdate() 
    {
        rb.linearVelocity = movimento * velocidade;
    }

    void Atacar()
{
    atacando = true;

    anim.SetTrigger("Attack");
}
    void FinalizarAtaque()
{
    atacando = false;
}

    // Hitboxes
    void AtivarHitbox()
{
    float x = anim.GetFloat("LastMoveX");
    float y = anim.GetFloat("LastMoveY");

    attackUp.SetActive(false);
    attackDown.SetActive(false);
    attackLeft.SetActive(false);
    attackRight.SetActive(false);

    if (y > 0)
        attackUp.SetActive(true);

    else if (y < 0)
        attackDown.SetActive(true);

    else if (x < 0)
        attackLeft.SetActive(true);

    else
        attackRight.SetActive(true);
}

    void DesativarHitbox()
{
    attackUp.SetActive(false);
    attackDown.SetActive(false);
    attackLeft.SetActive(false);
    attackRight.SetActive(false);
}
    

}