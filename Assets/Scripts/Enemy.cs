using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float wanderSpeed = 2f;
    public float fleeSpeed = 3f;
    public float detectionRange = 10f;
    public float shootCooldown = 2f;
    public GameObject ballPrefab;
    public Transform shootPoint;
    public float ballSpeed = 12f;

    public int maxHealth = 100;
    private int currentHealth;

    public SpriteRenderer spriteRenderer;
    public Sprite health100;
    public Sprite health75;
    public Sprite health50;
    public Sprite health25;
    public Sprite health10;

    private Transform player;
    private Vector2 wanderDirection;
    private float directionChangeInterval = 2f;
    private float directionTimer;
    private float shootTimer;

    private bool isFleeing = false;

    public delegate void EnemyDestroyedHandler();
    public event EnemyDestroyedHandler OnDestroyEvent;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
        UpdateSprite();
        PickRandomDirection();
    }

    void Update()
    {
        FacePlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            isFleeing = true;
            FleeFromPlayer();

            shootTimer += Time.deltaTime;
            if (shootTimer >= shootCooldown)
            {
                ShootAtPlayer();
                shootTimer = 0f;
            }
        }
        else
        {
            if (isFleeing)
            {
                isFleeing = false;
                PickRandomDirection(); // Reset wander direction when resuming
                directionTimer = 0f;
            }

            Wander();
        }
    }

    void FacePlayer()
    {
        if (!player) return;

        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust depending on your sprite's forward direction (usually up)
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }


    void Wander()
    {
        directionTimer += Time.deltaTime;
        if (directionTimer >= directionChangeInterval)
        {
            PickRandomDirection();
            directionTimer = 0f;
        }

        transform.Translate(wanderDirection * wanderSpeed * Time.deltaTime);
    }

    void FleeFromPlayer()
    {
        Vector2 directionAway = (transform.position - player.position).normalized;
        transform.Translate(directionAway * fleeSpeed * Time.deltaTime);
    }

    void PickRandomDirection()
    {
        wanderDirection = Random.insideUnitCircle.normalized;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boundary"))
        {
            wanderDirection = -wanderDirection;
        }
        else if (collision.gameObject.CompareTag("DefaultEffect"))
        {
            TakeDamage(30);
            Destroy(collision.gameObject);
        }
    }

    void ShootAtPlayer()
    {
        if (!player) return;

        Vector2 shootPos = shootPoint.position;
        Vector2 direction = ((Vector2)player.position - shootPos).normalized;

        GameObject ball = Instantiate(ballPrefab, shootPos, Quaternion.identity);
        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        rb.velocity = direction * ballSpeed;
    }

    void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateSprite();

        if (currentHealth <= 0)
        {
            OnDestroyEvent?.Invoke();
            ScoreManager.Instance?.AddScore(100);
            Destroy(gameObject);
        }
    }

    void UpdateSprite()
    {
        if (currentHealth > 75)
            spriteRenderer.sprite = health100;
        else if (currentHealth > 50)
            spriteRenderer.sprite = health75;
        else if (currentHealth > 25)
            spriteRenderer.sprite = health50;
        else if (currentHealth > 10)
            spriteRenderer.sprite = health25;
        else
            spriteRenderer.sprite = health10;
    }
}
