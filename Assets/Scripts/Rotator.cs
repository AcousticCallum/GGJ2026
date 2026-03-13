using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed;
    public float multiplier = 1.0f;

    public AnimationCurve multiplierOverTime;
    public float multiplierOverTimePeriod = 1.0f;
    public bool loopMultiplierOverTime = true;
    private float multiplierOverTimeTimer;


    void Update()
    {
        // Update the multiplier over time
        multiplierOverTimeTimer += Time.deltaTime / multiplierOverTimePeriod;
        if (multiplierOverTimeTimer > 1.0)
        {
            if (loopMultiplierOverTime)
            {
                multiplierOverTimeTimer -= 1.0f; // Loop back to the start of the curve
            }
            else
            {
                multiplierOverTimeTimer = 1.0f; // Clamp to the end of the curve
            }
        }

        // Get the current multiplier from the curve
        float currentMultiplier = multiplier * multiplierOverTime.Evaluate(multiplierOverTimeTimer);

        // Rotate the object around its Y-axis
        transform.Rotate(Vector3.forward, rotationSpeed * currentMultiplier * Time.deltaTime);
    }
}
