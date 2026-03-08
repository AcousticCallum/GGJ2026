using System.Collections.Generic;
using UnityEngine;

public static class GameState
{
    private static Dictionary<string, int> data = new Dictionary<string, int>();

    public static void SetState(string key, int value)
    {
        if (data.ContainsKey(key))
        {
            data[key] = value;

            return;
        }

        data.Add(key, value);
    }

    public static int GetState(string key)
    {
        if (data.ContainsKey(key)) return data[key];

        return 0; // Default value if key is not found
    }
}
