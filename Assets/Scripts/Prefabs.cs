using UnityEngine;

public class Prefabs : MonoBehaviour
{
    public static Prefabs instance;

    public Mask[] maskPrefabs;

    private void Awake()
    {
        Prefabs.instance = this;
    }
}
