using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float spinSpeed = 180f;
    public int maxCharge = 5;

    public Transform spriteTransform;
    public OrbHealth orbHealth;

    public GameObject paddle1;
    public GameObject paddle2;
    public GameObject shield;

    public GameObject healIconPaddle1;
    public GameObject healIconPaddle2;
    public GameObject spinIcon;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 rotateInput;
    private bool spinLeftHeld;
    private bool spinRightHeld;
    private bool healPressed;
    private bool doubleSpinPressed;

    public UnityEngine.UI.Image paddle1UIImage;
    public UnityEngine.UI.Image paddle2UIImage;

    private int paddle1Charge = 0;
    private int paddle2Charge = 0;

    private Vector2 movement;
    private Rigidbody2D rb;

    private bool isDoubleActive = false;

    private Sprite[] paddle1ChargeSprites = new Sprite[6];
    private Sprite[] paddle2ChargeSprites = new Sprite[6];

    public float attackCooldown;
    private bool isCooled = true;

    public void CooldownStart()
    {
        StartCoroutine(CooldownCoroutine());
    }
    IEnumerator CooldownCoroutine()
    {
        isCooled = false;
        Debug.LogError("Cooldown Started!");
        yield return new WaitForSeconds(attackCooldown);
        isCooled = true;
        Debug.LogError("Cooldown Ended!");
    }

    public bool IsPaddle1Charged => paddle1Charge >= maxCharge;
    public bool IsPaddle2Charged => paddle2Charge >= maxCharge;
    public bool IsDoubleReady => IsPaddle1Charged && IsPaddle2Charged;

    void Start()
    {
        inputActions = new PlayerInputActions();
        inputActions.Enable();

        // Movement
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // You don't have a Vector2 Rotate — these are buttons now:
        inputActions.Player.RotateCCW.performed += _ => spinLeftHeld = true;
        inputActions.Player.RotateCCW.canceled += _ => spinLeftHeld = false;

        inputActions.Player.RotateCW.performed += _ => spinRightHeld = true;
        inputActions.Player.RotateCW.canceled += _ => spinRightHeld = false;


        inputActions.Player.Heal.performed += _ => healPressed = true;
        inputActions.Player.DoubleSpin.performed += _ => doubleSpinPressed = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
        rb = GetComponent<Rigidbody2D>();

        // Ignore collisions between player and sprite children colliders
        Collider2D playerCol = GetComponent<Collider2D>();
        foreach (var col in spriteTransform.GetComponentsInChildren<Collider2D>())
            Physics2D.IgnoreCollision(playerCol, col);

        if (orbHealth == null)
        {
            orbHealth = GameObject.Find("Orb").GetComponent<OrbHealth>();
            if (orbHealth == null)
                Debug.LogError("OrbHealth component not found on Orb!");
        }

        Debug.Log($"Paddle1: {(paddle1 != null ? paddle1.name : "NULL")}");
        Debug.Log($"Paddle2: {(paddle2 != null ? paddle2.name : "NULL")}");

        // Load sprites for paddle1 charge 1-5
        for (int i = 1; i <= 5; i++)
        {
            Sprite loadedSprite = Resources.Load<Sprite>($"Paddle1Charges/charge{i}");
            if (loadedSprite != null)
                paddle1ChargeSprites[i] = loadedSprite;
            else
                Debug.LogWarning($"Failed to load Paddle1Charges/charge{i}");
        }
        // Assign default sprite from inspector as index 0 (already assigned)
        paddle1ChargeSprites[0] = paddle1UIImage.sprite;

        // Load sprites for paddle2 charge 1-5
        for (int i = 1; i <= 5; i++)
        {
            Sprite loadedSprite = Resources.Load<Sprite>($"Paddle2Charges/charge{i}");
            if (loadedSprite != null)
                paddle2ChargeSprites[i] = loadedSprite;
            else
                Debug.LogWarning($"Failed to load Paddle2Charges/charge{i}");
        }
        paddle2ChargeSprites[0] = paddle2UIImage.sprite;

        UpdateUI();
    }
    void FixedUpdate()
    {
        Vector2 movementDir = movement.normalized;
        Vector2 newPos = rb.position + movementDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
    void Update()
    {
        // Movement (overrides legacy Input.GetAxisRaw)
        transform.position += (Vector3)moveInput.normalized * moveSpeed * Time.deltaTime;
        if (spriteTransform != null)
            spriteTransform.position = transform.position;

        // Spinning
        if (spinLeftHeld || isDoubleActive)
            spriteTransform.Rotate(0, 0, spinSpeed * Time.deltaTime);
        else if (spinRightHeld)
            spriteTransform.Rotate(0, 0, -spinSpeed * Time.deltaTime);

        // Double Spin Activation
        if (doubleSpinPressed && IsDoubleReady && !isDoubleActive)
        {
            doubleSpinPressed = false;
            paddle1Charge = 0;
            paddle2Charge = 0;
            UpdateUI();
            orbHealth.Heal(75);
            StartCoroutine(ActivateDoubleSpin());
        }

        // Healing
        if (healPressed)
        {
            healPressed = false;

            paddle1Charge = Mathf.Clamp(paddle1Charge, 0, maxCharge);
            paddle2Charge = Mathf.Clamp(paddle2Charge, 0, maxCharge);

            if (IsPaddle1Charged && IsPaddle2Charged)
            {
                orbHealth.Heal(50);
                paddle1Charge = 0;
                paddle2Charge = 0;
            }
            else if (IsPaddle1Charged)
            {
                orbHealth.Heal(25);
                paddle1Charge = 0;
            }
            else if (IsPaddle2Charged)
            {
                orbHealth.Heal(25);
                paddle2Charge = 0;
            }

            UpdateUI();
        }
    }

    public void AddCharge(GameObject paddle)
    {
        if (isDoubleActive)
        {
            // Don't add charge while double spin is active
            return;
        }
        if (paddle.CompareTag("Paddle1"))
        {
            if (paddle1Charge < maxCharge)
            {
                paddle1Charge++;
                Debug.Log($"AddCharge called for Paddle1 - Current charge: {paddle1Charge} / {maxCharge}");
                UpdateUI();
            }
        }
        else if (paddle.CompareTag("Paddle2"))
        {
            if (paddle2Charge < maxCharge)
            {
                paddle2Charge++;
                Debug.Log($"AddCharge called for Paddle2 - Current charge: {paddle2Charge} / {maxCharge}");
                UpdateUI();
            }
        }
        else
        {
            Debug.Log($"AddCharge called with unknown paddle tag: {paddle.tag}");
        }
    }

    public void UpdateUI()
    {
        if (paddle1Charge < 0 || paddle1Charge > maxCharge)
            Debug.LogWarning($"paddle1Charge index out of range! ({paddle1Charge})");
        if (paddle2Charge < 0 || paddle2Charge > maxCharge)
            Debug.LogWarning($"paddle2Charge index out of range! ({paddle2Charge})");

        int p1 = Mathf.Clamp(paddle1Charge, 0, maxCharge);
        int p2 = Mathf.Clamp(paddle2Charge, 0, maxCharge);

        paddle1UIImage.sprite = paddle1ChargeSprites[p1];
        paddle2UIImage.sprite = paddle2ChargeSprites[p2];

        // Heal icons for individual paddles
        bool healthNotFull = orbHealth.currentHealth < orbHealth.maxHealth;

        if (IsPaddle1Charged && healthNotFull)
            healIconPaddle1?.SetActive(true);
        else
            healIconPaddle1?.SetActive(false);

        if (IsPaddle2Charged && healthNotFull)
            healIconPaddle2?.SetActive(true);
        else
            healIconPaddle2?.SetActive(false);

        // Spin icon when both are fully charged
        if (IsDoubleReady)
            spinIcon?.SetActive(true);
        else
            spinIcon?.SetActive(false);
    }

    IEnumerator ActivateDoubleSpin()
    {
        isCooled = true;
        isDoubleActive = true;
        shield.SetActive(true);

        float originalSpinSpeed = spinSpeed;
        spinSpeed *= 4f;

        yield return new WaitForSeconds(5f);

        spinSpeed = originalSpinSpeed;
        shield.SetActive(false);
        isDoubleActive = false;
        CooldownStart();
    }

}
