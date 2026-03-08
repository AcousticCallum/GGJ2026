using System.Collections.Generic;
using UnityEngine;

public class Shrine : MonoBehaviour
{
    public static List<Shrine> allShrines = new();

    public GameObject activeShrineObject;

    public Vector2 spawnPosition;
    public float spawnRotation;

    public int shrineID;

    private void Awake()
    {
        allShrines.RemoveAll(s => s == null);

        allShrines.Add(this);
    }

    private void Start()
    {
        if (GameState.GetState("Shrine") == shrineID)
        {
            activeShrineObject.SetActive(true);
        }
    }

    public void Activate()
    {
        GameState.SetState("Shrine", shrineID);

        activeShrineObject.SetActive(true);
    }

    public Vector2 GetSpawnPosition()
    {
        return (Vector2)transform.position + (Vector2)transform.TransformVector(spawnPosition);
    }

    public float GetSpawnRotation()
    {
        return Vector2.SignedAngle(Vector2.up, transform.up) + spawnRotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector2 spawnPos = GetSpawnPosition();
        Gizmos.DrawSphere(spawnPos, 0.25f);

        Vector2 rotationVector = Quaternion.Euler(0, 0, GetSpawnRotation()) * Vector2.right;
        Gizmos.DrawLine(spawnPos, spawnPos + rotationVector);
    }
}
