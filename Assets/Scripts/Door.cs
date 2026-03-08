using UnityEngine;

public class Door : MonoBehaviour
{
    public int locks;

    public string gameStateID;

    private void Start()
    {
        if (gameStateID != "")
        {
            locks = GameState.GetState(gameStateID) - 1;

            if (locks == 0) gameObject.SetActive(false);
        }
    }

    public void Open()
    {
        if (locks <= 0) return;

        locks--;

        if (gameStateID != "")
        {
            GameState.SetState(gameStateID, locks + 1);
        }

        if (locks == 0)
        {
            gameObject.SetActive(false);
        }
    }
}
