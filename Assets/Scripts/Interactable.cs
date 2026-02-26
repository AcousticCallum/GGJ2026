using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public static List<Interactable> allInteractables = new List<Interactable>();

    public UnityEvent onInteract;

    public bool destroyOnInteract;
    public bool disableOnInteract;

    protected virtual void Start()
    {
        allInteractables.Add(this);
    }

    public virtual void Interact()
    {
        onInteract.Invoke();

        if (disableOnInteract)
        {
            gameObject.SetActive(false);
        }

        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }
    }
}
