using UnityEngine;

public class Mask : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Health health;

    public MaskTeam maskTeam;

    public Body body;

    public bool controller;

    public bool canAim;

    public StatBonus[] statBonuses;

    public int souls;

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

    public virtual void AddSouls(int amount)
    {
        // Remove if negative amount.
        if (amount < 0)
        {
            TryRemoveSouls(-amount, true);
            return;
        }

        souls += amount;
    }

    public virtual bool TryRemoveSouls(int amount, bool force = false, bool checkOnly = false)
    {
        if (amount < 0) return false;

        // Check if there are enough souls to remove
        if (!force && souls < amount) return false;

        // Remove souls if not just checking
        if (!checkOnly) souls = Mathf.Max(souls - amount, 0);

        return true;
    }

    public enum MaskTeam
    {
        Neutral,
        Friendly,
        Hostile
    }
}
