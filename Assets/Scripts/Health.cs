/* Ethan Gapic-Kott, 000923124 */

using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Called when damage is taken
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Returns health
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    // Handles object death
    void Die()
    {
        Destroy(gameObject);
    }
}