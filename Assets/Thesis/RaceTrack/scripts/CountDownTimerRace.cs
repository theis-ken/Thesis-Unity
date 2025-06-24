using UnityEngine;
using TMPro;

public class CountDownTimerRace : MonoBehaviour
{
    public static CountDownTimerRace Instance;

    public TextMeshProUGUI countdownText;
    public float countdownDuration = 30f;

    private float currentTime;
    private bool raceRunning = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!raceRunning) return;

        currentTime -= Time.deltaTime;

        if (countdownText != null)
            countdownText.text = Mathf.Ceil(currentTime).ToString();

        if (currentTime <= 0)
        {
            countdownText.text = "0";
            raceRunning = false;
        }
    }

    public void ResetTimer()
    {
        currentTime = countdownDuration;
        raceRunning = true;
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }
}
