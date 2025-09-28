using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [Header("Speed Settings")]
    public float initialSpeed = 10f;
    public float boostSpeed = 25f;
    public float aimAssistStrength = 0.1f;

    [Header("Damage Settings")]
    public int defaultDamage = 20;
    public int fireDamage = 40;

    [Header("Effects")]
    public GameObject defaultEffect;
    public GameObject fireEffect;

    public PlayerController playerController;

    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private bool isPaddleBounce = false;
    private bool isFireBall = false;
    public bool IsFireBall => isFireBall;


    private HashSet<GameObject> recentlyCharged = new HashSet<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (defaultEffect != null) defaultEffect.SetActive(false);
        if (fireEffect != null) fireEffect.SetActive(false);

        if (rb.velocity == Vector2.zero)
            rb.velocity = Random.insideUnitCircle.normalized * initialSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject obj = collision.gameObject;

        bool isFirePaddleHit = obj.CompareTag("FirePaddle");

        if (obj.CompareTag("Paddle1") || obj.CompareTag("Paddle2") || isFirePaddleHit)
        {
            Debug.Log($"Ball hit {obj.name}, tag: {obj.tag}");

            if (isFirePaddleHit)
            {
                isFireBall = true;
                if (fireEffect != null) fireEffect.SetActive(true);
                if (defaultEffect != null) defaultEffect.SetActive(false);
            }
            else
            {
                if (!isFireBall)
                {
                    if (defaultEffect != null) defaultEffect.SetActive(true);
                    if (fireEffect != null) fireEffect.SetActive(false);
                }
            }

            if (!recentlyCharged.Contains(obj) && playerController != null)
            {
                playerController.AddCharge(obj);
                recentlyCharged.Add(obj);
                StartCoroutine(RemoveChargeCooldown(obj, 0.5f));
            }

            ContactPoint2D contact = collision.GetContact(0);
            DeflectTowardsEnemy(contact.normal);
            return;
        }

        if (obj.CompareTag("Enemy"))
        {
            Enemy enemyHealth = obj.GetComponent<Enemy>();
            if (enemyHealth != null)
            {
                int damage = isFireBall ? fireDamage : defaultDamage;
                enemyHealth.TakeDamage(damage);
            }

            ContactPoint2D contact = collision.GetContact(0);
            DeflectTowardsEnemy(contact.normal);
            return;
        }

        if (obj.CompareTag("Shield"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            DeflectTowardsEnemy(contact.normal);
            return;
        }

        if (obj.CompareTag("Boundary"))
        {
            Destroy(gameObject);
            return;
        }

        if (!isPaddleBounce)
            rb.velocity = rb.velocity.normalized * initialSpeed;
        else
            isPaddleBounce = false;
    }

    IEnumerator RemoveChargeCooldown(GameObject paddle, float delay)
    {
        yield return new WaitForSeconds(delay);
        recentlyCharged.Remove(paddle);
    }

    void DeflectTowardsEnemy(Vector2 collisionNormal)
    {
        Vector2 incoming = rb.velocity.normalized;
        Vector2 reflected = Vector2.Reflect(incoming, collisionNormal).normalized;

        isPaddleBounce = true;

        Vector2 directionToEnemy = GetDirectionToNearestEnemy();
        reflected = Vector2.Lerp(reflected, directionToEnemy, aimAssistStrength);

        rb.velocity = reflected * boostSpeed;

        if (!isFireBall && defaultEffect != null) defaultEffect.SetActive(true);
    }

    Vector2 GetDirectionToNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return rb.velocity.normalized;

        GameObject closest = enemies[0];
        float closestDist = Vector2.Distance(transform.position, closest.transform.position);

        foreach (GameObject e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < closestDist)
            {
                closest = e;
                closestDist = dist;
            }
        }

        return (closest.transform.position - transform.position).normalized;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Orb"))
        {
            OrbHealth orbHealth = collision.GetComponent<OrbHealth>();
            if (orbHealth != null)
                orbHealth.TakeDamage(defaultDamage);

            Destroy(gameObject);
        }
    }
}
