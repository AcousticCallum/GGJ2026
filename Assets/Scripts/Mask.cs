using UnityEngine;

public class Mask : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Health health;

    public MaskTeam maskTeam;

    public Body body;

    public bool controller;

    public StatBonus[] statBonuses;

    protected virtual void Start()
    {
        // Try to get Rigidbody2D and Health components
        TryGetComponent(out rb);
        TryGetComponent(out health);

        // Override in subclasses
    }

    protected virtual void Update()
    {
        // Override in subclasses
    }

    public virtual void Remove()
    {
        body.RemoveMask(this);

        // Override in subclasses
    }

    public virtual void OnRemove()
    {
        controller = false;

        Destroy(gameObject);

        // Override in subclasses
    }

    public enum MaskTeam
    {
        Neutral,
        Friendly,
        Hostile
    }
}
