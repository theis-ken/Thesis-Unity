using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    void Update()
    {
        float time = GameManager.Instance != null ? GameManager.Instance.GetTimer() : 0f;
        timerText.text = time.ToString();
    }
}
