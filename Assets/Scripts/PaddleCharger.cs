using UnityEngine;

public class PaddleCharger : MonoBehaviour
{
    public PlayerController playerController;
    public OrbHealth health;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            playerController.AddCharge(gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
            health.TakeDamage(25);
    }
}
