using UnityEngine;

using UnityEngine.UI;

using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Awake()
    {
        if (UIManager.instance != null && UIManager.instance != this)
        {
            Destroy(gameObject);

            return;
        }

        UIManager.instance = this;
    }

    private void LateUpdate()
    {
        PlayerMask playerMask = PlayerMask.instance;
        if (playerMask)
        {
            if (playerMask.bodyTimerActive)
            {
                timerText.text = TimeSpan.FromSeconds(PlayerMask.instance.bodyTimer).ToString("ss\\.ff");
                timerText.gameObject.SetActive(true);
            }
            else
            {
                timerText.text = "";
                timerText.gameObject.SetActive(false);
            }

            scoreText.text = "Kills: " + PlayerMask.kills;
        }
    }
}
