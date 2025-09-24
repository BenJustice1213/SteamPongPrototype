using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public float initialSpeed = 10f;
    public float boostSpeed = 25f;
    public float aimAssistStrength = 0.1f;
    public PlayerController playerController;

    private Rigidbody2D rb;
    private Transform enemy;
    private Collider2D enemyCollider;
    private bool isPaddleBounce = false;

    private HashSet<GameObject> recentlyCharged = new HashSet<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GameObject.FindGameObjectWithTag("Enemy")?.transform;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (enemy != null)
        {
            enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemyCollider, true);
            }
        }

        if (rb.velocity == Vector2.zero)
        {
            rb.velocity = Random.insideUnitCircle.normalized * initialSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject obj = collision.gameObject;

        if (obj.CompareTag("Paddle1") || obj.CompareTag("Paddle2"))
        {
            Debug.Log($"Ball collided with {obj.tag}");

            if (!recentlyCharged.Contains(obj))
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    PlayerController controller = player.GetComponent<PlayerController>();
                    if (controller != null)
                    {
                        controller.AddCharge(obj);
                        recentlyCharged.Add(obj);
                        StartCoroutine(RemoveChargeCooldown(obj, 0.5f));
                    }
                }
            }
            ContactPoint2D contact = collision.GetContact(0);
            DeflectTowardsEnemy(contact.normal);
        }
        else if (obj.CompareTag("Shield"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            DeflectTowardsEnemy(contact.normal);
        }
        else if (obj.CompareTag("Boundary"))
        {
            Destroy(gameObject);
        }
        else
        {
            if (!isPaddleBounce)
            {
                rb.velocity = rb.velocity.normalized * initialSpeed;
            }
            else
            {
                isPaddleBounce = false;
            }
        }
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

        if (enemyCollider != null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemyCollider, false);
        }

        Vector2 directionToEnemy = GetDirectionToNearestEnemy();
        reflected = Vector2.Lerp(reflected, directionToEnemy, aimAssistStrength);

        rb.velocity = reflected * boostSpeed;

        Transform effect = transform.Find("DefaultEffect");
        if (effect != null)
        {
            effect.gameObject.SetActive(true);
        }

        gameObject.tag = "DefaultEffect";
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
            {
                orbHealth.TakeDamage(20);
            }
            Destroy(gameObject);
        }
    }
}
