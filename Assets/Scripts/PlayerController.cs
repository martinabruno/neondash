using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Parametri Movimento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 13f;

    [Header("Controllo Terreno (Grounded)")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private bool isGrounded;
    private float horizontalInput;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Lettura input orizzontale: combina tastiera fisica e pulsanti touch a schermo
        float keyboardInput = Input.GetAxisRaw("Horizontal");
        float touchInput = (UIManager.Instance != null) ? UIManager.Instance.MoveInput : 0f;

        horizontalInput = Mathf.Abs(touchInput) > 0.1f ? touchInput : keyboardInput;

        // RILEVAMENTO SALTO A SINGOLO IMPULSO (Impedisce il volo continuo)
        bool jumpPressed = Input.GetButtonDown("Jump");
        
        if (UIManager.Instance != null && UIManager.Instance.JumpRequested)
        {
            jumpPressed = true;
            UIManager.Instance.JumpRequested = false; // "Consuma" subito il tocco così non si ripete all'infinito
        }

        if (jumpPressed)
        {
            jumpRequested = true;
        }


        // Orientamento sprite (flip a destra/sinistra in base alla direzione)
        if (horizontalInput > 0.1f)
        {
            sr.flipX = true;
        }
        else if (horizontalInput < -0.1f)
        {
            sr.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        // Verifica se il robottino tocca terra
        Vector2 checkPos = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position;
        isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);

        // Movimento orizzontale
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Esecuzione salto
        if (jumpRequested)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                if (RetroAudio.Instance != null) RetroAudio.Instance.PlayJump();
            }
            jumpRequested = false;
        }

        // Controllo caduta nel vuoto
        if (transform.position.y < -5f)
        {
            if (GameManager.Instance != null) GameManager.Instance.OnPlayerDied();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            if (RetroAudio.Instance != null) RetroAudio.Instance.PlayCoin();
            if (GameManager.Instance != null) GameManager.Instance.OnCoinCollected();
        }
        else if (other.CompareTag("Trap")) // Aggiunto controllo per le spine
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDied();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}