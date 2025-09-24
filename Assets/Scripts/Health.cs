using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OrbHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Image heartUI;
    public Sprite heart100, heart75, heart50, heart25;

    private PlayerController controller;

    void Start()
    {
        currentHealth = maxHealth;

        // Correctly find the PlayerController in the scene
        controller = FindObjectOfType<PlayerController>();

        if (controller == null)
            Debug.LogWarning("PlayerController not found by OrbHealth.");

        UpdateHearts();
        controller?.UpdateUI(); // Sync UI at start
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();
        controller?.UpdateUI(); // Update heal/spin icons

        if (currentHealth <= 0)
        {
            Debug.Log("Health reached 0 - restart or trigger game over here");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("GameOver");
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();
        controller?.UpdateUI(); // Update heal/spin icons
    }

    void UpdateHearts()
    {
        if (currentHealth >= 100)
            heartUI.sprite = heart100;
        else if (currentHealth >= 75)
            heartUI.sprite = heart75;
        else if (currentHealth >= 50)
            heartUI.sprite = heart50;
        else if (currentHealth >= 25)
            heartUI.sprite = heart25;
    }
}
