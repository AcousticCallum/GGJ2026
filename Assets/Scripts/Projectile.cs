using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;

    public Mask.MaskTeam maskTeam;

    public int damage;
    public float speed;

    public float duration;
    private float lifeTimer;

    private void Start()
    {
        TryGetComponent(out rb);
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * (Vector2)transform.up);
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        // Get Rigidbody2D of other object
        Rigidbody2D otherRb = collider2D.attachedRigidbody;

        if (!otherRb) return;

        // Check for Health component
        Health otherHealth = otherRb.GetComponent<Health>();

        if (!otherHealth) return;

        // Don't damage teammates
        if (otherHealth.GetMaskTeam() == maskTeam) return;

        // Deal damage
        otherHealth.TakeDamage(damage);

        // Destroy projectile
        Destroy(gameObject);
    }
}
