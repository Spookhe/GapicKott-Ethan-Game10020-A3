/* Ethan Gapic-Kott, 000923124 */

using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Transform target;
    public float speed = 10f;    // Movement speed
    public float damage = 15f;   // Damage dealt on hit

    void Update()
    {
        // Destroy projectile if target no player exists
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Homes towards the player
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.LookAt(target);
    }

    // Handles collision with the player
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health h = other.GetComponent<Health>();

            // Apply damage if player has health component (In demo video, removed this component for showcasing AI)
            if (h != null)
            {
                h.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}