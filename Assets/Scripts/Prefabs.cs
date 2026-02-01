using UnityEngine;

public class Prefabs : MonoBehaviour
{
    public static Prefabs instance;

    public Mask[] maskPrefabs;

    public Body[] enemyPrefabs;

    private void Awake()
    {
        Prefabs.instance = this;
    }
}
