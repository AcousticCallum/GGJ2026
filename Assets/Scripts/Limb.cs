using UnityEngine;

public class Limb : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Health health;

    public Body body;
    private int index;

    public StatBonus[] statBonuses;

    public bool destroyed;

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

    public void SetIndex(int newIndex)
    {
        index = newIndex;
    }

    public void Add()
    {
        destroyed = false;

        // Reset health
        if (health) health.ResetHealth();
    }

    public void Remove()
    {
        destroyed = true;
    }

    public virtual void PrimaryAction()
    {
        // Override in subclasses
    }

    public virtual void SecondaryAction()
    {
        // Override in subclasses
    }

    public Mask.MaskTeam GetMaskTeam()
    {
        return body.GetMaskTeam();
    }
}

[System.Serializable]
public struct StatBonus
{
    public StatBonusType statBonusType;
    public float value;

    public enum StatBonusType
    {
        moveSpeed,
        moveAcceleration,
        rotateSpeed
    }
}