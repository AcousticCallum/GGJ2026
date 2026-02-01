using UnityEngine;

public class Limb : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Health health;

    public Body body;

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

    public void Add()
    {
        destroyed = false;

        gameObject.SetActive(true);

        // Reset health
        if (health) health.ResetHealth();

        OnAdd();
    }

    protected virtual void OnAdd()
    {
        // Override in subclasses
    }

    public void Remove()
    {
        destroyed = true;

        gameObject.SetActive(false);

        OnRemove();
    }

    protected virtual void OnRemove()
    {
        // Override in subclasses
    }

    public virtual void PrimaryAction()
    {
        // Override in subclasses
    }

    public virtual void PrimaryActionEnd()
    {
        // Override in subclasses
    }

    public virtual void SecondaryAction()
    {
        // Override in subclasses
    }

    public virtual void SecondaryActionEnd()
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