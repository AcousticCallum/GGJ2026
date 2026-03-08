using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public static List<Interactable> allInteractables = new List<Interactable>();

    public UnityEvent onInteract;

    public int addSouls;

    public bool destroyOnInteract;
    public bool disableOnInteract;

    public string gameStateID;

    protected virtual void Start()
    {
        allInteractables.Add(this);

        if (gameStateID != "")
        {
            if (GameState.GetState(gameStateID) == 1)
            {
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
    }

    public virtual void Interact()
    {
        onInteract.Invoke();

        if (gameStateID != "")
        {
            GameState.SetState(gameStateID, 1);
        }

        if (addSouls != 0)
        {
            if (PlayerMask.instance) PlayerMask.instance.AddSouls(addSouls);
        }

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
