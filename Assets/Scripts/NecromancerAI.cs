/* Ethan Gapic-Kott, 000923124 */

using UnityEngine;
using System.Collections;

public class NecromancerAI : MonoBehaviour
{
    enum State { Ritual, Alert, Combat }

    public Transform player;
    public Transform castPoint;

    public GameObject projectilePrefab;
    public GameObject skeletonPrefab;

    public Animator animator;

    public float maxHealth = 300f;
    float currentHealth;

    public float maxMana = 100f;
    public float currentMana = 100f;
    public float raiseCost = 40f;

    public float attackCooldown = 2f;
    float lastAttackTime;

    public float summonCooldown = 5f;
    float lastSummonTime;

    GameObject currentSkeleton;

    public float viewRadius = 18f;
    public float viewAngle = 120f;

    State state;

    bool attackQueued;
    bool attackSpawnedThisCycle;

    void Start()
    {
        currentHealth = maxHealth;
        state = State.Ritual;
    }

    void Update()
    {
        // Check if player is detected by necromancer
        bool playerSeenByNecro = CanSeePlayer();
        bool playerSeenBySkeleton = currentSkeleton != null && SkeletonSeesPlayer();

        bool playerDetected = playerSeenByNecro || playerSeenBySkeleton;

        // Handles FSM states for the necromancer
        switch (state)
        {
            case State.Ritual:
                Ritual(playerDetected);
                break;

            case State.Alert:
                Alert(playerDetected);
                break;

            case State.Combat:
                Combat(playerDetected);
                break;
        }

        HandleAttackSpawn();
    }

    // Idle state until the player is detected
    void Ritual(bool detected)
    {
        if (animator != null)
            animator.SetBool("InCombat", false);

        if (detected)
        {
            state = State.Alert;
        }
    }

    // Transitions state before combat
    void Alert(bool detected)
    {
        transform.LookAt(player);

        if (animator != null)
            animator.SetBool("InCombat", true);

        if (!detected)
        {
            state = State.Ritual;
            return;
        }

        state = State.Combat;
    }

    // When vision cone detects the player
    void Combat(bool detected)
    {
        transform.LookAt(player);

        if (animator != null)
            animator.SetBool("InCombat", true);

        // Summon a skeleton if none exist
        if (currentSkeleton == null)
            TrySummon();

        // Attacks the player by launching projectiles
        if (detected && Time.time > lastAttackTime + attackCooldown)
        {
            attackQueued = true;
            attackSpawnedThisCycle = false;

            animator.SetTrigger("Attack");
            StartCoroutine(AttackDelay());

            lastAttackTime = Time.time;
        }

        // Returns to idle state if player is out of vision cone
        if (!detected)
            state = State.Ritual;
    }

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(0.5f);
        attackQueued = false;
    }

    // Spawns a projectile during attack animation
    void HandleAttackSpawn()
    {
        if (!attackQueued || attackSpawnedThisCycle) return;
        if (animator == null || player == null || castPoint == null) return;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName("Attack")) return;

        GameObject proj = Instantiate(
            projectilePrefab,
            castPoint.position,
            Quaternion.identity
        );

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
            p.target = player;

        attackSpawnedThisCycle = true;
    }

    // Attempts to summon skeleton if no skeletons exist
    void TrySummon()
    {
        if (Time.time < lastSummonTime + summonCooldown) return;
        if (currentMana < raiseCost) return;

        StartCoroutine(SummonRoutine());

        currentMana -= raiseCost;
        lastSummonTime = Time.time;
    }

    IEnumerator SummonRoutine()
    {
        if (animator != null)
            animator.SetTrigger("Summon");

        yield return new WaitForSeconds(1.2f);

        currentSkeleton = Instantiate(
            skeletonPrefab,
            transform.position + transform.forward * 2,
            Quaternion.identity
        );

        SkeletonAI sk = currentSkeleton.GetComponent<SkeletonAI>();
        sk.player = player;
        sk.necromancer = this;
    }

    // Vision cone handler
    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 dir = (player.position - origin).normalized;

        if (Vector3.Distance(origin, player.position) > viewRadius)
            return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle / 2f)
            return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, viewRadius))
            return hit.transform == player;

        return false;
    }

    // Skeleton vision cone allows necromancer to extend its own vision cone
    bool SkeletonSeesPlayer()
    {
        float dist = Vector3.Distance(currentSkeleton.transform.position, player.position);
        return dist <= viewRadius;
    }
}