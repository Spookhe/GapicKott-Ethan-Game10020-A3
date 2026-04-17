/* Ethan Gapic-Kott, 000923124 */

using UnityEngine;
using UnityEngine.AI;

public class SkeletonAI : MonoBehaviour
{
    enum State { Rising, Patrol, Attack }

    [Header("References")]
    public Transform player;
    public NecromancerAI necromancer;

    [Header("Animation")]
    public Animator animator;

    [Header("Combat")]
    public float attackRange = 2f;
    public float damage = 10f;
    public float attackCooldown = 1.5f;
    float lastAttack;

    [Header("Vision")]
    public float viewRadius = 10f;

    [Header("Patrol Settings")]
    public float orbitRadius = 3f;     // Distance from necromancer
    public float orbitSpeed = 1f;      // How fast the skeleton patrols around necromancer

    NavMeshAgent agent;
    State state;
    float orbitAngle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        orbitAngle = Random.Range(0f, 360f);

        state = State.Rising;

        if (animator != null)
            animator.SetTrigger("Rise");

        Invoke(nameof(FinishRising), 2f);
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Attack:
                Attack();
                break;
        }
    }

    void FinishRising()
    {
        state = State.Patrol;
    }

    void Patrol()
    {
        if (necromancer == null) return;

        // Moves around necromancer in a circle
        orbitAngle += orbitSpeed * Time.deltaTime * 50f;

        Vector3 offset = new Vector3(
            Mathf.Cos(orbitAngle * Mathf.Deg2Rad),
            0,
            Mathf.Sin(orbitAngle * Mathf.Deg2Rad)
        ) * orbitRadius;

        Vector3 targetPos = necromancer.transform.position + offset;

        agent.SetDestination(targetPos);

        if (animator != null)
            animator.SetBool("IsMoving", true);

        // Chases if player is close
        if (Vector3.Distance(transform.position, player.position) < viewRadius)
        {
            state = State.Attack;
        }
    }

    void Attack()
    {
        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);

        if (animator != null)
            animator.SetBool("IsMoving", dist > attackRange);

        if (dist <= attackRange && Time.time > lastAttack + attackCooldown)
        {
            if (animator != null)
                animator.SetTrigger("Attack");

            Health h = player.GetComponent<Health>();
            if (h != null)
                h.TakeDamage(damage);

            lastAttack = Time.time;
        }

        // Return to patrol when player escapes (Out of vision cone)
        if (dist > viewRadius)
        {
            state = State.Patrol;
        }
    }
}