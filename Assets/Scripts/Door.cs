using UnityEngine;

public class Door : MonoBehaviour
{
    public float locks;

    public void Open()
    {
        if(locks > 1)
        {
            locks--;
            return;
        }

        gameObject.SetActive(false);
    }
}
