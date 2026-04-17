/* Ethan Gapic-Kott, 000923124 */

using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 9f;
    public float mouseSensitivity = 2f;

    [Header("Combat")]
    public Transform sword;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.5f;

    CharacterController controller;
    float yRotation = 0f;
    float xRotation = 0f;

    Health health;
    bool canAttack = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<Health>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Look();
        Move();
        Attack();
    }

    // Handles mouse look for player and camera
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    // Handles WASD movement and sprinting
    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        Vector3 move = transform.forward * v + transform.right * h;
        controller.Move(move * speed * Time.deltaTime);
    }

    // Attack when clicking left mouse
    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(DoAttack());
        }
    }

    // Handles attack timing and damage
    IEnumerator DoAttack()
    {
        canAttack = false;

        // Play sword animation from SwordSlash.cs
        SwordSlash slash = sword.GetComponent<SwordSlash>();
        if (slash != null)
            slash.PlaySlash();

        yield return new WaitForSeconds(0.1f);
        Collider[] hits = Physics.OverlapSphere(
            sword.position,
            attackRange,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            Health h = hit.GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}